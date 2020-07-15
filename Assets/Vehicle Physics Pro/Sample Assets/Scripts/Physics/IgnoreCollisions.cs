//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Makes the physics ignore collisions between this collider (and/or the children colliders)
// and the listed colliders.


using UnityEngine;
using EdyCommonTools;

namespace VehiclePhysics.Utility
{

[AddComponentMenu("Vehicle Physics/Utility/Ignore Collisions", 2)]
public class IgnoreCollisions : MonoBehaviour
	{
	public Collider thisCollider;
	public bool includeChildren = false;
	public Collider[] otherColliders;


	Collider[] m_theseColliders;


	void OnEnable ()
		{
		if (thisCollider == null)
			thisCollider = GetComponent<Collider>();

		if (includeChildren)
			{
			if (thisCollider != null)
				m_theseColliders = thisCollider.GetComponentsInChildren<Collider>();
			else
				m_theseColliders = GetComponentsInChildren<Collider>();
			}
		else
			{
			m_theseColliders = new Collider[] { thisCollider };
			}

		IgnoreCollisionGroups(m_theseColliders, otherColliders, true);
		}


	void OnDisable ()
		{
		IgnoreCollisionGroups(m_theseColliders, otherColliders, false);
		}


	void IgnoreCollisionGroups (Collider[] group1, Collider[] group2, bool ignoreCollision)
		{
		foreach (Collider thisCol in group1)
			{
			foreach (Collider otherCol in group2)
				{
				if (thisCol != null && otherCol != null)
					Physics.IgnoreCollision(thisCol, otherCol, ignoreCollision);
				}
			}
		}
	}
}
