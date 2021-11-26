

using UnityEngine;
using UnityEngine.Audio;
using VehiclePhysics;
using EdyCommonTools;
using System;
using System.Text;
using System.Collections.Generic;

namespace Perrinn424
{

public class Perrinn424Underfloor : VehicleBehaviour
	{
	[Serializable]
	public class ContactPoint
		{
		[Tooltip("End point of contact. Interaction occurs when this point touches the ground.")]
		public Transform pointBase;
		[Tooltip("Vertical stiffness (spring between the ground and the car body) producing vertical load (N/m)")]
		public float stiffness = 500000.0f;
		[Tooltip("Coefficient of friction (μ) producing horizontal load (drag)")]
		public float friction = 0.2f;

		[Space(5)]
		[Tooltip("Maximum contact depth above the Point Base producing increasing vertical load (m). Beyond this depth the vertical load is limited the maximum (limitContactDepth * stiffness). Also, the audio effect is played at maximum volume at this contact depth.")]
		public float limitContactDepth = 0.05f;
		[Tooltip("Length above the Point Base for detecting the contact (m). Contact is not detected above this lenght (i.e. on top of the car). This length should fall inside the car's colliders")]
		public float detectionLength = 0.2f;
		}

	[Tooltip("Points of contact with the ground")]
	public ContactPoint[] contactPoints = new ContactPoint[0];

	[Tooltip("Layers to be verified for collision")]
	public LayerMask groundLayers = ~(1 << 8);

	[Header("Telemetry")]
	[Tooltip("Telemetry assumes the first 4 contact points to be:\n 0  Splitter Left\n 1  Splitter Right\n 2  Floor Front\n 3  Floor Rear")]
	public bool emitTelemetry = true;

	[Header("Audio Effect")]
	[Tooltip("Enable/Disable audio effects")]
	public bool isAudioEnabled = true;

	[Tooltip("Loopable audio clip with the underfloor drag effect")]
	public AudioClip contactLoopClip;
	[Tooltip("Unity audio mixer to send the audio effects to (optional)")]
	public AudioMixerGroup output;

	[Space(5)]
	[Tooltip("The slightest contact is played at this volume")]
	public float minVolume = 0.2f;
	[Tooltip("Volume at the limit contact depth")]
	public float maxVolume = 1.0f;

	[Space(5)]
	[Tooltip("Below this speed (m/s) volume and pitch are faded out to zero. Above this speed pitch is interpolated to maxPitch at maxSpeed")]
	public float minSpeed = 50 / 3.6f;  // 50 km/h
	[Tooltip("Pitch is interpolated between minSpeed and maxSpeed (m/s) from minPitch to maxPitch")]
	public float maxSpeed = 100.0f;     // 360 km/h
	[Tooltip("Minimum audio pitch at minSpeed")]
	public float minPitch = 0.9f;
	[Tooltip("Maximum audio pitch at maxSpeed and above")]
	public float maxPitch = 1.1f;

	[Header("3D Audio Source")]
	[Tooltip("Maximum volume under this distance")]
	public float minDistance = 2.0f;
	[Tooltip("Volume clipped beyond this distance")]
	public float maxDistance = 400.0f;
	[Tooltip("Volume attenuated progressively until this distance")]
	public float attenuationDistance = 100.0f;
	[Tooltip("This volume constant from Attenuation Distance to Max Distance")]
	public float attenuatedVolume = 0.025f;

	[Header("On-screen widget")]
	public bool showWidget = false;
	public GUITextBox.Settings widget = new GUITextBox.Settings();


	// Widget components

	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder(1024);

	// Trick to assign a default font to the GUI box. Configure it at the script settings in the Editor.
	[HideInInspector] public Font defaultFont;


	// Runtime data

	public class ContactPointData
		{
		// Physics

		public ContactPoint contactPoint;
		public bool contact;
		public int contactCount;
		public float contactDepth;
		public float verticalLoad;
		public Vector3 verticalForce;
		public Vector3 dragForce;

