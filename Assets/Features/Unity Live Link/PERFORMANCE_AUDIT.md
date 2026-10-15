# App-controlled simulation performance audit

The default browser Start simulation flow now applies `telemetry-fast-v2`. It is scoped to an authenticated, app-started run; manually entering Play Mode and simulations already running when this code is installed are unchanged. No scene or prefab is saved or modified.

## Evidence and scope

The audit inspected `424 Nordschleife Scene.unity`, its referenced car/UI/camera prefabs, the presentation scripts and VPP component lifecycle methods in the installed SDK. The first pass was a dependency and component inventory audit. The subsequent CPU profiling and worker scheduling investigation is recorded below. Runtime `performance.json` records the actual enabled component inventory and the exact list disabled for each run, rather than assuming every prefab component is active.

The completed recording `20261015-113845-27124c605bc1472891e7f358398cc895` averaged **4.330×** after excluding its first five real seconds: 94.474 simulation seconds in 21.820 real seconds. Its median five-second speed was 4.329×. That is the baseline used for the measured comparison below.

## Automatically disabled

| Work | Evidence and action |
| --- | --- |
| Unity graphical telemetry | `VPTelemetryDisplay.OnEnableVehicle` allocates a dataset and subscribes to `onDynamicsStepCompleted`; `OnDisableVehicle` removes the subscription. Disable it, `VPTelemetryTools` and `VPPerformanceDisplay`. The bridge uses `car.telemetry` directly, independently of these displays. |
| Dashboard and scene UI | Disable the reviewed Perrinn UI controllers, lap table listener, dashboard/value/timer displays, track-map UI, Canvas components and Unity UI behaviours. These update text, textures, layout and map icons. Merely hiding the Canvas would leave the scripts running. |
| Cameras | Disable loaded scene Cameras, Cinemachine behaviours and reviewed camera/input controllers. Do not deactivate their GameObjects, which may also contain simulation components. |
| Audio | Disable AudioSource/AudioListener components and the reviewed MGU, underfloor, cockpit and VPP audio updaters. Pause audio processing globally. Muting volume alone leaves updater work active. |
| Runtime pairing overlay | Hide the bridge's Unity IMGUI panel; pairing is already handled by the Editor/browser handshake. |
| Rendering waits | Temporarily set VSync to 0, targetFrameRate to -1 and renderFrameInterval to 8. Rendering is decoupled from physics; the fixed timestep stays at 0.002 seconds. |

The existing `TimeScaleCallbackHandler` can disable presentation in keyboard-controlled accelerated runs, but the app previously assigned `Time.timeScale` directly and bypassed that event. The new profile uses an explicit allowlist and exact restoration rather than triggering a callback that deactivates whole GameObjects.

## Deliberately retained

Vehicle controller, wheel colliders, Rigidbody/colliders, suspension, tyres, aero/underfloor physics, battery, Autopilot and its recorded reference, lap timer/sector marks/transponder, speed estimators, performance benchmark and all telemetry providers remain enabled. Unknown scripts remain enabled. No physics timestep, solver setting, capture channel, recording rate or requested 10× time scale is changed.

Replay and legacy lap/CSV exporters are not disabled in this first pass. Their lifecycle includes file finalization and replay/control state; treating them as presentation without a separate audit could affect capture or vehicle state. PhysicsCameraMode is also retained because its OnDisable reactivates referenced GameObjects. Hardware I/O is retained.

## Lifecycle and verification

Before the first dynamics row, the Editor snapshots enabled presentation components and global settings, then disables only those components. The snapshot survives assembly reload through Editor SessionState and is reapplied after reload. Disabled prefab assets, unloaded scenes and inactive objects are excluded. Normal Play Mode exit restores global settings; Unity restores the original scene. Startup failure restores the recorded components and the original time scale, audio volume and background setting. Initially disabled components remain disabled.

Each bridge recording folder receives a `performance.json` sidecar with the profile version, original pacing settings, physics timestep, enabled component counts and disable/retain decisions. It contains no pairing token. Existing importers ignore the additional file. The PERRINN → Copy 424 simulation performance audit menu copies the active or last audit; outside a profiled run it can inspect the current loaded scene without changing it.

`EditorControlHarness.cs` executes the actual profile and coordinator against lifecycle APIs. It covers the allowlist, retained providers/physics/unknown components, scene filtering, original disabled states, audit output, domain reload and failure/exit restoration. The Editor sources also compile against the installed Unity 2021.3/VPP assemblies.

## Measure the improvement

Compare recordings from the same scene, setup and lap plan with the same browser charts and other apps open:

```sh
python3 Tools/UnityLiveLinkTests/analyze-performance.py /path/to/baseline-session /path/to/optimized-session
```

The tool reads original binary wall/simulation timestamps, excludes the first five real seconds and paused intervals, and reports average and median five-second ratios, the profile and disabled component counts. `secondRunSpeedup` compares the second run to the first. It can also inspect a flushed snapshot of an ongoing recording without writing it. The browser's recent measured ratio remains available beside Start/Stop; it is not the configured time scale.

