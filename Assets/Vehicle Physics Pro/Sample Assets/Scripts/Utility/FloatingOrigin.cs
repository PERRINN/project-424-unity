//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2020 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------

// Provides methods to translate the origin of coordinates either automatically or explicitly.
//
// Source
// https://twitter.com/Why485/status/1246255611043557377
//
// Based on the Unity Wiki FloatingOrigin script by Peter Stirling
// URL: http://wiki.unity3d.com/index.php/Floating_Origin


using UnityEngine;
using UnityEngine.SceneManagement;


namespace VehiclePhysics.Utility
{

public class FloatingOrigin : MonoBehaviour
	{
	[Tooltip("Point of reference from which to check the distance to origin.")]
	public Transform referenceObject = null;

	[Tooltip("Distance from the origin the reference object must be in order to trigger an origin shift.")]
	public float threshold = 3000f;

	[Header("Options")]
	[Tooltip("When true, origin shifts are considered only from the horizontal distance to origin.")]
	public bool use2DDistance = true;

	[Tooltip("When true, updates ALL open scenes. When false, updates only the active scene.")]
	public bool UpdateAllScenes = true;

	[Tooltip("Should ParticleSystems be moved with an origin shift.")]
	public bool UpdateParticles = true;

	[Tooltip("Should TrailRenderers be moved with an origin shift.")]
	public bool UpdateTrailRenderers = true;

	[Tooltip("Should LineRenderers be moved with an origin shift.")]
	public bool UpdateLineRenderers = true;

	private ParticleSystem.Particle[] m_particles = null;


	void LateUpdate ()
		{
		CheckTranslateOrigin();
		}


	public void CheckTranslateOrigin (bool forceTranslate = false)
		{
		if (referenceObject == null)
			return;

		Vector3 referencePosition = referenceObject.position;

		if (use2DDistance)
			referencePosition.y = 0f;

		if (forceTranslate || referencePosition.magnitude > threshold)
			TranslateOrigin(-referencePosition);
		}


	public void TranslateOrigin (Vector3 offset)
		{
		MoveRootTransforms(offset);
		NotifyVehicles(offset);
		Physics.SyncTransforms();

		if (UpdateParticles)
			MoveParticles(offset);

		if (UpdateTrailRenderers)
			MoveTrailRenderers(offset);

		if (UpdateLineRenderers)
			MoveLineRenderers(offset);
		}


	private void MoveRootTransforms (Vector3 offset)
		{
		if (UpdateAllScenes)
			{
			for (int z = 0; z < SceneManager.sceneCount; z++)
				{
				foreach (GameObject g in SceneManager.GetSceneAt(z).GetRootGameObjects())
					g.transform.position += offset;
				}
			}
		else
			{
			foreach (GameObject g in SceneManager.GetActiveScene().GetRootGameObjects())
				g.transform.position += offset;
			}
		}


	private void NotifyVehicles (Vector3 offset)
		{
		var vehicles = FindObjectsOfType<VehicleBase>();
		foreach (var v in vehicles)
			v.NotifyPositionChanged(offset);
		}


	private void MoveTrailRenderers (Vector3 offset)
		{
		var trails = FindObjectsOfType<TrailRenderer>() as TrailRenderer[];
		foreach (var trail in trails)
			{
			Vector3[] positions = new Vector3[trail.positionCount];

			int positionCount = trail.GetPositions(positions);
			for (int i = 0; i < positionCount; ++i)
				positions[i] += offset;

			trail.SetPositions(positions);
			}
		}

	private void MoveLineRenderers (Vector3 offset)
		{
		var lines = FindObjectsOfType<LineRenderer>() as LineRenderer[];
		foreach (var line in lines)
			{
			Vector3[] positions = new Vector3[line.positionCount];

			int positionCount = line.GetPositions(positions);
			for (int i = 0; i < positionCount; ++i)
				positions[i] += offset;

			line.SetPositions(positions);
			}
		}

	private void MoveParticles (Vector3 offset)
		{
		var particeSystems = FindObjectsOfType<ParticleSystem>() as ParticleSystem[];
		foreach (ParticleSystem ps in particeSystems)
			{
			if (ps.main.simulationSpace != ParticleSystemSimulationSpace.World)
				continue;

			int particlesNeeded = ps.main.maxParticles;

			if (particlesNeeded <= 0)
				continue;

			bool wasPlaying = ps.isPlaying;

			if (wasPlaying)
				ps.Pause();

			// ensure a sufficiently large array in which to store the particles
			if (m_particles is null || m_particles.Length < particlesNeeded)
				m_particles = new ParticleSystem.Particle[particlesNeeded];

			// now get the particles
			int num = ps.GetParticles(m_particles);
			for (int i = 0; i < num; i++)
				m_particles[i].position += offset;

			ps.SetParticles(m_particles, num);

			if (wasPlaying)
				ps.Play();
			}
		}
	}
}