		// Audio effect

		public RuntimeAudio contactAudio;
		public int lastPlayedContact;
		public float maxContactDepth;

		// Widget

		public bool contactDetected;
		}


	ContactPointData[] m_contactPointData = new ContactPointData[0];

	public IList<ContactPointData> contactPointData { get => Array.AsReadOnly<ContactPointData>(m_contactPointData); }


	// Initialize the values of new members of the array when its size changes

	bool m_firstDeserialization = true;
	int m_contactsLength = 0;

	void OnValidate ()
		{
		if (m_firstDeserialization)
			{
			// First time the properties have been deserialized in the object. Take the actual array size.

			m_contactsLength = contactPoints.Length;
			m_firstDeserialization = false;
			}
		else
			{
			// If the array has been expanded initialize the new elements.

			if (contactPoints.Length > m_contactsLength)
				{
				for (int i = m_contactsLength; i < contactPoints.Length; i++)
					contactPoints[i] = new ContactPoint();
				}

			m_contactsLength = contactPoints.Length;
			}

		// Initialize default widget font

		if (widget.font == null)
			widget.font = defaultFont;
		}


	// Initialize widget


	public override void OnEnableVehicle ()
		{
		m_textBox.settings = widget;
		m_textBox.header = "424 Underfloor";
		}


	public override void OnDisableVehicle ()
		{
		ReleaseRuntimeData();
		}


	// Process contact points


	public override void FixedUpdateVehicle ()
		{
		if (contactPoints.Length != m_contactPointData.Length)
			InitializeRuntimeData();

		// Process contact points

		for (int i = 0, c = m_contactPointData.Length; i < c; i++)
			ProcessContactPoint(m_contactPointData[i]);
		}


	public override void UpdateAfterFixedUpdate ()
		{
		if (vehicle.paused) return;

		// Update audio

		for (int i = 0, c = m_contactPointData.Length; i < c; i++)
			UpdateContactAudio(m_contactPointData[i]);

		// Update widget

		if (showWidget)
			{
			m_text.Clear();
			m_text.Append($"               Stiffnes  Friction  Events    Depth      Fz    Drag\n");
			m_text.Append($"                   N/mm         μ               mm       N       N");
			for (int i = 0, c = m_contactPointData.Length; i < c; i++)
				AppendContactPointText(m_text, m_contactPointData[i]);
			m_textBox.text = m_text.ToString();
			}
		}


	public override void OnEnterPause ()
		{
		// Note: If the scripts are reloaded while paused then OnEnterPause will be called _before_
		// FixedUpdateVehicle. The runtime data will be empty.

		for (int i = 0, c = m_contactPointData.Length; i < c; i++)
			m_contactPointData[i].contactAudio.source.Stop();
		}


	void ProcessContactPoint (ContactPointData cpData)
		{
		cpData.contact = false;

		ContactPoint cp = cpData.contactPoint;
		if (cp.pointBase == null || cp.limitContactDepth <= 0.0001f)
			return;

		// Throw raycast to detect contact

		Vector3 up = cp.pointBase.up;
		Vector3 origin = cp.pointBase.position + up * cp.detectionLength;

		RaycastHit hitInfo;
		if (!Physics.Raycast(origin, -up, out hitInfo, cp.detectionLength, groundLayers, QueryTriggerInteraction.Ignore))
			return;

		// Determine if this contact makes sense (i.e. ignore contacts against nearly vertical surfaces)

		float upNormal = Vector3.Dot(up, hitInfo.normal);
		if (upNormal < 0.00001f)
			return;

		// Contact!

		cpData.contact = true;
		cpData.contactCount++;

		// Determine contact length ("penetration" of the ground above the point of contact)

		float contactDepth = cp.detectionLength - hitInfo.distance;
		if (contactDepth > cp.limitContactDepth)
			contactDepth = cp.limitContactDepth;

		cpData.contactDepth = contactDepth;

		// Calculate vertical force

		cpData.verticalLoad = contactDepth * cp.stiffness * upNormal;
		cpData.verticalForce = cpData.verticalLoad * up;

		// Calculate longitudinal force

		Vector3 velocity = vehicle.cachedRigidbody.GetPointVelocity(hitInfo.point);
		Vector3 slipDirection = Vector3.ProjectOnPlane(velocity, hitInfo.normal).normalized;
		cpData.dragForce = -cpData.verticalLoad * cp.friction * slipDirection;

		// Apply resulting forces

		vehicle.cachedRigidbody.AddForceAtPosition(cpData.verticalForce + cpData.dragForce, hitInfo.point);

		// Audio effect:
		// Store maximum registered contact depth.

		if (contactDepth > cpData.maxContactDepth)
			cpData.maxContactDepth = contactDepth;

		// Notify the widget.
		// This flag will be cleared when the widget has displayed the updated data.

		cpData.contactDetected = true;
		}


