# 424 Telemetry

Local, bidirectional WebSocket connection between a running PERRINN 424 car and client-web. Intended for Unity 2021.3 Editor or desktop builds on macOS and Chrome. No new package dependencies or cloud relay. Telemetry is recorded locally; setup changes remain session-only.

## Start with Autopilot from the web app

Keep this project open in Unity with the intended driving scene loaded. Once after installing/recompiling the Editor controller, choose **PERRINN → Copy 424 Telemetry Editor pairing token**. Paste that token into 424 Telemetry and choose **Start simulation**. Pairing is remembered in this browser; later starts need only that button. The connection panel shows Start/Stop simulation and Open saved session. The pairing form appears after requesting Start simulation when the Editor is detected and no valid pairing is saved. It stays hidden while the Editor is unavailable. Rejected Editor authentication clears the saved token; temporary presence-check failures retain it for automatic reuse when Unity returns. Start simulation handles the runtime telemetry connection automatically.

Start simulation stays disabled until this project's Editor controller answers an `editorProbe` with `editorAvailable`. The app checks every three seconds and displays Editor availability. This origin-restricted, unauthenticated presence response contains no credentials or simulation data and grants no control; starting still requires pairing.

An Editor-owned loopback WebSocket listener on `127.0.0.1:42482/live` stays available outside Play Mode. It requires the existing allowed origins and its own per-project Editor pairing secret. The secret lives in local EditorPrefs and the paired browser's `perrinn.telemetry.editorToken` localStorage entry, never Firebase or bundled assets. Successful authenticated starts return the current temporary telemetry token only after Autopilot is on. Runtime telemetry still uses port 42480 and its per-session token. No new service or launcher is installed.

The controller selects the chosen car (or the only car in the loaded scene), validates its enabled Autopilot and recorded lap, enters Play Mode, temporarily allows background execution so the first dynamics row can initialize while the browser is focused, enables telemetry, resumes the simulation and calls Autopilot's existing ToggleStatus action only if it is off. This preserves its alignment checks and matches the keyboard controller's force-feedback setting. App-started simulations stay muted (`AudioListener.volume = 0`) at 10× Unity time (`Time.timeScale = 10`), including after domain reloads. Physics timestep settings remain unchanged; achieved wall-clock speed depends on available CPU capacity. The previous audio volume, time scale and background setting are restored after Play Mode ends. It never supplies manual driving inputs or changes saved scene settings. Invalid/missing lap data and refused Autopilot startup produce errors rather than a connected success state.

Start request IDs and selected-car identity survive Unity domain reloads through Editor SessionState. The browser reconnects the Editor channel and retries the same operation ID during reload; an already-processed start never toggles Autopilot off or starts a stopped operation again. Startup has a 90-second deadline. Stop simulation exits Play Mode, finishes the recording and disconnects telemetry; the Editor listener remains available for the next Start.

## Run in the Editor

1. Open a driving scene with a `Perrinn424CarController` and enter Play Mode.
2. Choose **PERRINN → Enable 424 Telemetry in Play Mode**. If multiple cars exist, select the intended car first. The menu adds a temporary component to the running car; it does not modify the saved scene.
3. Copy the session pairing token from the car's **424 Telemetry** inspector or its small on-screen panel.
4. In the sibling `client-web` project, run `npm start`, then open `http://localhost:4200/424-telemetry` in Chrome. Paste the token and connect. Grant Chrome local-network access if requested.
5. Configure the two test rows in Session before starting an app-controlled simulation. Each row selects a parameter with an offset in its physical unit or a dimensionless gain. A reply confirms the applied value and the first dynamics frame that can use it.

For desktop builds, add `UnityLiveLink` to the car before building. The connection panel displays the session token and provides a copy button. It can be hidden with `showConnectionPanel`. Keep one bridge active per port. Build startup must be checked on the target macOS machine; Editor compilation is not a standalone-build validation.