If the gain is limited, capture Unity Profiler CPU Timeline traces for comparable warm laps with Deep Profiling off. Compare Physics/FixedUpdate, VPP dynamics/Autopilot, telemetry capture/serialization, rendering waits, EditorLoop and GC. Attribute CPU time before adding more allowlist entries. A standalone batch run is a later option for removing Editor overhead, not part of this change.

Unity API references: [OnDemandRendering](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Rendering.OnDemandRendering.html), [renderFrameInterval](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Rendering.OnDemandRendering-renderFrameInterval.html), [targetFrameRate](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Application-targetFrameRate.html).

## Measured result — 2026-10-15

The optimized browser-launched run `20261015-115141-c5a4c9109b7d406288e6197663c70855` completed three valid laps and stopped automatically, with 45,395 samples and no parameter changes. The audit confirms 622 enabled presentation components disabled and fixedDeltaTime still 0.002 seconds. No script compilation or runtime exceptions were recorded during the profiled run. Channel descriptors, starting parameter values and the no-change lap plan match the baseline.

| Measurement | Previous run | Performance mode |
| --- | ---: | ---: |
| Same simulation-clock interval, approximately 21.70–116.17 s | 4.330× | 4.149× |
| Real time for that interval | 21.820 s | 22.765 s |
| Full optimized run, excluding first five real seconds | — | 4.162× |

The matched interval was 4.16% slower in this comparison. The full optimized measurement covered 886.712 simulation seconds in 213.053 real seconds; its median five-second ratio was 4.181×. The baseline covers a shorter run, so the matched interval is the fairer direct comparison. A preliminary unprofiled run immediately before the optimized run averaged 4.216×, indicating some run-to-run variation. These measurements show no demonstrated speed improvement from this presentation profile; they do not isolate a CPU bottleneck or establish a statistically reliable regression.

The next investigation should use CPU Timeline samples, including the Editor coordinator's per-update object lookups, the retained VPP/Autopilot/physics work, and the legacy CSV/replay paths. Do not infer CPU cost from the number of disabled components. The temporary preliminary run `20261015-115032-de8ebcf2cb5d4790a0232d870b8668ff` did not contain performance.json and was stopped before the actual profiled run; it must not be labelled an optimized result.


## CPU profiling and scheduling fix — 2026-10-15

A 15-second macOS native CPU sample and a bounded Unity CPU marker capture with Deep Profiling disabled both identified physics job synchronization as the primary limit. The original Editor used 11 Unity job workers. In the comparable Unity capture (`20261015-120633-71f16a6bfa684f4cba0a4667b683b422`), 18,334 physics steps accumulated 5,951 ms in `FixedUpdate.PhysicsFixedUpdate`, including 5,430 ms in `Physics.Simulate`. Main-thread `Semaphore.WaitForSignal` accumulated 5,603 ms. These are overlapping elapsed timings, not additive CPU consumption. `VehicleBase.FixedUpdate` accumulated 1,900 ms inclusive / 1,038 ms self; the legacy CSV writer was much smaller. The native stacks independently showed `PhysicsManager::Simulate` waiting on job fences and substantial worker semaphore signalling. Preserve the vehicle calculations and reduce scheduling overhead first.

A compact copy of the main-thread marker results is in `Tools/UnityLiveLinkTests/cpu-profile-2026-10-15.json`. The raw native sample is `/tmp/424-cpu-before.sample.txt`; complete Unity markers are beside the diagnostic recording. Instrumented throughput is excluded from speed comparisons. A temporary capture helper was used only during this investigation and removed from the final source. Unity crashed in native `FileCacherRead` / `SerializedFile` teardown while leaving the diagnostic run; the Editor was restarted before the final benchmark. The trace does not establish the crash's cause.

The app-started `telemetry-fast-v2` profile uses one Unity job worker (bounded by the platform maximum). It snapshots and restores the previous count on normal exit and startup failure, and preserves it through assembly reload. It also caches the selected runtime car, replacing the coordinator's per-update slow GlobalObjectId resolution. No timestep, solver, telemetry frequency, vehicle model, ML estimator, replay or CSV export was removed or changed. The worker choice is measured for this single-car scene; heavier multi-car scenes should be profiled separately.

Exploratory runs with profiling off, using the same simulation interval of approximately 21.70–116.17 seconds:

| Worker count | Ratio |
| --- | ---: |
| 11 (previous presentation profile) | 4.149× |
| 2 | 6.882× |
| 1 | 7.117× |

The one-worker and two-worker recordings are `20261015-120438-81cef9d922634a3e8ea0d0fc2b9b17ec` and `20261015-120527-4edd05beaddb41b5ac0357b10b62c67f`. These short runs established the candidate setting; the completed final run below validates automatic operation. `performance.json` now records both `originalWorkers` and `workers`. The regression harness verifies worker restoration and that repeated running updates no longer perform slow scene-identifier lookups.

### Completed comparison

