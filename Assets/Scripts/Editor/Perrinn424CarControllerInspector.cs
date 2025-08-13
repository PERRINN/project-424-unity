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
public class Perrinn424CarControllerInspector : CustomInspector
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

		// Front tires
		bool editPressed;
		var tireAsset = DrawEditableProperty("frontTires", out editPressed).objectReferenceValue as TireDataContainerBase;
		if (editPressed && tireAsset != null)
			TireEditorWindow.OpenAsset(tireAsset);

		// Rear tires
		tireAsset = DrawEditableProperty("rearTires", out editPressed).objectReferenceValue as TireDataContainerBase;
		if (editPressed && tireAsset != null)
			TireEditorWindow.OpenAsset(tireAsset);

		GUI.enabled = true;
		Space();
		DrawProperty("groundTracking");

		// Torque maps

		DrawHeader("Powertrain and dynamics");
		if (CommonEditorTools.IsActiveAndPlaying((MonoBehaviour)target))
			GUI.enabled = false;
		var torqueMapAsset = DrawEditableProperty("torqueMap", out editPressed).objectReferenceValue as DualMguTorqueMapContainerBase;
		if (editPressed && torqueMapAsset != null)
			DualMguTorqueMapEditorWindow.OpenAsset(torqueMapAsset);
		GUI.enabled = true;
		DrawProperty("reverseGearLimiter");

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
		SetMinLabelWidth(175);
		DrawProperty("solverSubsteps");
		DrawProperty("wheelMomentumFactor");
		SetMinLabelWidth(220);
		DrawProperty("asyncPhysicsQueries");
		DrawProperty("allowLiftAndCoastOnAutopilot");
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