The hosted `https://perrinn.com/424-telemetry` route is available only after a separate client deployment. It connects to this same localhost endpoint, not Firebase. Test it in the intended Chrome version: HTTPS-to-loopback WebSocket policy and local-network permissions can differ from the local development page. The bridge provides `ws://`, not TLS. It does not require disabling browser security; if Chrome blocks the hosted connection, use the local development page until a compatible secured connection is implemented.

## Data and timing

Run In Background stays disabled in project Player settings. Enabling the live-link component temporarily sets `Application.runInBackground = true`; disabling it restores the previous runtime setting. Switching to Chrome therefore keeps the simulation and telemetry running while the bridge is enabled. Browser disconnect does not disable the bridge. This does not override the Editor Pause button or machine sleep.

The catalogue starts with Speed, Throttle, Brake, SteeringAngle, RideHeightFront, RideHeightRear, UndersteerAngle, MotorRpmFront, MotorRpmRear and ElectricalPowerTotal. The bridge reads these existing VPP channels and applies their display-unit multipliers. It appends PositionX, PositionY and PositionZ in metres, sampled from the car’s Rigidbody world position (or Transform if no Rigidbody is present). Unity X/Z form the ground plane; Y is elevation. The browser map projects X/Z with equal scale, matching the existing Unity track-map convention. AeroFrontFlap and AeroDrsPosition are appended after AutopilotDistanceError, preserving existing channel indices. The aero provider supplies flap angle in degrees and DRS position in percent. The client places Flap angle and DRS immediately before the aero coefficients, with DRS fixed to 0–100%. Older recordings retain their original channel catalogue. Generic and MGU telemetry providers must emit these channels in the scene. Missing or inactive data is `null`, never fabricated or serialized as NaN. The welcome message reports each source frequency and unit; providers should be registered before connecting. Reconnect to refresh the catalogue after enabling additional providers.

VPP's `onPreDynamicsStep` applies accepted commands before model processing. `onDynamicsStepCompleted` samples after VPP updates its telemetry row, with a 50 Hz target in simulation time. Paused status/snapshots and commands use regular Unity Update when the vehicle or time scale is paused. The Editor Pause button suspends regular update handling; resume/reconnect afterward. Reduced time scale reduces wall-clock telemetry frequency; slow frames or a slow browser can also reduce received frequency.

The background socket workers never access Unity objects. Inbound messages, commands and replies have bounded queues. Outbound telemetry keeps only the latest unsent snapshot. A separate recording keeps every captured sample, regardless of browser connection. Protocol 2 history requests recover missing packets in batches of up to 500 rows and 100 events; only one history request is in flight. Reconnecting to the same session preserves the browser history. A new bridge session resets it.

## Full-session recording and analysis

Recording starts with the bridge's first sample, including samples before browser pairing. Each enable creates a new folder under `Application.persistentDataPath/LiveLink/<UTC timestamp>-<session ID>`. The exact folder appears in the Unity inspector. On macOS the parent is normally in `~/Library/Application Support/<company>/<product>`; use the displayed path rather than guessing the company/product identifiers.

- `session.json`: protocol, channel units/source rates, initial setup, session ID, folder path, final sample/event counts and completion flag. It never contains the pairing token.
- `samples.bin`: little-endian `ULL2` magic, Int32 channel count, followed by rows of `25 + 4 × channel count` bytes (185 bytes for new 40-channel sessions; 149 bytes for the original 31 channels; 77 for thirteen-channel sessions; 65 for ten-channel sessions): Int32 sequence, Int32 frame, Float64 elapsed real seconds since bridge enable, Float64 simulation seconds, one byte paused, one Float32 value per channel. Missing channel values use IEEE NaN on disk and null in JSON.
- `changes.jsonl`: successfully applied parameter changes, including event ID, elapsed time, next sample sequence, value and first effective dynamics frame. Rejections are reported to the caller, without an applied-change marker.

Disk IO uses a background worker with a bounded 4096-item queue, flushing buffered output about every 50 entries or after one idle second. Disabling the bridge/ending Play Mode drains the queue and marks the metadata complete. Disk errors/overflow are visible in the browser and inspector; such archives remain incomplete, while in-memory history remains available. A crash can lose buffered output; these are ordinary local files, not crash-proof storage. Retention is manual: recording folders are not automatically deleted.

