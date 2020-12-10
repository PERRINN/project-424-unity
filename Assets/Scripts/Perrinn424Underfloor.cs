

using UnityEngine;
using System;


public class Perrinn424Underfloor : MonoBehaviour
    {
    [Serializable]
    public class ContactPoint
        {
        [Tooltip("End point of contact. Interaction occurs when this point touches the ground.")]
        public Transform pointBase;
        [Tooltip("Vertical stiffness (spring between the ground and the car body) producing vertical load")]
        public float stiffness = 500000.0f;
        [Tooltip("Coefficient of friction producing horizontal load (drag)")]
        public float friction = 0.2f;
        [Space(5)]
        [Tooltip("Maximum contact length above the Point Base producing increasing vertical load. Beyond this length the vertical load is not increased and will be kept to the maximum (maxLength * stiffness)")]
        public float maxLength = 0.05f;
        [Tooltip("Length above the Point Base for detecting the contact. Contact is not detected above this lenght (i.e. on top of the car)")]
        public float detectionLength = 0.2f;
        }

    [Tooltip("Points of contact with the ground")]
    public ContactPoint[] contactPoints = new ContactPoint[0];

    [Tooltip("Layers to be verified for collision")]
    public LayerMask groundLayers = ~(1 << 8);


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
		}
    }
