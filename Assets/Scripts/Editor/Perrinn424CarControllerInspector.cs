//--------------------------------------------------------------
//      Vehicle Physics Pro: advanced vehicle physics kit
//          Copyright © 2011-2019 Angel Garcia "Edy"
//        http://vehiclephysics.com | @VehiclePhysics
//--------------------------------------------------------------


using UnityEngine;
using UnityEditor;
using EdyCommonTools.EditorTools;


namespace VehiclePhysics.EditorTools
{
[CustomEditor(typeof(Perrinn424CarController))]
public class Perrinn424CarControllerInspector : VPInspector
	{
	static bool s_inertiaExpanded = false;

	public override void DrawInspectorGUI ()
		{
		DrawProperty("centerOfMass");
		s_inertiaExpanded = DrawProperty("inertia").isExpanded;

		Space();
		if (CommonEditorTools.IsActiveAndPlaying((MonoBehaviour)target))
			GUI.enabled = false;
		DrawProperty("frontAxle");
		DrawProperty("rearAxle");
		Space();
		DrawProperty("frontTires");
		DrawProperty("rearTires");
		GUI.enabled = true;
		Space();
		DrawProperty("groundTracking");

		DrawHeader("Powertrain and dynamics");

		DrawProperty("pedals");
		DrawProperty("frontMgu");
		DrawProperty("rearMgu");
		Space();
		DrawProperty("frontDifferential");
		DrawProperty("rearDifferential");
		Space();
		DrawProperty("steering");

		DrawHeader("Driving Aids");
		DrawProperty("steeringAids");
		DrawProperty("speedControl");

		DrawHeader("Safety Aids");
		DrawProperty("tractionControl", "Traction Control (TCS)");

		DrawHeader("Advanced");
		SetMinLabelWidth(165);
		DrawProperty("integrationSteps");
		DrawProperty("integrationUseRK4");
		DrawProperty("wheelSleepVelocity");
		DrawProperty("tireAdherentImpulseRatio");

		DrawHeader("Experimental");
		DrawProperty("tireSideDeflection");
		DrawProperty("tireSideDeflectionRate");

		Space();
		SetMinLabelWidth(190);
		bool advancedSuspension = DrawProperty("advancedSuspensionDamper").boolValue;
		if (!advancedSuspension) GUI.enabled = false;
		DrawProperty("suspensionDamperLimitFactor");
		GUI.enabled = true;

		}


	[DrawGizmo(GizmoType.Selected)]
	static void DrawVehicleGizmo (Perrinn424CarController src, GizmoType gizmoType)
		{
		// Component disabled - don't draw

		if (!src.enabled || !src.gameObject.activeInHierarchy) return;

		// CoM gizmo. Ensure it's drawn in the correct center of mass in Editor mode

		if (!Application.isPlaying && src.centerOfMass != null)
			src.cachedRigidbody.centerOfMass = src.cachedTransform.InverseTransformPoint(src.centerOfMass.position);

		EdyCommonTools.DebugUtility.DrawCrossMark(src.cachedTransform.TransformPoint(src.cachedRigidbody.centerOfMass), src.cachedTransform, Color.white);

		// Inertia gizmo

        if (s_inertiaExpanded)
			{
			Inertia.DrawGizmo(src.inertia, src.cachedRigidbody);
			}
		}

	}
}