The browser keeps the full captured session in append-only typed buffers. Angular refreshes at 10 Hz; the speed overview refreshes at most once per second. Charts use connected first/min/max/last sample reduction in chronological order, preserving isolated spikes and flat sections. Reduction density follows actual chart width and device pixel ratio (capped at 4096 buckets); SVG coordinates remain fixed. Null values and capture gaps split continuous segments even inside one bucket. Do not replace this with disconnected vertical min/max strokes: those make constant and slowly varying signals disappear or appear dotted. Small windows show original samples, break lines at missing channel values or recording gaps, and share an elapsed-real-time axis. Each channel learns its minimum and maximum across the entire loaded session, ignoring unavailable values. Zoom/pan retain this Y range; newly captured extremes expand it, and a new session resets it. Overview traces and cursor dots use the same learned range. Charts label elapsed time on the X axis and uniform rounded values on the Y axis. A shared axis helper chooses 1/2/2.5/5-based steps with at most five labeled levels, rounds outward, and centers substantially bipolar signals around zero. Tiny sign noise near zero does not add a wasteful whole negative interval, while the plot domain still contains every recorded sample. Constant channels receive padding. Grid lines match the labels and the zero line has subtle extra emphasis. Sample-rate diagnostics are omitted from the UI. Units appear in chart headings. Labels sit outside the SVG plot so they retain readable font sizes and cursor coordinates exclude the label gutter. Narrow mobile views omit the middle X label to avoid overlap. Gold lines mark parameter changes; red spans mark intervals over 100 ms with no captured samples. Paused snapshots carry the pause flag. Lower wall-clock sampling rates (such as reduced time scale) can therefore appear as gaps; network loss alone is recovered from Unity and does not manufacture a recording gap.

Drag the overview window to pan or its edges to resize. Double-click the navigation chart (Enter on the keyboard) to follow the newest 30 seconds. Arrow keys pan, plus/minus zoom and Home shows the full session; capture continues while inspecting an older range. Hovering any channel chart shows a synchronized cursor, original stored channel values with adaptive display precision and elapsed/simulation time at hundredth-second resolution and dynamics frame. Lookup uses binary search over full-resolution samples, without interpolating or reading the reduced trace. Null values stay unavailable; hovering inside a capture gap shows no sample. During live following the cursor tracks the pointer position as the time window advances. Drag horizontally across a channel chart to select a time or distance window and zoom all channels to that range. The selection band appears only on the chart being dragged; pointer capture keeps dragging active outside the chart and clamps it to the visible range. Small pointer movements do not zoom. Double-click a channel chart, or press Home while it is focused, to restore the full session. Vertical touch scrolling remains available. Zoom in/out centers on the current window. Cursor values are transient: leaving or blurring a chart clears them. Focus a chart and use left/right arrows to step through recorded samples. There is no persistent cursor pin. Parameter events appear as chart markers and remain in the local recording; the UI omits the event log and routine recording diagnostics. Browser memory is temporary and clears on page reload. While the bridge is still enabled, a new connection reloads its full history. After stopping the bridge, use Open a saved session folder and choose the timestamped session folder (not the parent LiveLink folder) to inspect every final sample. The folder picker automatically reads its three recording files; nested folders and unrelated files are ignored. Files are parsed locally without upload.

For the original 10 channels/50 Hz, 30 minutes is 90,000 rows: 5,850,008 bytes on disk and 5,496,912 bytes of browser telemetry buffers. Unity also retains samples in memory to serve catch-up; its object overhead is higher. A 13-channel recording uses 6,930,008 disk bytes and 6,578,280 browser-buffer bytes for the same duration. The importer reads the channel count from each file and still supports the original sample. No fixed session-duration limit is imposed. Memory and disk usage grow with duration, so substantially larger channel counts/rates or multi-hour sessions should move catch-up serving to disk-backed chunks.

## Parameters and lifetime

