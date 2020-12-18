

using UnityEngine;
using VehiclePhysics;
using EdyCommonTools;
using System;
using System.Text;


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
		[Tooltip("Maximum contact length above the Point Base producing increasing vertical load (m). Beyond this length the vertical load is not increased and will be kept to the maximum (maxLength * stiffness)")]
		public float maxLength = 0.05f;
		[Tooltip("Length above the Point Base for detecting the contact (m). Contact is not detected above this lenght (i.e. on top of the car)")]
		public float detectionLength = 0.2f;

		// Runtime data

		[NonSerialized] public bool contact;
		[NonSerialized] public int contactCount;
		[NonSerialized] public float contactLength;
		[NonSerialized] public Vector3 verticalForce;
		[NonSerialized] public Vector3 dragForce;
		}

	[Tooltip("Points of contact with the ground")]
	public ContactPoint[] contactPoints = new ContactPoint[0];

	[Tooltip("Layers to be verified for collision")]
	public LayerMask groundLayers = ~(1 << 8);

	[Header("On-screen widget")]
	public bool showWidget = false;
	public GUITextBox.Settings widget = new GUITextBox.Settings();


	// Widget components

	GUITextBox m_textBox = new GUITextBox();
	StringBuilder m_text = new StringBuilder(1024);

	// Trick to assign a default font to the GUI box. Configure it at the script settings in the Editor.
	[HideInInspector] public Font defaultFont;


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

		// ClearContacts();
		}


	// Process contact points


	public override void FixedUpdateVehicle ()
		{
		for (int i = 0, c = contactPoints.Length; i < c; i++)
			ProcessContactPoint(contactPoints[i]);
		}


	void ProcessContactPoint (ContactPoint cp)
		{
		if (cp.pointBase == null)
			return;

		// Throw raycast to detect contact

		Vector3 up = cp.pointBase.up;
		Vector3 origin = cp.pointBase.position + up * cp.detectionLength;

		RaycastHit hitInfo;
		if (!Physics.Raycast(origin, -up, out hitInfo, cp.detectionLength, groundLayers, QueryTriggerInteraction.Ignore))
			return;

		// This is cleared when the widget has acknowledged the contact

		cp.contact = true;
		cp.contactCount++;

		// Determine if this contact makes sense (i.e. ignore contacts against vertical surfaces)

		float upNormal = Vector3.Dot(up, hitInfo.normal);
		if (upNormal < 0.00001f)
			return;

		// Determine contact length ("penetration" of the ground above the point of contact)

		float contactLength = cp.detectionLength - hitInfo.distance;
		if (contactLength > cp.maxLength)
			contactLength = cp.maxLength;

		cp.contactLength = contactLength;

		// Calculate vertical force

		float verticalLoad = contactLength * cp.stiffness * upNormal;
		cp.verticalForce = verticalLoad * up;

		// Calculate longitudinal force

		Vector3 velocity = vehicle.cachedRigidbody.GetPointVelocity(hitInfo.point);
		Vector3 slipDirection = Vector3.ProjectOnPlane(velocity, hitInfo.normal).normalized;
		cp.dragForce = -verticalLoad * cp.friction * slipDirection;

		// Apply resulting forces

		vehicle.cachedRigidbody.AddForceAtPosition(cp.verticalForce + cp.dragForce, hitInfo.point);
		}


	void ClearContacts ()
		{
		foreach (ContactPoint cp in contactPoints)
			{
			cp.contact = false;
			cp.contactCount = 0;
			}
		}


	// Telemetry widget


	public override void UpdateAfterFixedUpdate ()
		{
		if (showWidget)
			{
			m_text.Clear();
			m_text.Append($"               Stiffnes  Friction  Events    Depth      Fz    Drag\n");
			m_text.Append($"                   N/mm         μ               mm       N       N");
			for (int i = 0, c = contactPoints.Length; i < c; i++)
				AppendContactPointText(m_text, contactPoints[i]);
			m_textBox.UpdateText(m_text.ToString());
			}
		}


	void AppendContactPointText (StringBuilder text, ContactPoint cp)
		{
		string name = cp.pointBase != null? cp.pointBase.name : "(unused)";
		string contact = cp.contact? "■" : " ";
		text.Append($"\n{name,-16} {cp.stiffness/1000.0f,6:0.}    {cp.friction,6:0.00} ");
		text.Append($"{cp.contactCount,7}{contact} {cp.contactLength*1000.0f,7:0.00} {cp.verticalForce.magnitude,7:0.} {cp.dragForce.magnitude,7:0.}");
		cp.contact = false;
		}


	void OnGUI ()
		{
		if (showWidget)
			m_textBox.OnGUI();
		}


	// The OnDrawGizmos method makes the component appear at the Scene view's Gizmos dropdown menu,
	// Also causes the gizmo to be hidden if the component inspector is collapsed even in GizmoType.NonSelected mode.

	void OnDrawGizmos ()
		{
		}
	}