The final uninstrumented recording `20261015-120810-9ad51e4c13f34828b303986856510947` completed three valid laps and stopped automatically. It recorded `telemetry-fast-v2`, one worker (originally 11), the original 0.002-second timestep and no CPU capture. Channels, starting parameters and the no-change lap plan match both earlier runs. All three lap times are bit-for-bit identical in the binary telemetry to the previous three-lap run: 299.4486389160156, 299.3357849121094 and 299.3353271484375 seconds.

| Same simulation interval, 35.040–907.492 seconds | Previous mode | CPU scheduling fix |
| --- | ---: | ---: |
| Ratio | 4.161× | 7.019× |
| Wall time | 209.654 s | 124.304 s |

This is **68.66% more simulation throughput / 40.71% less wall time**. Full-run measurements excluding each recording's first five real seconds are 4.162× and 7.019×; medians over five-second windows are 4.181× and 7.047×. Against the original shorter 4.330× pre-mode run, the matched early interval reached 7.007×, a 61.85% gain. The same-scene repeated runs support this large gain; small differences between worker candidates remain subject to ordinary run variation. Machine-readable comparison results are in `Tools/UnityLiveLinkTests/cpu-comparison-2026-10-15.json`.

The Editor subsequently crashed in Mono JIT/debug metadata while repainting its Project Browser after leaving Play Mode. The complete recording was finalized before that crash. Normal restoration has been moved from `ExitingPlayMode` to `EnteredEditMode`, so worker and rendering settings remain consistent throughout scene/job teardown; startup-failure restoration remains immediate. The harness checks this boundary as well as restored worker counts, caching and reload behavior. This is a lifecycle hardening change; native crash causality is not established by the stack trace alone.


Repeat validation `20261015-121129-8fe44783e39d47509372c9878f997b86` completed another three valid laps (45,395 samples), averaging **7.042×** after five seconds of warmup, median **7.082×**. The Editor again crashed after Play Mode exit in native Mono/JIT/Editor repaint code. Deferring settings restoration did **not** resolve the crash. Both completed recordings remain usable; the Editor was restarted after the second crash. **Exit stability remains unresolved**, and this performance improvement must not be presented as fully stable. No further Unity version, debugger or project settings were changed speculatively.

## Exit crash investigation and transport shutdown — 2026-10-15

The transport previously launched the accept loop and per-connection tasks without retaining their completion. `Dispose` cancelled tokens and aborted the current socket, then returned immediately. It also left partially completed HTTP handshakes outside its shutdown inventory and never disposed completed WebSockets/semaphores. Both the runtime and Editor-owned transport could therefore leave network continuations or keepalive work alive when Unity reloaded its script domain. Moving render/worker restoration alone did not address this defect.

`LocalWebSocketServer` now owns the accept task, admitted TCP connections (including incomplete handshakes), and connection tasks. Shutdown cancels and closes I/O, joins the accept loop, then joins every admitted connection before disposing its cancellation source. Per-connection cleanup disposes the WebSocket and its keepalive resources only after both read/write loops have finished. Repeated disposal is harmless. Network awaits all use `ConfigureAwait(false)`, so the main thread can wait for them without requiring Unity's synchronization context. The runtime bridge now performs this shutdown after the stop acknowledgement is sent and before requesting Play Mode exit; `OnDisable` remains a fallback for other exit paths. The Editor transport uses the same disposal during assembly reload.

`ShutdownHarness.cs` runs the actual transport under the bundled Mono runtime through 60 shutdown/rebind cases: idle listener, incomplete HTTP headers, active WebSocket, queued reply receipts and repeated disposal. It verifies that all owned tasks have completed when disposal returns and sockets close promptly. The session harness checks that application exit follows network shutdown. The full regression suite passed after the change. Physics/worker settings and telemetry capture remain unchanged.


### Clean-exit verification

After the transport fix, Unity process 46287 completed a manually stopped smoke run (`20261015-122621-1bae3993adce4f85ae322ea96a5c209c`) and then a full three-lap automatic run (`20261015-122702-4f8dab79f4d544bda0d76cc968292d55`) without restarting the Editor. All three timing rows were complete, all laps valid, with exactly the same lap times as the previous recording. The Editor returned to Edit Mode and its authenticated-control listener remained available after both stops. No native crash, crash reporter launch or script compile error appeared in `/tmp/424-unity-shutdown-fix.log`. The same Unity process remained alive during the post-exit idle check.

The complete run recorded 45,395 rows and averaged **7.019×**, median five-second ratio **7.052×**, after excluding the first five real seconds. It retained one worker, a 0.002-second physics timestep, identical channels, parameters and lap plan. This preserves the approximately 69% throughput gain over the earlier 4.162× profile. Machine-readable validation is appended to `Tools/UnityLiveLinkTests/cpu-comparison-2026-10-15.json`. These manual and automatic checks resolve the previously reproduced exit failure in this environment; they are not a guarantee against every native Unity crash. The older unresolved-crash notes above describe the pre-fix investigation.
