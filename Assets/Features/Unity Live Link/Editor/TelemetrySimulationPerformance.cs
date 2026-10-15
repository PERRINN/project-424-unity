using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs.LowLevel.Unsafe;

namespace Perrinn424.LiveLink
{
    // Presentation-only allowlist. Never disable a GameObject containing vehicle/timing components.
    internal static class TelemetrySimulationPerformance
    {
        const string Key = "Perrinn424.Telemetry.Performance.";
        const int RenderInterval = 8;
        static Snapshot snapshot;
        static bool reportWritten;
        static readonly HashSet<string> Presentation = new HashSet<string> {
            "VehiclePhysics.VPTelemetryDisplay", "VehiclePhysics.VPTelemetryTools", "VehiclePhysics.VPPerformanceDisplay",
            "VehiclePhysics.VPCameraController",
            "VehiclePhysics.VPAudio", "VehiclePhysics.VPAudioExtras", "VehiclePhysics.VPTireAudio",
            "VehiclePhysics.UI.ValueMonitor", "VehiclePhysics.UI.TimerDisplay",
            "Perrinn424.MGUAudioUpdater", "Perrinn424.Perrinn424UnderfloorAudio", "Perrinn424.CockpitCameraAudioFilter",
            "Perrinn424.DashboardDisplay", "Perrinn424.VisualPedalRotation",
            "Perrinn424.UI.TelemetryUIIntegration", "Perrinn424.UI.UserInterfaceController", "Perrinn424.UI.LapTimerListener",
            "Perrinn424.UI.Dashboard", "Perrinn424.UI.OnBoardDashboard", "Perrinn424.UI.FPSCounter", "Perrinn424.UI.TimeScaleUI",
            "Perrinn424.UI.ReferenceInformation", "Perrinn424.UI.DriverCameraAdjustmentsUI", "Perrinn424.UI.LapTimeTable",
            "Perrinn424.TrackMapSystem.TrackMapUI", "Perrinn424.CameraSystem.CameraController", "Perrinn424.CameraSystem.CameraControllerInput",
            "Perrinn424.CameraSystem.OnboardCamerasController", "Perrinn424.CameraSystem.OnboardCamerasControllerInput",
            "EdyCommonTools.CameraFovController", "EdyCommonTools.CameraFovInput", "EdyCommonTools.CameraShift",
            "Perrinn424.TVCameraZoomController", "ClosestCameraEvaluator", "VehiclePhysics.VRCamera",
            "UnityEngine.EventSystems.EventSystem", "VersionCompatibility.UnityUIInputModule"
        };
        [Serializable] internal class Entry { public string type; public int enabledCount; public string action; }
        [Serializable] class Snapshot
        {
            public string profile = "telemetry-fast-v2";
            public bool applied;
            public int renderFrameInterval = RenderInterval;
            public int panelId;
            public bool panelManaged, originalPanelVisible;
            public int originalVSync, originalFrameRate, originalRenderInterval;
            public int originalWorkers, workers;
            public bool originalAudioPause;
            public float fixedDeltaTime;
            public int[] disabled;
            public Entry[] inventory;
        }
        static bool IsPresentation(Behaviour component)
        {
            var type = component.GetType();
            return component is Camera || component is Canvas || component is AudioSource || component is AudioListener ||
                type.Namespace == "Cinemachine" || type.Namespace == "UnityEngine.UI" || Presentation.Contains(type.FullName);
        }
        static Snapshot Inspect()
        {
            var disabled = new List<int>();
            var inventory = new SortedDictionary<string, Entry>(StringComparer.Ordinal);
            foreach (var component in UnityEngine.Object.FindObjectsOfType<Behaviour>())
            {
                if (!component.enabled || !component.gameObject.scene.IsValid() || !component.gameObject.scene.isLoaded) continue;
                string name = component.GetType().FullName;
                bool presentation = IsPresentation(component);
                Entry entry;
                if (!inventory.TryGetValue(name, out entry))
                    inventory[name] = entry = new Entry { type = name, action = presentation ? "disable presentation" : "retain" };
                entry.enabledCount++;
                if (presentation) disabled.Add(component.GetInstanceID());
            }
            return new Snapshot {
                originalWorkers = JobsUtility.JobWorkerCount, workers = Math.Min(1, JobsUtility.JobWorkerMaximumCount),
                originalVSync = QualitySettings.vSyncCount, originalFrameRate = Application.targetFrameRate,
                originalRenderInterval = OnDemandRendering.renderFrameInterval, originalAudioPause = AudioListener.pause,
                fixedDeltaTime = Time.fixedDeltaTime, disabled = disabled.ToArray(), inventory = new List<Entry>(inventory.Values).ToArray()
            };
        }
        public static void Apply()
        {
            if (snapshot == null)
            {
                string saved = SessionState.GetString(Key + "snapshot", "");
                snapshot = string.IsNullOrEmpty(saved) ? Inspect() : JsonUtility.FromJson<Snapshot>(saved);
                snapshot.applied = true;
                SessionState.SetString(Key + "snapshot", JsonUtility.ToJson(snapshot));
                reportWritten = false;
                // Capture the entire state before invoking any OnDisable callbacks.
                foreach (int id in snapshot.disabled)
                {
                    var component = EditorUtility.InstanceIDToObject(id) as Behaviour;
                    if (component != null) component.enabled = false;
                }
            }
            if (JobsUtility.JobWorkerCount != snapshot.workers) JobsUtility.JobWorkerCount = snapshot.workers;
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            OnDemandRendering.renderFrameInterval = RenderInterval;
            AudioListener.pause = true;
        }
        public static void ConfigureBridge(UnityLiveLink bridge)
        {
            if (snapshot == null) return;
            if (!snapshot.panelManaged)
            {
                snapshot.panelId = bridge.GetInstanceID(); snapshot.originalPanelVisible = bridge.showConnectionPanel; snapshot.panelManaged = true;
                SessionState.SetString(Key + "snapshot", JsonUtility.ToJson(snapshot));
            }
            bridge.showConnectionPanel = false;
        }
        public static void WriteReport(string recordingPath)
        {
            if (snapshot == null || reportWritten || string.IsNullOrEmpty(recordingPath)) return;
            try
            {
                File.WriteAllText(Path.Combine(recordingPath, "performance.json"), JsonUtility.ToJson(snapshot, true));
                reportWritten = true;
            }
            catch (Exception ex)
            {
                reportWritten = true;
                Debug.LogWarning("424 performance audit could not be saved: " + ex.Message);
            }
        }
        public static void Restore(bool restoreComponents)
        {
            if (snapshot == null)
            {
                string saved = SessionState.GetString(Key + "snapshot", "");
                if (string.IsNullOrEmpty(saved)) return;
                snapshot = JsonUtility.FromJson<Snapshot>(saved);
            }
            if (restoreComponents)
                foreach (int id in snapshot.disabled)
                {
                    var component = EditorUtility.InstanceIDToObject(id) as Behaviour;
                    if (component != null) component.enabled = true;
                }
            var bridge = EditorUtility.InstanceIDToObject(snapshot.panelId) as UnityLiveLink;
            if (snapshot.panelManaged && bridge != null) bridge.showConnectionPanel = snapshot.originalPanelVisible;
            JobsUtility.JobWorkerCount = snapshot.originalWorkers;
            QualitySettings.vSyncCount = snapshot.originalVSync;
            Application.targetFrameRate = snapshot.originalFrameRate;
            OnDemandRendering.renderFrameInterval = snapshot.originalRenderInterval;
            AudioListener.pause = snapshot.originalAudioPause;
            SessionState.SetString(Key + "lastAudit", JsonUtility.ToJson(snapshot, true));
            SessionState.SetString(Key + "snapshot", "");
            snapshot = null; reportWritten = false;
        }
        [MenuItem("PERRINN/Copy 424 simulation performance audit")]
        static void CopyAudit()
        {
            EditorGUIUtility.systemCopyBuffer = snapshot != null ? JsonUtility.ToJson(snapshot, true) :
                SessionState.GetString(Key + "lastAudit", JsonUtility.ToJson(Inspect(), true));
        }
    }
}