- `frontSpringRate`: N/m, applies equally to both front wheel colliders. Supports VPKinematicWheelCollider and VPStandardWheelCollider. Kinematic damper settings are copied before mutation; RefreshPhysics updates the solver's settings reference. The command response reads the solver's spring rate. Original left/right settings are restored independently when the bridge disables.
- `autopilotPositionOffset`: metres, affects Autopilot's timing/reference position offset. It is not a lateral displacement command. The car's existing Autopilot component must be present. Original offset is restored when the bridge disables.

Prototype input guards are 10,000–1,000,000 N/m and −10–10 m. These are validation bounds, not established vehicle operating limits. No car settings assets are overwritten; initial values and applied changes are included only in the session recording. Browser disconnect leaves the applied session settings in place. Disabling the bridge or ending Play Mode restores originals. An abrupt process termination simply discards runtime state.

## Protocol version 2

Endpoint: `ws://127.0.0.1:42480/live`. Loopback bind only. Allowed HTTP Origin values are configured on the Unity component; defaults cover local development on port 4200, the localhost Firebase emulator on 5004, and HTTPS perrinn.com/www.perrinn.com. Origin checking and the random per-session token both gate browser commands. Tokens are held only in memory, not URLs, logs or browser storage. One active browser connection is supported.

Browser first sends:

```json
{"type":"hello","protocol":2,"token":"<session token>"}
```

Unity sends `welcome` with session ID, rateHz, ordered channel descriptors and parameter descriptors/current values. Values in telemetry align with this catalogue:

```json
{"type":"telemetry","recordingError":{"error":null},"sample":{"sequence":123,"frame":1230,"time":2.46,"simulationTime":2.46,"paused":false,"values":[42.1,0.6,0,2.1,65,70,0.2,1500,1500,120]}}
```

History request (after = number of contiguous samples already stored, eventAfter = stored event count):

```json
{"type":"history","after":0,"eventAfter":0,"limit":500}
{"type":"history","total":90000,"eventTotal":6,"samples":["<up to 500 sample objects>"],"events":["<up to 100 event objects>"]}
```

A command and response:

```json
{"type":"setParameter","requestId":"<unique id>","parameter":"frontSpringRate","value":230000}
{"type":"result","requestId":"<unique id>","parameter":"frontSpringRate","ok":true,"value":230000,"effectiveFromFrame":1231,"error":null}
```

Invalid commands return `ok:false` with an error; malformed protocol messages return `type:error`. Commands require an explicit finite numeric value and a bounded request ID. The last 32 command replies are cached per connection for duplicate request IDs; queued duplicates are also suppressed. This is not durable exactly-once delivery across reconnects. Browser reconnect reads authoritative current values and never replays writes.

Browser sends `ping` every second and Unity replies `pong`. Inactive/unpaired connections expire after five seconds. Writes time out after two seconds. Client commands time out after three seconds and report an unconfirmed outcome rather than assuming success. Editor/network interruptions can leave an outcome unconfirmed; reconnect before retrying.

## Validation

Run `zsh Tools/UnityLiveLinkTests/run-tests.sh` from the Unity project (requires the installed Unity Mono runtime and sibling client-web node_modules).

`Tools/UnityLiveLinkTests` includes transport integration checks and tests of the actual Angular service with a mock socket. Transport checks cover origin/path rejection, fragmentation, text round trips, latest-snapshot publishing, one-client enforcement, oversized/binary rejection and reconnect. Service checks cover pairing, freshness gating, full history, range validation, confirmed/rejected commands, no command replay, reconnect catch-up, duplicate batches and session reset. The recorder harness generates 90,000 rows, an isolated spike, missing values, a two-second gap and six changes; verifies every binary row after flush; and transfers the generated batches into the actual client storage/service. It also checks completed-file import, incomplete/mismatched files and cancellation. Rendering regressions cover connected constant signals, ordered extrema, spikes at the final boundary, nulls/capture gaps inside a coarse bucket, and increasing detail with display resolution.

