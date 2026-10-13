# Unity Live Link prototype

Local, bidirectional WebSocket connection between a running PERRINN 424 car and client-web. Intended for Unity 2021.3 Editor or desktop builds on macOS and Chrome. No new package dependencies or cloud relay. Telemetry is recorded locally; setup changes remain session-only.

## Run in the Editor

1. Open a driving scene with a `Perrinn424CarController` and enter Play Mode.
2. Choose **PERRINN → Enable Unity Live Link in Play Mode**. If multiple cars exist, select the intended car first. The menu adds a temporary component to the running car; it does not modify the saved scene.
3. Copy the session pairing token from the car's **Unity Live Link** inspector or its small on-screen panel.
4. In the sibling `client-web` project, run `npm start`, then open `http://localhost:4200/prototypes/unity-live-link` in Chrome. Paste the token and connect. Grant Chrome local-network access if requested.
5. Use Apply to change both front springs or the autopilot position offset. A reply confirms the applied value and the first dynamics frame that can use it.

For desktop builds, add `UnityLiveLink` to the car before building. The connection panel displays the session token and provides a copy button. It can be hidden with `showConnectionPanel`. Keep one bridge active per port. Build startup must be checked on the target macOS machine; Editor compilation is not a standalone-build validation.

The hosted `https://perrinn.com/prototypes/unity-live-link` route is available only after a separate client deployment. It connects to this same localhost endpoint, not Firebase. Test it in the intended Chrome version: HTTPS-to-loopback WebSocket policy and local-network permissions can differ from the local development page. This prototype provides `ws://`, not TLS. It does not require disabling browser security; if Chrome blocks the hosted connection, use the local development page until a compatible secured connection is implemented.

## Data and timing

Run In Background stays disabled in project Player settings. Enabling the live-link component temporarily sets `Application.runInBackground = true`; disabling it restores the previous runtime setting. Switching to Chrome therefore keeps the simulation and telemetry running while the bridge is enabled. Browser disconnect does not disable the bridge. This does not override the Editor Pause button or machine sleep.

The fixed ten-channel catalogue is Speed, Throttle, Brake, SteeringAngle, RideHeightFront, RideHeightRear, UndersteerAngle, MotorRpmFront, MotorRpmRear and ElectricalPowerTotal. The bridge reads existing VPP channels and applies their display-unit multipliers. Generic and MGU telemetry providers must emit these channels in the scene. Missing or inactive data is `null`, never fabricated or serialized as NaN. The welcome message reports each source frequency and unit; providers should be registered before connecting. Reconnect to refresh the catalogue after enabling additional providers.

VPP's `onPreDynamicsStep` applies accepted commands before model processing. `onDynamicsStepCompleted` samples after VPP updates its telemetry row, with a 50 Hz target in simulation time. Paused status/snapshots and commands use regular Unity Update when the vehicle or time scale is paused. The Editor Pause button suspends regular update handling; resume/reconnect afterward. Reduced time scale reduces wall-clock telemetry frequency; slow frames or a slow browser can also reduce received frequency.

The background socket workers never access Unity objects. Inbound messages, commands and replies have bounded queues. Outbound telemetry keeps only the latest unsent snapshot. A separate recording keeps every captured sample, regardless of browser connection. Protocol 2 history requests recover missing packets in batches of up to 500 rows and 100 events; only one history request is in flight. Reconnecting to the same session preserves the browser history. A new bridge session resets it.

## Full-session recording and analysis

Recording starts with the bridge's first sample, including samples before browser pairing. Each enable creates a new folder under `Application.persistentDataPath/LiveLink/<UTC timestamp>-<session ID>`. The exact folder appears in the Unity inspector and browser. On macOS the parent is normally in `~/Library/Application Support/<company>/<product>`; use the displayed path rather than guessing the company/product identifiers.

- `session.json`: protocol, channel units/source rates, initial setup, session ID, folder path, final sample/event counts and completion flag. It never contains the pairing token.
- `samples.bin`: little-endian `ULL2` magic, Int32 channel count (10), followed by fixed 65-byte rows: Int32 sequence, Int32 frame, Float64 elapsed real seconds since bridge enable, Float64 simulation seconds, one byte paused, ten Float32 channel values. Missing channel values use IEEE NaN on disk and null in JSON.
- `changes.jsonl`: successfully applied parameter changes, including event ID, elapsed time, next sample sequence, value and first effective dynamics frame. Rejections are reported to the caller, without an applied-change marker.

