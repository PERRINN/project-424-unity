//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace VehiclePhysics.UI
{

public class ManettinoDial : MonoBehaviour, IPointerDownHandler, IDragHandler
	{
	public int position = 0;
	public int[] angles;
	[Space(5)]
	public float separatorAngle = 0.0f;
	public float minRadius = 15.0f;
	[Space(5)]
	public RectTransform dial;

	[System.Serializable]
	public class OnChangeEvent : UnityEvent<int> { }

	[Space(10)]
	public OnChangeEvent onChange = new OnChangeEvent();

	int m_prevPosition;
    RectTransform m_rectTransform;


	void OnEnable ()
		{
		m_rectTransform = GetComponent<RectTransform>();
		m_prevPosition = -1;
		}


	void Update ()
		{
		if (position != m_prevPosition)
			{
			if (dial != null && position >= 0 && position < angles.Length)
				dial.localRotation = Quaternion.AngleAxis(UniformAngle(angles[position]), Vector3.forward);

			onChange.Invoke(position);
			m_prevPosition = position;
			}
		}


	// UI event receivers


	public int positionSelected
		{
		get
			{ return position; }

		set
			{ position = value; }
		}


    // Pointer events


	public void OnPointerDown(PointerEventData eventData)
		{
		PointerEvent(eventData);
		}

	public void OnDrag(PointerEventData eventData)
		{
		PointerEvent(eventData);
		}


	public void PointerEvent (PointerEventData eventData)
		{
		Vector2 localCursor;

		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
			return;

		if (localCursor.magnitude < minRadius) return;

		// Angle is calculated as it matches the angle around the Z axis for the dial

		float angle = Mathf.Atan2(-localCursor.y, -localCursor.x) * Mathf.Rad2Deg;
		position = FindPosition(angle);

		// Debug.Log("Angle: " + angle.ToString("0.00") + " Position: " + position);
		}


	public int FindPosition (float angle)
		{
		angle = UniformAngle(angle);

		for (int i=0; i<angles.Length; i++)
			{
			float a = UniformAngle(angles[i]);

			if (angle > a)
				{
				if (i == 0) return 0;

				float boundary = (a + UniformAngle(angles[i-1])) / 2;

				if (angle > boundary)
					return i-1;
				else
					return i;
				}
			}

		return angles.Length - 1;
		}


	public float UniformAngle (float angle)
		{
		while (angle > separatorAngle) angle -= 360.0f;
		return angle;
		}
	}

}