	void InitializeRuntimeData ()
		{
		// Release previously initialized runtime audio sources

		ReleaseRuntimeData();

		// Create new runtime data buffer and initialize new audio sources

		m_contactPointData = new ContactPointData[contactPoints.Length];

		for (int i = 0, c = m_contactPointData.Length; i < c; i++)
			{
			ContactPointData cp = new ContactPointData();
			cp.contactPoint = contactPoints[i];
			if (cp.contactPoint.pointBase != null)
				{
				cp.contactAudio = new RuntimeAudio(cp.contactPoint.pointBase);
				ConfigureAudioSource(cp.contactAudio);
				}
			m_contactPointData[i] = cp;
			}
		}


	void ReleaseRuntimeData ()
		{
		for (int i = 0, c = m_contactPointData.Length; i < c; i++)
			{
			if (m_contactPointData[i].contactAudio != null)
				m_contactPointData[i].contactAudio.Release();
			}
		}


	// Telemetry widget


	void AppendContactPointText (StringBuilder text, ContactPointData cpData)
		{
		ContactPoint cp = cpData.contactPoint;
		string name = cp.pointBase != null ? cp.pointBase.name : "(unused)";
		string contact = cpData.contactDetected ? "■" : " ";
		text.Append($"\n{name,-16} {cp.stiffness / 1000.0f,6:0.}    {cp.friction,6:0.00} ");
		text.Append($"{cpData.contactCount,7}{contact} {cpData.contactDepth * 1000.0f,7:0.00} {cpData.verticalLoad,7:0.} {cpData.dragForce.magnitude,7:0.}");
		cpData.contactDetected = false;
		}


	void OnGUI ()
		{
		if (showWidget)
			m_textBox.OnGUI();
		}


	// Telemetry
	//
	// Assumes these first 4 contact points in this order:
	//
	//		Splitter Left
	//		Splitter Right
	//		Floor Front
	//		Floor Rear


	public override bool EmitTelemetry ()
		{
		return emitTelemetry;
		}


	public override void RegisterTelemetry ()
		{
		vehicle.telemetry.Register<Perrinn424UnderfloorTelemetry>(this);
		}


	public override void UnregisterTelemetry ()
		{
		vehicle.telemetry.Unregister<Perrinn424UnderfloorTelemetry>(this);
		}


	public class Perrinn424UnderfloorTelemetry : Telemetry.ChannelGroup
		{
		public override int GetChannelCount ()
			{
			return 8;
			}


		public override Telemetry.PollFrequency GetPollFrequency ()
			{
			return Telemetry.PollFrequency.Normal;
			}