Disk IO uses a background worker with a bounded 4096-item queue, flushing buffered output about every 50 entries or after one idle second. Disabling the bridge/ending Play Mode drains the queue and marks the metadata complete. Disk errors/overflow are visible in the browser and inspector; such archives remain incomplete, while in-memory history remains available. A crash can lose buffered output; these are ordinary local files, not crash-proof storage. Retention is manual: recording folders are not automatically deleted.

The browser keeps the full captured session in append-only typed buffers. Angular refreshes at 10 Hz; the speed overview refreshes at most once per second. Charts use connected first/min/max/last sample reduction in chronological order, preserving isolated spikes and flat sections. Reduction density follows actual chart width and device pixel ratio (capped at 4096 buckets); SVG coordinates remain fixed. Null values and capture gaps split continuous segments even inside one bucket. Do not replace this with disconnected vertical min/max strokes: those make constant and slowly varying signals disappear or appear dotted. Small windows show original samples, break lines at missing channel values or recording gaps, and share an elapsed-real-time axis. Each channel learns its minimum and maximum across the entire loaded session, ignoring unavailable values. Zoom/pan retain this Y range; newly captured extremes expand it, and a new session resets it. Overview traces and cursor dots use the same learned range. Charts label elapsed time on the X axis and maximum/midpoint/minimum values on the Y axis, with subtle major/quarter grid lines. Units appear in chart headings. Labels sit outside the SVG plot so they retain readable font sizes and cursor coordinates exclude the label gutter. Narrow mobile views omit the middle X label to avoid overlap. Gold lines mark parameter changes; red spans mark intervals over 100 ms with no captured samples. Paused snapshots carry the pause flag. Lower wall-clock sampling rates (such as reduced time scale) can therefore appear as gaps; network loss alone is recovered from Unity and does not manufacture a recording gap.

Use Full session, Zoom in/out, the overview and the window-start slider to inspect history. Follow live returns to the newest 30 seconds; capture continues while inspecting an older range. Hovering any channel chart shows a synchronized cursor, original stored channel values with adaptive display precision and elapsed/simulation time at hundredth-second resolution and dynamics frame. Lookup uses binary search over full-resolution samples, without interpolating or reading the reduced trace. Null values stay unavailable; hovering inside a capture gap shows no sample. During live following the cursor tracks the pointer position as the time window advances. Click a chart to pin its selected time and values while scrolling through channels. Pinning stops automatic window following while capture continues; clicking another point moves the pin. Press and drag across a channel chart to move the pinned cursor continuously; pointer capture keeps dragging active outside the chart, clamps to the visible time range, and leaves the final sample pinned on release. Zoom in/out centers on the pinned sample, clamped to the session boundaries; without a pin, zoom centers on the current window. Unpin cursor or Escape restores hover mode. Panning outside the pinned time hides its chart markers but retains the readouts. Focus a chart and use left/right arrows to step through recorded samples; Enter or Space pins the cursor. The event list is paginated in groups of 20. Browser memory is temporary and clears on page reload. While the bridge is still enabled, a new connection reloads its full history. After stopping the bridge, use Open a saved session folder and choose the timestamped session folder (not the parent LiveLink folder) to inspect every final sample. The folder picker automatically reads its three recording files; nested folders and unrelated files are ignored. Files are parsed locally without upload.

At 10 channels/50 Hz, 30 minutes is 90,000 rows: 5,850,008 bytes on disk and 5,496,912 bytes of browser telemetry buffers. Unity also retains samples in memory to serve catch-up; its object overhead is higher. No fixed session-duration limit is imposed. Memory and disk usage grow with duration, so substantially larger channel counts/rates or multi-hour sessions should move catch-up serving to disk-backed chunks.

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

client-web includes the completed real recording `20261013-213020-03a500c7d9494eddbb33470cd7eb8ed1` as `src/assets/unity-live-link/sample/`. **Open sample session** loads its 6,861 samples (about 137 seconds) through the same validated recording importer, without a WebSocket connection or local folder picker. The sample is read-only, uses all chart/cursor tools, and works on machines serving the web app without Unity. Binary samples and events are copied unchanged; metadata replaces the original machine-specific recording path with “Bundled sample session”. Assets are fetched only on request; the existing production service worker caches them after first use. Production availability requires a separate web-app deployment.

Display precision is derived per channel from its learned full-session range: one decimal place for normal spans, two for spans around 0.1 units, three around 0.01, and so on. Constant channels use their value magnitude. Very small spans use scientific notation; negative values that round to zero display as zero. Live values, cursor values, Y ticks and range captions share this formatting. Precision follows newly learned extrema but is independent of zoom/pan. Raw samples, ranges, chart geometry and disk recordings are never rounded by display formatting.
