//==================================================================================================
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//
// UnityRigidbody: wrapper to use Rigidbodies across Unity versions where the API has changed.
//==================================================================================================
//
// If you need to use any other property or method not declared in UnityRigidbody, just do:
//
//	Rigidbody rb = vehicle.cachedRigidbody;		// or any other UnityRigidbody reference
//	rb.SomeOtherMethodInTheUnityAPI();
//
// Note: destroying an UnityRigidbody struct with Destroy() also destroys the Rigidbody component.


using UnityEngine;


namespace VersionCompatibility
{

[System.Serializable]
public struct UnityRigidbody
	{
	Rigidbody m_rigidbody;
	bool m_isValid;

	// Standard operators

	public static implicit operator Rigidbody (UnityRigidbody r) => r.m_rigidbody;
	public static implicit operator UnityRigidbody (Rigidbody r) => new UnityRigidbody { m_rigidbody = r, m_isValid = r != null };

	public static bool operator == (UnityRigidbody a, UnityRigidbody b) => a.m_rigidbody == b.m_rigidbody;
	public static bool operator != (UnityRigidbody a, UnityRigidbody b) => a.m_rigidbody != b.m_rigidbody;
	public override bool Equals (object r) => m_rigidbody == ((UnityRigidbody)r).m_rigidbody;
	public override int GetHashCode () => m_isValid? m_rigidbody.GetHashCode() : 0;

	// Fast comparison and null-check

	public bool isNull => !m_isValid;
	public bool isNotNull => m_isValid;
	public static bool FastCompare (UnityRigidbody a, UnityRigidbody b) => !a.m_isValid && !b.m_isValid
		|| a.m_isValid && b.m_isValid && a.m_rigidbody.GetHashCode() == b.m_rigidbody.GetHashCode();

	// String representation

	public override string ToString () => m_isValid? $"[UnityRigidbody:{m_rigidbody.gameObject.name}]" : "[UnityRigidbody:null]";
	public static implicit operator string (UnityRigidbody r) => r.ToString();

	// Wrap properties

	public Transform transform => m_rigidbody.transform;
	public GameObject gameObject => m_rigidbody.gameObject;

	public HideFlags hideFlags
	  	{
		get => m_rigidbody.hideFlags;
		set => m_rigidbody.hideFlags = value;
		}

	#if UNITY_6000_0_OR_NEWER
	public Vector3 linearVelocity
		{
		get => m_rigidbody.linearVelocity;
		set => m_rigidbody.linearVelocity = value;
		}

	public float linearDamping
		{
		get => m_rigidbody.linearDamping;
		set => m_rigidbody.linearDamping = value;
		}

	public float angularDamping
		{
		get => m_rigidbody.angularDamping;
		set => m_rigidbody.angularDamping = value;
		}
	#else
	public Vector3 linearVelocity
		{
		get => m_rigidbody.velocity;
		set => m_rigidbody.velocity = value;
		}

	public float linearDamping
		{
		get => m_rigidbody.drag;
		set => m_rigidbody.drag = value;
		}

	public float angularDamping
		{
		get => m_rigidbody.angularDrag;
		set => m_rigidbody.angularDrag = value;
		}
	#endif

	public Vector3 angularVelocity
		{
		get => m_rigidbody.angularVelocity;
		set => m_rigidbody.angularVelocity = value;
		}

	public float maxAngularVelocity
		{
		get => m_rigidbody.maxAngularVelocity;
		set => m_rigidbody.maxAngularVelocity = value;
		}

	public float mass
		{
		get => m_rigidbody.mass;
		set => m_rigidbody.mass = value;
		}

	public Vector3 centerOfMass
		{
		get => m_rigidbody.centerOfMass;
		set => m_rigidbody.centerOfMass = value;
		}

	public Vector3 worldCenterOfMass => m_rigidbody.worldCenterOfMass;

	public Vector3 inertiaTensor
		{
		get => m_rigidbody.inertiaTensor;
		set => m_rigidbody.inertiaTensor = value;
		}

	public Quaternion inertiaTensorRotation
		{
		get => m_rigidbody.inertiaTensorRotation;
		set => m_rigidbody.inertiaTensorRotation = value;
		}

	public float maxDepenetrationVelocity
		{
		get => m_rigidbody.maxDepenetrationVelocity;
		set => m_rigidbody.maxDepenetrationVelocity = value;
		}

	public Vector3 position
		{
		get => m_rigidbody.position;
		set => m_rigidbody.position = value;
		}

	public Quaternion rotation
		{
		get => m_rigidbody.rotation;
		set => m_rigidbody.rotation = value;
		}

	public bool isKinematic
		{
		get => m_rigidbody.isKinematic;
		set { m_rigidbody.isKinematic = value; }
		}

	public bool useGravity
		{
		get => m_rigidbody.useGravity;
		set => m_rigidbody.useGravity = value;
		}

	public RigidbodyInterpolation interpolation
		{
		get => m_rigidbody.interpolation;
		set => m_rigidbody.interpolation = value;
		}

	// Wrap methods

	public T[] GetComponentsInChildren<T>() => m_rigidbody.GetComponentsInChildren<T>();

	public void AddForce (Vector3 force, ForceMode mode = ForceMode.Force) => m_rigidbody.AddForce(force, mode);
	public void AddForceAtPosition (Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force) => m_rigidbody.AddForceAtPosition(force, position, mode);
	public void AddRelativeForce (Vector3 force, ForceMode mode = ForceMode.Force) => m_rigidbody.AddRelativeForce(force, mode);

	public void AddTorque (Vector3 torque, ForceMode mode = ForceMode.Force) => m_rigidbody.AddTorque(torque, mode);
	public void AddRelativeTorque (Vector3 torque, ForceMode mode) => m_rigidbody.AddRelativeTorque(torque, mode);

	public void ResetInertiaTensor () => m_rigidbody.ResetInertiaTensor();

	public void MovePosition (Vector3 position) => m_rigidbody.MovePosition(position);
	public void MoveRotation (Quaternion rotation) => m_rigidbody.MoveRotation(rotation);

	public Vector3 GetPointVelocity (Vector3 point) => m_rigidbody.GetPointVelocity(point);
	}

}


