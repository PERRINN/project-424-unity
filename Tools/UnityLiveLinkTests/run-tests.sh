#!/bin/zsh
set -e
cd "$(dirname "$0")/../.."
perrinn_unity_contents="${UNITY_CONTENTS:-/Applications/Unity/Hub/Editor/2021.3.45f2/Unity.app/Contents}"
mkdir -p /tmp/perrinn-live-link
"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -langversion:latest -out:/tmp/perrinn-live-link/TransportHarness.exe 'Assets/Features/Unity Live Link/LocalWebSocketServer.cs' Tools/UnityLiveLinkTests/TransportHarness.cs
node Tools/UnityLiveLinkTests/transport.test.cjs
"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -langversion:latest -out:/tmp/perrinn-live-link/ShutdownHarness.exe 'Assets/Features/Unity Live Link/LocalWebSocketServer.cs' Tools/UnityLiveLinkTests/ShutdownHarness.cs
"$perrinn_unity_contents/MonoBleedingEdge/bin/mono" /tmp/perrinn-live-link/ShutdownHarness.exe
"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -langversion:latest -out:/tmp/perrinn-live-link/RecordingHarness.exe 'Assets/Features/Unity Live Link/SessionRecording.cs' Tools/UnityLiveLinkTests/RecordingHarness.cs
"$perrinn_unity_contents/MonoBleedingEdge/bin/mono" /tmp/perrinn-live-link/RecordingHarness.exe
node Tools/UnityLiveLinkTests/client-service.test.cjs

"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -out:/tmp/perrinn-live-link/TelemetryRatiosHarness.exe 'Assets/Features/Unity Live Link/TelemetryRatios.cs' Tools/UnityLiveLinkTests/TelemetryRatiosHarness.cs
"$perrinn_unity_contents/MonoBleedingEdge/bin/mono" /tmp/perrinn-live-link/TelemetryRatiosHarness.exe

"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -langversion:latest -r:System.Web.Extensions -out:/tmp/perrinn-live-link/EditorControlHarness.exe Tools/UnityLiveLinkTests/EditorControlHarness.cs 'Assets/Features/Unity Live Link/Editor/TelemetryEditorControl.cs' 'Assets/Features/Unity Live Link/Editor/TelemetrySimulationPerformance.cs'
"$perrinn_unity_contents/MonoBleedingEdge/bin/mono" /tmp/perrinn-live-link/EditorControlHarness.exe

"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -langversion:latest -r:System.Web.Extensions -out:/tmp/perrinn-live-link/SessionRunHarness.exe Tools/UnityLiveLinkTests/SessionRunHarness.cs 'Assets/Features/Unity Live Link/UnityLiveLink.cs' 'Assets/Features/Unity Live Link/TelemetryRatios.cs'
"$perrinn_unity_contents/MonoBleedingEdge/bin/mono" /tmp/perrinn-live-link/SessionRunHarness.exe

python3 -B Tools/UnityLiveLinkTests/performance-analysis.test.py