		public override void GetChannelInfo (Telemetry.ChannelInfo[] channelInfo, UnityEngine.Object instance)
			{
			channelInfo[0].SetNameAndSemantic("SplitterDepthLeft", Telemetry.Semantic.SuspensionTravel);
			channelInfo[1].SetNameAndSemantic("SplitterDepthRight", Telemetry.Semantic.SuspensionTravel);
			channelInfo[2].SetNameAndSemantic("FloorDepthFront", Telemetry.Semantic.SuspensionTravel);
			channelInfo[3].SetNameAndSemantic("FloorDepthRear", Telemetry.Semantic.SuspensionTravel);
			channelInfo[4].SetNameAndSemantic("SplitterFzLeft", Telemetry.Semantic.SuspensionForce);
			channelInfo[5].SetNameAndSemantic("SplitterFzRight", Telemetry.Semantic.SuspensionForce);
			channelInfo[6].SetNameAndSemantic("FloorFzFront", Telemetry.Semantic.SuspensionForce);
			channelInfo[7].SetNameAndSemantic("FloorFzRear", Telemetry.Semantic.SuspensionForce);
			}


		public override void PollValues (float[] values, int index, UnityEngine.Object instance)
			{
			Perrinn424Underfloor underfloor = instance as Perrinn424Underfloor;

			// Get contact point data and verify we have at least 4 contact points

			var contactPoints = underfloor.contactPointData;
			if (contactPoints.Count < 4)
				{
				for (int i = 0; i < 8; i++)
					values[index+i] = float.NaN;
				return;
				}

			// Fill in the corresponding channel values

			for (int i = 0; i < 4; i++)
				{
				ContactPointData cp = contactPoints[i];
				if (cp.contact)
					{
					values[index+i] = cp.contactDepth;
					values[index+i+4] = cp.verticalLoad;
					}
				else
					{
					values[index+i] = 0.0f;
					values[index+i+4] = 0.0f;
					}
				}
			}
		}


	// Audio


	private void ConfigureAudioSource (RuntimeAudio audio)
		{
		AudioSource source = audio.source;
		source.clip = contactLoopClip;
		source.outputAudioMixerGroup = output;
		source.spatialBlend = 1.0f;
		source.loop = true;
		source.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
		RuntimeAudio.SetVolumeRolloff(source, minDistance, 1.0f, attenuationDistance, attenuatedVolume, maxDistance);
		}

	void UpdateContactAudio (ContactPointData cpData)
		{
		RuntimeAudio audio = cpData.contactAudio;
		if (audio == null)
			return;

		AudioSource source = audio.source;
		if (!isAudioEnabled)
			{
			if (source.isPlaying) source.Stop();
			return;
			}

		float absSpeed = MathUtility.FastAbs(vehicle.speed);
		if (cpData.contactCount > cpData.lastPlayedContact && absSpeed > 0.005f)
			{
			if (!source.isPlaying) source.Play();

			float attenuation = Mathf.Clamp01(absSpeed / minSpeed);
			float intensity = Mathf.Clamp01(cpData.maxContactDepth / cpData.contactPoint.limitContactDepth);
			source.volume = Mathf.Lerp(minVolume, maxVolume, intensity) * attenuation;

			float speedFactor = Mathf.InverseLerp(minSpeed, maxSpeed, absSpeed);
			source.pitch = Mathf.Lerp(minPitch, maxPitch, speedFactor) * (0.8f + 0.2f * attenuation);   // Reduce pitch 80%
			MuteZeroPitch(source);

			cpData.lastPlayedContact = cpData.contactCount;
			cpData.maxContactDepth = 0.0f;
			}
		else
			{
			source.volume = 0.0f;
			}
		}

	private void MuteZeroPitch (AudioSource audio)
		{
		// Moving the AudioListener around a source with zero or nearly-zero pitch causes artifacts.
		// The audio source is muted in such chase to prevent that

		float absPitch = MathUtility.FastAbs(audio.pitch);
		if (absPitch < 0.05f)
			{
			audio.volume *= absPitch / 0.05f;
			}
		}

	// The OnDrawGizmos method makes the component appear at the Scene view's Gizmos dropdown menu,
	// Also causes the gizmo to be hidden if the component inspector is collapsed even in GizmoType.NonSelected mode.

	void OnDrawGizmos ()
		{
		}
	}

}