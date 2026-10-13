#!/bin/zsh
set -e
cd "$(dirname "$0")/../.."
perrinn_unity_contents="${UNITY_CONTENTS:-/Applications/Unity/Hub/Editor/2021.3.45f2/Unity.app/Contents}"
mkdir -p /tmp/perrinn-live-link
"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -langversion:latest -out:/tmp/perrinn-live-link/TransportHarness.exe 'Assets/Features/Unity Live Link/LocalWebSocketServer.cs' Tools/UnityLiveLinkTests/TransportHarness.cs
node Tools/UnityLiveLinkTests/transport.test.cjs
"$perrinn_unity_contents/MonoBleedingEdge/bin/mcs" -langversion:latest -out:/tmp/perrinn-live-link/RecordingHarness.exe 'Assets/Features/Unity Live Link/SessionRecording.cs' Tools/UnityLiveLinkTests/RecordingHarness.cs
"$perrinn_unity_contents/MonoBleedingEdge/bin/mono" /tmp/perrinn-live-link/RecordingHarness.exe
node Tools/UnityLiveLinkTests/client-service.test.cjs