The 30-minute synthetic benchmark on this machine used about 18.6 MB of JSON for a full catch-up, took approximately 80 ms to ingest in Node, and, after the rendering-quality fix, prepared ten full overview traces at 1200 buckets per channel in about 24 ms median / 27 ms p95. These timings measure processing, not Unity driving performance or Chrome frame rate. The generated session was also loaded in the in-app browser to check overview, zoom, gaps/events and completed-file import at narrow and desktop widths. Unity/runtime and editor code compiled against installed Unity/VPP assemblies; targeted Angular lint and production build passed. A real 30-minute driving run and standalone-build validation remain separate checks.

## Bundled browser sample

client-web includes the completed real recording `20261015-143056-cbc2da42fe9d413ba180490d503f8cdc` as `src/assets/unity-live-link/sample/`. Opening 424 Telemetry automatically loads its 45,165 samples (three completed laps with two front-spring-rate changes), including all 43 channels with trajectory error, flap angle and DRS and world PositionX/PositionY/PositionZ for the track map, through the same validated recording importer, without a WebSocket connection or local folder picker. The sample is read-only, uses all chart/cursor tools, and works on machines serving the web app without Unity. Binary samples and events are copied unchanged; metadata replaces the original machine-specific recording path with “Bundled sample session”. Assets are fetched when the page opens; the existing production service worker caches them after first use. Production availability requires a separate web-app deployment.

Display precision is derived per channel from its learned full-session range: one decimal place for normal spans, two for spans around 0.1 units, three around 0.01, and so on. Constant channels use their value magnitude. Very small spans use scientific notation; negative values that round to zero display as zero. Live values, cursor values, Y ticks and range captions share this formatting. Precision follows newly learned extrema but is independent of zoom/pan. Raw samples, ranges, chart geometry and disk recordings are never rounded by display formatting.

## Session navigation

The sticky session navigation area combines a recorded-trajectory track map, speed overview and clickable Time/Distance axis label. It remains visible while scrolling through the stacked channel charts, on mobile and desktop. Drag the speed overview’s selected window to pan without changing its duration; click outside it to recenter, or use left/right arrows while focused. Pointer capture preserves dragging outside the plot; release/cancel ends it. The old navigation slider and overview heading are removed.

The map fits the whole recorded trajectory without stretching X/Z, breaks at missing coordinates or recording gaps, and shows the shared cursor position (or the latest live position when there is no cursor). Click the trajectory to pin its nearest recorded sample; a selection outside the visible chart window pans that window to include it. Repeated spatial matches prefer the current cursor time. Old recordings without PositionX/PositionZ display a placeholder. The bundled real sample now includes these coordinates; older ten-channel sessions remain readable with a map placeholder.

The Angular shell permits overflow through its content wrappers only for this route, allowing sticky navigation to use main_container’s existing scroll area. Position channels follow the same metadata/binary catalogue contract as other channels; the client accepts 1–64 channels and validates sample lengths against that catalogue. Protocol and ULL2 magic remain version 2. An older deployed client that requires ten channels must be updated before connecting to the new bridge.

## Expanded analysis catalogue

New sessions now append 18 channels to the original 13, preserving the earlier indices. There are 31 recorded channels and 29 charts (world X/Z remain map-only). Existing recordings retain only their originally captured channels; the bundled sample now includes all 40 channels; restart the bridge and record again to capture the additions. Missing or inactive providers leave their channels unavailable.

Charts are ordered as driver inputs → accelerations → chassis → tire loads / differential speeds → powertrain → aero → altitude. Client presentation names and order live in client-web’s `chartChannels.ts`; sorting never changes sample indices.

