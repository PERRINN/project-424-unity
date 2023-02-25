
using UnityEngine;
using UnityEngine.Audio;
using VehiclePhysics;
using EdyCommonTools;
using System.Collections.Generic;


namespace Perrinn424
{

public class Perrinn424UnderfloorAudio : VehicleBehaviour, IAudioSourceSettings
	{
	public Perrinn424Underfloor target;

	[Header("Audio Effect")]
	[Tooltip("Enable/Disable audio effects")]
	public bool audioEnabled = true;

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

	[Header("3D Audio Settings")]
	[Tooltip("Configuration of audio sources. Note: The Spatial Settings section is not applied. Modifying these settings in runtime requires disabling and re-enabling the component.")]
	public AudioSourceSettings settings = new AudioSourceSettings();


	// Runtime data

	public class AudioData
		{
		public Perrinn424Underfloor.ContactPointData contact;
		public RuntimeAudio contactAudio;
		public int lastPlayedContact;
		}


	AudioData[] m_audioData = new AudioData[0];


	public Perrinn424UnderfloorAudio ()
		{
		// Configure default values in audio settings before deserialization

		settings.minDistance = 2.0f;
		settings.maxDistance = 400.0f;
		settings.customRolloff = true;
		settings.farDistance = 50.0f;
		settings.farVolume = 0.03f;
		settings.filterRatio = 0.82f;
		}


	public override void OnEnableVehicle ()
		{
		if (target == null)
			{
			Debug.LogWarning("Perrinn424UnderfloorAudio requires a reference to the Perrinn424Underfloor component.");
			enabled = false;
			return;
			}
		}


	public override void OnDisableVehicle ()
		{
		ReleaseRuntimeData();
		}


	// Process contact points


	public override void UpdateAfterFixedUpdate ()
		{
		// Initialize runtime data to match the target's runtime data.
		// It might be an empty array if the target is disabled.

		IList<Perrinn424Underfloor.ContactPointData> contactPointData = target.contactPointData;
		if (contactPointData.Count != m_audioData.Length)
			InitializeRuntimeData(contactPointData);

		// Update audio

		if (vehicle.paused) return;

		for (int i = 0, c = m_audioData.Length; i < c; i++)
			UpdateContactAudio(m_audioData[i]);
		}


	public override void OnEnterPause ()
		{
		// Note: If the scripts are reloaded while paused then OnEnterPause will be called _before_
		// FixedUpdateVehicle. The runtime data will be empty.

		for (int i = 0, c = m_audioData.Length; i < c; i++)
			m_audioData[i].contactAudio.source.Stop();
		}


	void InitializeRuntimeData (IList<Perrinn424Underfloor.ContactPointData> contactPointData)
		{
		// Release previously initialized runtime audio sources

		ReleaseRuntimeData();

		// Create new runtime data buffer and initialize audio sources

		m_audioData = new AudioData[contactPointData.Count];

		for (int i = 0, c = contactPointData.Count; i < c; i++)
			{
			AudioData audioData = new AudioData();
			audioData.contact = contactPointData[i];
			if (audioData.contact.contactPoint.pointBase != null)
				{
				audioData.contactAudio = new RuntimeAudio(audioData.contact.contactPoint.pointBase);
				ConfigureAudioSource(audioData.contactAudio);
				}
			m_audioData[i] = audioData;
			}
		}


	void ReleaseRuntimeData ()
		{
		for (int i = 0, c = m_audioData.Length; i < c; i++)
			{
			if (m_audioData[i].contactAudio != null)
				m_audioData[i].contactAudio.Release();
			}

		m_audioData = new AudioData[0];
		}


	private void ConfigureAudioSource (RuntimeAudio audio)
		{
		AudioSource source = audio.source;
		source.clip = contactLoopClip;
		source.outputAudioMixerGroup = output;
		source.loop = true;
		source.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;

		settings.Apply(source, audio.lowPassFilter, false, 1.0f);
		}


	void UpdateContactAudio (AudioData audioData)
		{
		RuntimeAudio audio = audioData.contactAudio;
		if (audio == null)
			return;

		AudioSource source = audio.source;
		if (!audioEnabled)
			{
			if (source.isPlaying) source.Stop();
			return;
			}

		float absSpeed = MathUtility.FastAbs(vehicle.speed);
		if (audioData.contact.contactCount > audioData.lastPlayedContact && absSpeed > 0.005f)
			{
			if (!source.isPlaying) source.Play();

			float attenuation = Mathf.Clamp01(absSpeed / minSpeed);
			float intensity = Mathf.Clamp01(audioData.contact.maxContactDepth / audioData.contact.contactPoint.limitContactDepth);
			source.volume = Mathf.Lerp(minVolume, maxVolume, intensity) * attenuation;

			float speedFactor = Mathf.InverseLerp(minSpeed, maxSpeed, absSpeed);
			source.pitch = Mathf.Lerp(minPitch, maxPitch, speedFactor) * (0.8f + 0.2f * attenuation);   // Reduce pitch 80%
			MuteZeroPitch(source);

			audioData.lastPlayedContact = audioData.contact.contactCount;
			audioData.contact.maxContactDepth = 0.0f;
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


	// -----------------------------------------------------------------------------------------------------
	// IAudioSourceSettings interface


	AudioSourceSettings IAudioSourceSettings.settings
		{
		get => settings;
		set
			{
			settings = value;

			foreach (AudioData audioData in m_audioData)
				{
				if (audioData.contactAudio != null)
					ConfigureAudioSource(audioData.contactAudio);
				}
			}
		}
	}

}