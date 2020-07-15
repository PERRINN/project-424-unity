//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;

namespace VehiclePhysics.Utility
{

#if !VPP_LIMITED

[RequireComponent(typeof(VPVehicleJoint))]
public class MoveJointOnKey : MonoBehaviour
	{
	public enum Anchor { ThisAnchor, OtherAnchor }

	public Anchor anchor = Anchor.ThisAnchor;
	public Vector3 direction = Vector3.up;
	public float speed = 0.01f;
	public KeyCode moveForwardKey = KeyCode.KeypadPlus;
	public KeyCode moveReverseKey = KeyCode.KeypadMinus;


    VPVehicleJoint m_joint;


	void OnEnable ()
		{
		m_joint = GetComponent<VPVehicleJoint>();
		}


	void Update ()
		{
		if (Input.GetKey(moveForwardKey))
			{
			Vector3 delta = direction * speed * Time.deltaTime;

			if (anchor == Anchor.ThisAnchor)
				m_joint.thisAnchorPosition += delta;
			else
				m_joint.otherAnchorPosition += delta;
			}


		if (Input.GetKey(moveReverseKey))
			{
			Vector3 delta = direction * speed * Time.deltaTime;

			if (anchor == Anchor.ThisAnchor)
				m_joint.thisAnchorPosition -= delta;
			else
				m_joint.otherAnchorPosition -= delta;
			}
		}
	}

#endif
}