| Analysis | Source channels and interpretation |
| --- | --- |
| Electrical power | ElectricalPowerFront, ElectricalPowerRear, ElectricalPowerTotal; existing MGU telemetry in kW |
| Wheel speed differences | RpmDiffFront, RpmDiffRear; signed left minus right in rpm |
| Accelerations | AccelerationLong, AccelerationLat, AccelerationVert; VPP telemetry with its reported display units and coordinate convention |
| Tire vertical forces | WheelLoadFL, WheelLoadFR, WheelLoadRL, WheelLoadRR; contact-normal loads, explicitly shown in N |
| Front electrical / wheel torque share | PowerBalanceDelivered and WheelTorqueBalance, recalculated as 100 × front / (front + rear); signed totals preserve regeneration and opposing axle flows. Zero or negligible totals produce NaN/null, rather than an invented 50%. Wheel torque uses the MGU’s delivered front/rear WheelsTorque values. |
| Chassis roll | AeroRoll; the aero model’s average of front/rear ground-relative chassis roll angles |
| Drag and front/rear downforce coefficients | AeroScx, AeroSczFront, AeroSczRear; the model uses area-weighted SC coefficients in m², multiplied by dynamic pressure to obtain forces. No reference area is assumed to convert these into dimensionless Cd/Cl. |
| Aero balance | AeroBalance; the existing front share as a percentage |

`TelemetryRatiosHarness.cs` covers front/total calculations including regeneration, opposing flows, invalid values and undefined totals. The client regression checks the expanded Unity catalogue against every ordered chart and exercises 31-channel streaming and binary import without reindexing values.

## Production page and timing channels

424 Telemetry is a production app at `/424-telemetry`, linked from Apps. The former prototype URL redirects to it. The component's serialized Unity class and the existing `LiveLink` recording directory remain compatible with saved scenes and recordings.

New sessions append LapDistance (metres), LapTrigger, SectorTrigger, SectorIndex, SectorTime (seconds), SectorValid, LapTime (seconds), LapValid and LapCompleteTrigger to the original 31 channels. Trigger values are incrementing counters, not transient pulses. SectorIndex is one-based. Durations and validity come directly from LapTimer events. Every event captures an immediate recording row as well as the regular 50 Hz samples, preserving the final sector, completed lap and next lap start in order. Result channels remain latched until their next event. Missing LapTimer data remains unavailable. Disabling the bridge unsubscribes its timing listeners.

The browser derives the lap list from those counters during both live history recovery and file import. It omits the initial partial lap when capture starts mid-lap and labels unfinished laps incomplete. Clicking a lap selects its time window. The sticky Time/Distance label changes all chart coordinates, tick units, parameter markers, capture gaps and cursor/navigation lookup together. Distance uses resetting LapDistance in metres to overlay laps. The overview and detailed charts share a lap-distance window. The shared cursor finds one original nearest sample per lap and shows lap-labelled values; gaps and incomplete distance ranges stay blank. An active lap controls the map dot and keyboard stepping, and determines the sample when switching to time mode. Older archives without distance/timing data remain readable with explicit unavailable states. The bundled sample now includes lap distance and timing data for two completed laps and a partial third lap. No old sample values are fabricated.

## Stop simulation from the browser

The connection panel's Start simulation button becomes Stop simulation once runtime telemetry connects. Stopping is enabled only for a paired live bridge advertising canStopSimulation. The authenticated stopSimulation command includes a request ID. Unity captures a final sample, flushes/finalizes the recording and waits for its simulationStopped acknowledgement to leave the socket worker before exiting Editor Play Mode or quitting the standalone application. The button shows Stopping… and disables writes while stopping, then returns to Start simulation after acknowledgement. The browser disconnects without reconnecting and preserves loaded history. Lost acknowledgements/timeouts are reported as unconfirmed; stop commands are never replayed automatically.

Editor startup validation includes the actual coordinator compiled against simulated Unity lifecycle APIs (authentication, domain reload, idempotent Play entry, Autopilot enablement, missing/refused Autopilot and runtime-token handoff). Browser service tests cover remembered pairing, duplicate clicks, reload reconnect, operation matching and rejected startup. Compilation uses the installed Unity/VPP assemblies. These checks do not substitute for an interactive Play Mode run on the target machine.

## Three-lap Session

Session shows three fixed timing rows: Baseline, Test 1 and Test 2. Lap and sector differences use the first lap as the reference; negative differences are green and positive differences red. Invalid or missing times have no comparison. Sector columns follow the recorded track, with three placeholders before timing arrives. Lap selection still controls chart inspection. The table scrolls horizontally on narrow screens with lap identity pinned.

