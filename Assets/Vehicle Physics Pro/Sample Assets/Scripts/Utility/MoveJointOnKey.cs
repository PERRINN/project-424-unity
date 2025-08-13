//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2025 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

using UnityEngine;
using VersionCompatibility;


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
	public UnityKey moveForwardKey = UnityKey.NumpadPlus;
	public UnityKey moveReverseKey = UnityKey.NumpadMinus;


    VPVehicleJoint m_joint;


	void OnEnable ()
		{
		m_joint = GetComponent<VPVehicleJoint>();
		}


	void Update ()
		{
		if (UnityInput.GetKey(moveForwardKey))
			{
			Vector3 delta = direction * speed * Time.deltaTime;

			if (anchor == Anchor.ThisAnchor)
				m_joint.thisAnchorPosition += delta;
			else
				m_joint.otherAnchorPosition += delta;
			}


		if (UnityInput.GetKey(moveReverseKey))
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