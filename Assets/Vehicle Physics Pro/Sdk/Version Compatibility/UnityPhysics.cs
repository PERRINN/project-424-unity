//==================================================================================================
// (c) Angel Garcia "Edy" - Oviedo, Spain
// http://www.edy.es
//
// Cross-version physics utilities
//==================================================================================================


using UnityEngine;


namespace VersionCompatibility
{

public static class UnityPhysics
	{
	#if UNITY_6000_0_OR_NEWER
	public static bool autoSimulation
		{
		get => Physics.simulationMode != SimulationMode.Script;
		set => Physics.simulationMode = value? SimulationMode.FixedUpdate : SimulationMode.Script;
		}
	#else
	public static bool autoSimulation
		{
		get => Physics.autoSimulation;
		set => Physics.autoSimulation = value;
		}
	#endif
	}

}