The authenticated Editor start request carries two `laps` entries (`parameter`, `mode` of `offset`/`gain`, and `amount`), retained across domain reloads. Unity validates the plan against the actual original setup before enabling Autopilot. Each test target is original + offset or original × gain. Changes apply inside the lap-start callback before the following dynamics step. A parameter used only by the preceding test returns to baseline and that restoration is recorded. Plans lock during the run. Runtime welcomes and session metadata include `lapPlan` and `baselineParameters`; existing archives remain readable.

At lap 3 completion Unity captures the final timing event, pauses the car and sends `sessionComplete` with final sample/event counts. It suppresses lap 4 and further snapshots. The browser recovers those final rows and events, then uses the existing correlated stop command to finalize and exit Play Mode, retaining the charts. If the browser is absent or cannot finish, Unity finalizes and exits after 15 seconds. Manual bridge sessions retain their existing command behavior.

`SessionRunHarness.cs` executes the real bridge with deterministic timing/suspension APIs, covering range validation, lap boundaries, independent baseline targets, parameter restoration, completion and disconnected shutdown. Browser regressions cover signed/invalid comparisons and waiting for final history before automatic stop.

## Automatic simulation performance mode

New app-started runs apply `telemetry-fast-v1` before the first dynamics row: reviewed Unity telemetry displays, camera/Cinemachine controls, dashboards/UI and audio work are disabled temporarily, VSync/frame limiting is removed, and rendering is reduced to every eighth frame. Vehicle physics, Autopilot, timing and telemetry providers retain their normal settings and the 2 ms physics timestep. Manual Play Mode and already-running sessions are unaffected. Startup failure and Play Mode exit restore original settings; the profile survives assembly reload.

The runtime component audit is saved as `performance.json` beside the recording and is available from PERRINN → Copy 424 simulation performance audit. No new browser controls or diagnostic blocks are added. See [PERFORMANCE_AUDIT.md](PERFORMANCE_AUDIT.md) for audited dependencies, exclusions and measurement instructions. `python3 Tools/UnityLiveLinkTests/analyze-performance.py BASELINE OPTIMIZED` compares actual recorded speed without browser/network receipt timing.

### Trajectory error and ride-height scatter

New recordings append `AutopilotDistanceError` to the catalogue without changing existing indices, using the existing Autopilot provider's display multiplier (metres to millimetres). The client labels it Distance error to trajectory; older recordings without it show an unavailable chart. The final scatter plot uses front ride height on X and rear ride height on Y. Each dot pairs original values from a single stored sample; null pairs are omitted. It follows the selected time or lap-distance range, with matching lap colours in distance mode, and reuses the exact full-session bounds and ticks from each ride-height chart. No interpolation, connecting lines or independent axis rescaling is applied.

The ride-height scatter supports transient cursor readouts: hover a dot to show original paired values and synchronize the other charts, or hover another chart to highlight its paired ride-height values. Distance mode shows corresponding per-lap values. Leaving or blurring clears the cursor; arrow keys step recorded samples while focused.

The trajectory-error axis is fixed at −50 to +50 mm in both time and distance views, including startup and every zoom level. Original values remain recorded and inspectable, with out-of-range values marked at the chart boundaries. No lap-start exception is applied.

Channel charts are organized into compact engineering groups: Session & trajectory, Driver inputs, Vehicle dynamics, Chassis, Tire loads, Powertrain and Aerodynamics; the final ride-height scatter sits under Correlations. Muted labels and thin rules separate groups. This is presentation-only grouping: recorded catalogue indices and shared chart navigation stay unchanged.

Distance-view traces reuse the exact resolved channel axis used for labels, cursor dots and scatter scaling. Rounding a channel's bounds again can change its signed-range centring and produce visible cursor/trace offsets; the client avoids that second rounding pass. Regressions cover front/rear downforce and both ride-height channels in the bundled real recording.
