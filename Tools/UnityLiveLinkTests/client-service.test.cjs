// Actual Angular service and storage code, with a deterministic WebSocket peer.
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const client = path.resolve(process.env.CLIENT_WEB || '../client-web');
const ts = require(path.join(client, 'node_modules/typescript'));
const root = path.join(client, 'src/app/telemetry');
function load(file) {
  const source = fs.readFileSync(path.join(root, file + '.ts'), 'utf8');
  const compiled = ts.transpileModule(source, { compilerOptions: { module: ts.ModuleKind.CommonJS, target: ts.ScriptTarget.ES2022, experimentalDecorators: true } }).outputText;
  const moduleMock = { exports: {} };
  new Function('require', 'module', 'exports', compiled)(name => {
    if (name === '@angular/core') return { Injectable: () => type => type, Component: () => type => type };
    if (name.startsWith('./')) return load(name.slice(2));
    if (name === 'tslib') return require(path.join(client, 'node_modules/tslib'));
    throw new Error(name);
  }, moduleMock, moduleMock.exports);
  return moduleMock.exports;
}
const { TelemetryService } = load('telemetry.service');
class Socket {
  static OPEN = 1; static instances = [];
  readyState = 1; sent = [];
  constructor(url) { this.url=url; Socket.instances.push(this); }
  send(text) { this.sent.push(JSON.parse(text)); }
  close() { this.readyState = 3; this.onclose?.(); }
  receive(message) { this.onmessage({ data: JSON.stringify(message) }); }
}
global.WebSocket = Socket;
const service = new TelemetryService({ runOutsideAngular: fn => fn() });
const channels = Array.from({ length: 10 }, (_, i) => ({ id: 'channel' + i, unit: 'm', sourceHz: 50, available: true }));
const parameters = [{ id: 'frontSpringRate', label: 'Front spring rate', unit: 'N/m', value: 200000, min: 10000, max: 1000000, available: true }];
const welcome = { type: 'welcome', protocol: 2, session: 'session1', channels, parameters, recordedSamples: 600, recordingPath: '/test/session' };
const sample = sequence => ({ sequence, frame: sequence * 10, time: sequence / 50, simulationTime: sequence / 50, paused: false, values: Array(10).fill(sequence) });
const history = (first, last, total = 600) => ({ type: 'history', total, eventTotal: 0, events: [], samples: Array.from({ length: last - first + 1 }, (_, i) => sample(first + i)) });
(async () => {
try {
  service.connect('bad'); assert.equal(Socket.instances.length, 0);
  service.connect('a'.repeat(32));
  let socket = Socket.instances.at(-1); socket.onopen();
  assert.equal(socket.sent[0].protocol, 2); assert.equal(service.ready, false);
  socket.receive(welcome); assert.equal(service.ready, false);
  assert.equal(socket.sent.at(-1).after, 0);
  socket.receive({ type: 'pong' }); assert.equal(service.ready, false);
  socket.receive({ type: 'telemetry', sample: sample(600) });
  assert.equal(service.history.length, 0, 'latest-only packets must not create a hole in the session');
  socket.receive(history(1, 500));
  assert.equal(socket.sent.at(-1).after, 500);
  socket.receive(history(501, 600));
  assert.equal(service.history.length, 600); assert.equal(service.missingSamples, 0); assert.equal(service.ready, true);
  service.setParameter(service.parameters[0], 0); assert.equal(service.commands.frontSpringRate.error, true);
  service.setParameter(service.parameters[0], 230000);
  const command = socket.sent.at(-1); assert.equal(command.value, 230000); assert.equal(service.parameters[0].value, 200000);
  socket.receive({ type: 'result', requestId: command.requestId, ok: true, value: 230000, effectiveFromFrame: 601 });
  assert.equal(service.parameters[0].value, 230000); assert.equal(service.commands.frontSpringRate.pending, false);
  service.setParameter(service.parameters[0], 250000);
  socket.receive({ type: 'result', requestId: socket.sent.at(-1).requestId, ok: false, error: 'Rejected' });
  assert.equal(service.parameters[0].value, 230000);
  service.disconnect(); assert.equal(service.ready, false);
  service.connect('a'.repeat(32)); socket = Socket.instances.at(-1); socket.onopen(); socket.receive({ ...welcome, recordedSamples: 700 });
  assert.equal(service.history.length, 600, 'same session survives reconnect');
  assert.equal(socket.sent.at(-1).after, 600);
  assert.equal(socket.sent.filter(s => s.type === 'setParameter').length, 0);
  socket.receive(history(601, 700, 700)); assert.equal(service.missingSamples, 0);
  socket.receive(history(601, 700, 700)); assert.equal(service.history.length, 700, 'duplicates are idempotent');
  service.disconnect(); service.connect('a'.repeat(32)); socket = Socket.instances.at(-1); socket.onopen(); socket.receive({ ...welcome, session: '30-minute-test', recordedSamples: 90000 });
  assert.equal(service.history.length, 0, 'new session clears previous session');
  const start = performance.now();
  const batches = fs.readFileSync('/tmp/perrinn-live-link/30-minute-history.jsonl', 'utf8').trim().split('\n');
  let wireBytes = 0;
  for (const line of batches) { wireBytes += Buffer.byteLength(line); socket.onmessage({ data: line }); }
  const ingestionMs = performance.now() - start;
  assert.equal(service.history.length, 90000); assert.equal(service.missingSamples, 0); assert.equal(service.history.events.length, 6);
  assert.equal(service.history.gaps.length, 1); assert.equal(service.history.value(12344, 0), 1000);
  assert.ok(Number.isNaN(service.history.value(19999, 2)));
  assert.ok(service.history.bytes < 6 * 1048576);
  const full = service.history.trace(0, 0, service.history.end); assert.equal(full.max, 1000, 'overview must preserve isolated spikes');
  assert.ok(full.path.length < 20000, 'SVG complexity must depend on pixels, not session length');
  const zoom = service.history.trace(2, 399.9, 400.6);
  assert.ok((zoom.path.match(/M/g) || []).length >= 2, 'missing channel values must break the detailed trace');
  const gap = service.history.trace(0, 899.9, 902.1);
  assert.ok((gap.path.match(/M/g) || []).length >= 2, 'recording gaps must break the detailed trace');
  const { SessionHistory } = load('sessionHistory');
  const edge = new SessionHistory();
  for (let i = 1; i <= 4; i++) edge.append({ ...sample(i), values: Array(10).fill(i === 4 ? 1000 : 1) });
  assert.match(edge.trace(0, .02, .08, 1).path, /L300\.000,8\.000/, 'last-sample spike must be drawn in the final overview bucket');
  assert.equal(edge.trace(0, .02, .07).max, 1000, 'zoomed windows retain extremes from elsewhere in the session');
  assert.ok(!edge.trace(0, .02, .07).path.includes(',8.000'), 'out-of-window samples affect the scale but are not drawn');
  const flat = new SessionHistory(), ordered = new SessionHistory(), nullBreak = new SessionHistory(), timeBreak = new SessionHistory();
  for (let i = 1; i <= 3000; i++) {
    flat.append({ ...sample(i), values: Array(10).fill(0) });
    ordered.append({ ...sample(i), values: Array(10).fill(i === 1400 ? 1000 : i === 1500 ? -1000 : 0) });
    nullBreak.append({ ...sample(i), values: Array(10).fill(i === 1500 ? null : 0) });
    timeBreak.append({ ...sample(i), time: i / 50 + (i >= 1500 ? 2 : 0), values: Array(10).fill(0) });
  }
  const connected = flat.trace(0, flat.start, flat.end, 16).path;
  assert.equal((connected.match(/M/g) || []).length, 1, 'flat overview must be a single connected stroke');
  assert.ok((connected.match(/L/g) || []).length >= 16, 'flat sections must remain visible between buckets');
  const points = [...ordered.trace(0, ordered.start, ordered.end, 1).path.matchAll(/[ML]([\d.]+),([\d.]+)/g)].map(m => ({ x: +m[1], y: +m[2] }));
  assert.deepEqual(points.map(p => p.y), [30, 8, 52, 30], 'extrema must preserve their actual chronological order');
  assert.ok(points.every((p, i) => !i || p.x >= points[i - 1].x));
  assert.equal((nullBreak.trace(0, nullBreak.start, nullBreak.end, 1).path.match(/M/g) || []).length, 2, 'a null inside one bucket must split the coarse trace');
  assert.equal((timeBreak.trace(0, timeBreak.start, timeBreak.end, 1).path.match(/M/g) || []).length, 2, 'a capture gap inside one bucket must split the coarse trace');
  const dense = flat.trace(0, flat.start, flat.end, 1200).path;
  assert.ok(dense.length > connected.length, 'wide/high-DPI plots must receive additional detail');
  const { TelemetryComponent } = load('telemetry.component');
  const originalPerformance = global.performance, originalWindow = global.window;
  let clock = 2000, displayWidth = 300;
  global.performance = { now: () => clock }; global.window = { devicePixelRatio: 2 };
  try {
    const renderHistory = new SessionHistory();
    for (let i = 1; i <= 600; i++) renderHistory.append({ ...sample(i), values: Array(10).fill(0) });
    const model = { history: renderHistory, session: 'render-check', parameters: [], channels };
    const component = new TelemetryComponent(model, { runOutsideAngular: () => {} }, { nativeElement: { querySelector: () => ({ getBoundingClientRect: () => ({ width: displayWidth }) }) } });
    component.render(); const oldOverview = component.overview;
    assert.equal(component.overviewResolution, 600, 'resolution follows width multiplied by device pixel ratio');
    renderHistory.append({ ...sample(601), values: Array(10).fill(1000) });
    clock = 2100; component.render(); assert.notEqual(component.overview, oldOverview, 'new session extremes update the overview scale immediately');
    const peakOverview = component.overview;
    renderHistory.append({ ...sample(602), values: Array(10).fill(500) });
    clock = 2200; component.render(); assert.equal(component.overview, peakOverview, 'ordinary overview updates remain throttled');
    clock = 3200; component.render(); assert.notEqual(component.overview, peakOverview, 'overview must finish refreshing even if no further samples arrive');
    displayWidth = 900; component.render(); assert.equal(component.overviewResolution, 1800, 'resizing invalidates overview resolution');
    model.state = 'Recorded session'; component.render();
    assert.equal(component.full, true); assert.equal(component.follow, false);
    assert.equal(component.start, renderHistory.start); assert.equal(component.end, renderHistory.end);
    component.zoom(.5); component.render(); assert.equal(component.full, false, 'loaded sessions remain zoomable');
    const replacement = new SessionHistory(); replacement.append(sample(1)); replacement.append(sample(2));
    model.history = replacement; component.render();
    assert.equal(component.full, true, 'reopening the same session identity resets to full duration');
    assert.equal(component.start, replacement.start); assert.equal(component.end, replacement.end);

  } finally { global.performance = originalPerformance; global.window = originalWindow; }
  console.log('PASS: high-DPI sizing, resize invalidation and final overview refresh after catch-up');
  console.log('PASS: connected constant traces, ordered extrema, exact gap/null breaks at overview scale and adaptive resolution');
  assert.equal(flat.nearest(10.011).sequence, 501, 'cursor snaps to nearest full-resolution sample');
  assert.equal(flat.nearest(10.019).time, 10.02);
  assert.equal(flat.nearest(10.011, 10.02, 11).sequence, 501, 'cursor stays within the visible window');
  assert.equal(timeBreak.nearest(31), undefined, 'capture gaps must not fabricate a sample');
  assert.equal(nullBreak.nearest(30).values[0], null, 'null channel values remain unavailable');
  assert.equal(new SessionHistory().nearest(0), undefined);
  const cursorModel = { history: flat, session: 'cursor-test', parameters: [], channels };
  const cursorComponent = new TelemetryComponent(cursorModel, { runOutsideAngular: () => {} }, { nativeElement: { querySelector: () => null } });
  cursorComponent.start = 0; cursorComponent.end = 60;
  cursorComponent.hover({ clientX: 110, currentTarget: { getBoundingClientRect: () => ({ left: 10, width: 200 }) } });
  assert.equal(cursorComponent.cursor.time, 30); assert.equal(cursorComponent.cursorX, 150);
  assert.deepEqual(cursorComponent.cursor.values, Array(10).fill(0), 'all channel readouts come from one sample');
  cursorComponent.stepCursor(1); assert.equal(cursorComponent.cursor.time, 30.02);
  cursorComponent.clearCursor(true); assert.equal(cursorComponent.cursorTime, undefined);
  cursorComponent.live.history = timeBreak; cursorComponent.inspectTime(31);
  assert.equal(cursorComponent.cursor, undefined); assert.equal(cursorComponent.cursorTime, 31);
  cursorComponent.live.history = new SessionHistory(); cursorComponent.clearCursor(); cursorComponent.stepCursor(1);
  assert.equal(cursorComponent.cursor, undefined);
  const savedWindow = global.window;
  global.window = { devicePixelRatio: 1 };
  try {
    cursorComponent.live.history = flat; cursorComponent.session = 'cursor-test'; cursorComponent.follow = false;
    cursorComponent.start = 0; cursorComponent.end = 60;
    cursorComponent.hover({ clientX: 110, currentTarget: { getBoundingClientRect: () => ({ left: 10, width: 200 }) } });
    cursorComponent.start = 20; cursorComponent.render();
    assert.equal(cursorComponent.cursor.time, 40, 'hover cursor follows pointer fraction as the live window advances');
  } finally { global.window = savedWindow; }
  console.log('PASS: nearest raw-sample cursor, synchronized values, visible bounds, nulls/gaps, clearing and keyboard stepping');
  cursorComponent.live.history = flat; cursorComponent.start = 0; cursorComponent.end = 60; cursorComponent.follow = true;
  cursorComponent.hover({clientX:170,currentTarget:{getBoundingClientRect:()=>({left:10,width:200})}});
  assert.equal(cursorComponent.cursor.time,48);assert.equal(cursorComponent.follow,true,'hover does not stop live following');
  cursorComponent.clearCursor();assert.equal(cursorComponent.cursorTime,undefined,'leaving clears values without retaining a pin');
  cursorComponent.focusCursor();cursorComponent.stepCursor(1);assert.equal(cursorComponent.cursor.time,.04);cursorComponent.clearCursor();assert.equal(cursorComponent.cursor,undefined,'blur clears keyboard inspection');
  assert.equal('pinCursor' in cursorComponent,false);assert.equal('cursorPinned' in cursorComponent,false);
  console.log('PASS: transient hover and keyboard inspection, clearing on leave/blur and no pin state');
  const captured = new Set(); let pointerDefaultsPrevented=0, pointerBlurred=0;
  const dragChart = { blur:()=>pointerBlurred++, getBoundingClientRect: () => ({ left: 10, width: 200 }), setPointerCapture: id => captured.add(id), hasPointerCapture: id => captured.has(id), releasePointerCapture: id => captured.delete(id) };
  const dragEvent = (clientX, pointerId = 1) => ({ clientX, pointerId, button: 0, isPrimary: true, currentTarget: dragChart, preventDefault:()=>pointerDefaultsPrevented++ });
  cursorComponent.start = 0; cursorComponent.end = 60;
  cursorComponent.startDrag(dragEvent(60)); assert.equal(captured.has(1), true); assert.ok(pointerDefaultsPrevented && pointerBlurred);
  cursorComponent.hover(dragEvent(110)); assert.deepEqual(cursorComponent.selection.band, {x:75,width:75});
  assert.deepEqual(cursorComponent.selection.bandFor(dragChart),{x:75,width:75});assert.equal(cursorComponent.selection.bandFor({}),undefined,'selection appears only on its source chart');
  cursorComponent.hover(dragEvent(170, 2)); assert.deepEqual(cursorComponent.selection.band, {x:75,width:75}, 'another pointer cannot move the selection');
  cursorComponent.clearCursor();assert.equal(cursorComponent.selection.active,true,'leaving an unpinned chart must preserve captured selection');
  cursorComponent.hover(dragEvent(300)); assert.deepEqual(cursorComponent.selection.band, {x:75,width:225}, 'selection clamps at the chart edge');
  cursorComponent.finishDrag(dragEvent(170)); assert.equal(cursorComponent.start,15);assert.equal(cursorComponent.end,48);assert.equal(cursorComponent.span,33);assert.equal(cursorComponent.full,false);assert.equal(cursorComponent.follow,false);assert.equal(cursorComponent.selection.active,false);assert.equal(captured.size,0);
  cursorComponent.follow=true;cursorComponent.startDrag(dragEvent(110));cursorComponent.finishDrag(dragEvent(113));assert.equal(cursorComponent.follow,true,'clicking without dragging does not pause live following');
  assert.equal(cursorComponent.cursorTime,undefined,'small movement does not retain a cursor');assert.equal(cursorComponent.start,15);assert.equal(cursorComponent.end,48);
  cursorComponent.start = 0;cursorComponent.end = 60;
  cursorComponent.startDrag(dragEvent(170));cursorComponent.finishDrag(dragEvent(60));assert.equal(cursorComponent.start,15);assert.equal(cursorComponent.end,48,'reverse dragging selects the same window');
  cursorComponent.startDrag(dragEvent(60));cursorComponent.hover(dragEvent(170));cursorComponent.endDrag(dragEvent(170));assert.equal(captured.size,0);assert.equal(cursorComponent.start,15);assert.equal(cursorComponent.end,48,'cancellation leaves range unchanged');
  cursorComponent.startDrag(dragEvent(110));cursorComponent.clearCursor(true);assert.equal(captured.size,0);assert.equal(cursorComponent.selection.active,false);
  cursorComponent.resetChart();assert.equal(cursorComponent.full,true);assert.equal(cursorComponent.follow,false);assert.equal(cursorComponent.cursorTime,undefined);
  console.log('PASS: channel window zoom, reverse selection, click threshold, source-chart-only preview, edge clamping, pointer isolation, cancellation and full-session reset');
  cursorComponent.start=0;cursorComponent.end=60;cursorComponent.inspectTime(20);cursorComponent.zoom(.5);
  assert.equal(cursorComponent.start,15);assert.equal(cursorComponent.end,45,'zoom centers on the window regardless of hover');
  cursorComponent.zoom(2);assert.equal(cursorComponent.start,flat.start);assert.equal(cursorComponent.end,flat.end);
  cursorComponent.start=40;cursorComponent.end=60;cursorComponent.zoom(.5);assert.equal(cursorComponent.start,45);assert.equal(cursorComponent.end,55);
  console.log('PASS: zoom centers on the current window without cursor anchoring');
  const { rideHeightScatter, nearestScatterRow }=load('rideHeightScatter');
  const paired=new SessionHistory(2);
  [[10,20],[20,30],[null,40],[40,null],[50,60]].forEach((values,i)=>paired.append({sequence:i+1,frame:i,time:i,simulationTime:i,paused:false,values}));
  const dots=rideHeightScatter(paired,0,1,0,4);
  assert.deepEqual(dots[0].points,[{x:10,y:20,row:0},{x:20,y:30,row:1},{x:50,y:60,row:4}],'scatter pairs only finite values from the same raw row');
  assert.deepEqual(rideHeightScatter(paired,0,1,1,3)[0].points,[{x:20,y:30,row:1}],'time selection includes only selected paired samples');
  assert.deepEqual(rideHeightScatter(paired,-1,1,0,4),[],'missing channels cannot fabricate scatter dots');
  const scatterOverlay={tracks:[{rows:[0,1],distances:[0,10],color:'first'},{rows:[3,4],distances:[0,10],color:'second'}]};
  assert.deepEqual(rideHeightScatter(paired,0,1,10,10,scatterOverlay),[{color:'first',points:[{x:20,y:30,row:1}]},{color:'second',points:[{x:50,y:60,row:4}]}],'distance selection preserves same-row pairs and lap colours');
  assert.equal(nearestScatterRow(dots,20.2,30.1,2,4),1,'scatter hover resolves an original raw row');
  assert.equal(nearestScatterRow(dots,1000,1000,2,4),undefined,'empty scatter regions do not invent readings');
  assert.equal(nearestScatterRow([{color:'test',points:[{x:0,y:10,row:1},{x:10,y:0,row:2}]}],0,0,10,1),1,'nearest scatter dot uses pixel distance with unequal axis scales');
  cursorComponent.start=0;cursorComponent.end=60;cursorComponent.follow=true;cursorComponent.inspectScatter(flat.nearest(30));
  assert.equal(cursorComponent.cursor.time,30);assert.equal(cursorComponent.follow,true,'scatter inspection stays transient during live following');
  cursorComponent.inspectScatter();assert.equal(cursorComponent.cursorTime,undefined,'leaving scatter clears the shared cursor');
  console.log('PASS: scatter raw-row hit testing, empty space, pixel distance, synchronized cursor and clearing');
  console.log('PASS: original paired scatter dots, null exclusion, missing channels, time/distance windows and lap colours');
  const { telemetryPlots }=load('telemetryPlots');
  const errorChannels=['AutopilotDistanceError','LapTrigger','Speed'].map(id=>({id,unit:'test',sourceHz:50,available:true}));
  const errors=new SessionHistory(3);
  const appendError=(error,lap,speed)=>{const i=errors.length+1;errors.append({sequence:i,frame:i,time:i*.02,simulationTime:i*.02,paused:false,values:[error,lap,speed]});};
  appendError(-10000,0,300);appendError(20000,0,10);
  const startup=telemetryPlots(errors,errorChannels,0,1,300)[0];
  assert.equal(startup.axis.min,-50);assert.equal(startup.axis.max,50);assert.deepEqual(startup.ticks.map(t=>t.value),[50,25,0,-25,-50]);
  appendError(-2,1,20);appendError(3,1,30);appendError(null,1,40);appendError(500,2,50);
  const errorPlot=telemetryPlots(errors,errorChannels,0,1,300)[0];
  assert.deepEqual(errorPlot.axis,startup.axis,'startup, lap crossings and later outliers cannot rescale trajectory error');
  assert.equal(errors.range(0).min,-10000);assert.equal(errors.range(0).max,20000,'original outliers remain recorded');assert.ok(errorPlot.above && errorPlot.below);
  assert.deepEqual(telemetryPlots(errors,errorChannels,.08,.1,128)[0].axis,startup.axis,'zoom keeps fixed error bounds');
  assert.deepEqual(telemetryPlots(errors,errorChannels,0,100,128,{trace:()=>[]})[0].axis,startup.axis,'distance view uses identical fixed bounds');
  assert.equal(telemetryPlots(errors,errorChannels,0,1,300)[2].axis.max,300,'other channel scales remain unchanged');
  const missingTriggerChannels=errorChannels.map(c=>c.id==='LapTrigger'?{...c,id:'Other'}:c);
  assert.deepEqual(telemetryPlots(errors,missingTriggerChannels,0,1,300)[0].axis,startup.axis,'fixed bounds require no lap metadata');
  const emptyErrors=new SessionHistory(3);assert.deepEqual(telemetryPlots(emptyErrors,errorChannels,0,1,300)[0].axis,startup.axis,'new and empty sessions use the fixed range');
  console.log('PASS: fixed +/-50 mm trajectory error axis across startup, later laps, time/distance zoom, missing metadata and empty sessions; raw values preserved');
  const learned = new SessionHistory();
  learned.append({ ...sample(1), values: Array.from({length:10}, (_,c) => -2 * (c + 1)) });
  learned.append({ ...sample(2), values: Array.from({length:10}, (_,c) => 4 * (c + 1)) });
  assert.deepEqual(learned.range(0), {min:-2,max:4}); assert.deepEqual(learned.range(1), {min:-4,max:8});
  const narrowed = learned.trace(0, .04, .04);
  assert.equal(narrowed.min, -2); assert.equal(narrowed.max, 4); assert.match(narrowed.path, /,8\.000/);
  learned.append({ ...sample(3), values: Array(10).fill(null) }); assert.deepEqual(learned.range(0), {min:-2,max:4});
  const unavailableWindow = learned.trace(0, .06, .06); assert.equal(unavailableWindow.path, ''); assert.equal(unavailableWindow.min,-2); assert.equal(unavailableWindow.max,4);
  learned.append({ ...sample(4), values: Array(10).fill(-10) }); assert.deepEqual(learned.range(0), {min:-10,max:4});
  learned.append({ ...sample(5), values: Array(10).fill(20) }); assert.deepEqual(learned.range(0), {min:-10,max:20});
  assert.equal(new SessionHistory().hasRange(0), false, 'new sessions reset learned ranges');
  const onlyNulls = new SessionHistory(); onlyNulls.append({...sample(1),values:Array(10).fill(null)}); assert.equal(onlyNulls.hasRange(0), false);
  console.log('PASS: full-session Y ranges, zoom stability, independent channels, null-only windows, live expansion and new-session reset');
  const { timeAxis } = load('timeAxis');
  assert.deepEqual(timeAxis(0,136.89,1000).map(t=>t.value), [0,50,100]);
  assert.deepEqual(timeAxis(107,137,500).map(t=>t.value), [110,120,130]);
  assert.deepEqual(timeAxis(108,138,500).map(t=>t.value), [110,120,130], 'panning retains zero-anchored round ticks');
  assert.ok(timeAxis(0,1800,220).length < timeAxis(0,1800,1000).length, 'narrow charts reduce tick density');
  assert.ok(timeAxis(0,30,500).every(t=>/^\d+ s$/.test(t.label)), 'whole-second grids have no decimal padding');
  assert.ok(timeAxis(.101,.301,500).some(t=>t.label.includes('.')), 'subsecond zoom adds decimal precision');
  for (const [start,end,width] of [[0,1800,220],[107,137,500],[.101,.301,500],[1799.8,1800,220]]) {
    const ticks=timeAxis(start,end,width);
    assert.equal(new Set(ticks.map(t=>t.label)).size,ticks.length);
    for (const tick of ticks) { assert.ok(tick.value>=start && tick.value<=end); assert.ok(Math.abs(tick.position-(tick.value-start)/(end-start)*100)<1e-7); }
  }
  assert.deepEqual(timeAxis(0,0),[]);
  console.log('PASS: round time intervals, zero-anchored panning, adaptive precision and width-dependent density');
  const axesWindow = global.window; global.window = {devicePixelRatio:1};
  try {
    cursorComponent.live.history = learned; cursorComponent.session = 'cursor-test'; cursorComponent.full = cursorComponent.follow = false;
    cursorComponent.start = .02; cursorComponent.end = .10; cursorComponent.rendered = ''; cursorComponent.render();
    assert.deepEqual(cursorComponent.timeTicks.map(t=>+t.position.toFixed(2)), [0,25,50,75,100]);
    assert.deepEqual(cursorComponent.timeTicks.map(t=>+t.value.toFixed(2)), [.02,.04,.06,.08,.10]);
    assert.deepEqual(cursorComponent.traces[0].ticks, [{value:20,y:8},{value:10,y:19},{value:0,y:30},{value:-10,y:41},{value:-20,y:52}]);
    const originalTicks = cursorComponent.traces[0].ticks;
    cursorComponent.start = .06; cursorComponent.end = .10; cursorComponent.render();
    assert.deepEqual(cursorComponent.traces[0].ticks, originalTicks, 'Y tick labels retain the session range during zoom');
    assert.deepEqual(cursorComponent.timeTicks.map(t=>+t.value.toFixed(2)), [.06,.07,.08,.09,.10]);
    assert.equal(cursorComponent.traces[0].axis.min, -20, 'signed session range is centered at zero');
  } finally { global.window = axesWindow; }
  console.log('PASS: elapsed time ticks, session-fixed Y labels, trace/grid alignment and constant-channel labels');
  const { formatTelemetryValue, precisionForRange, formatAxisValue } = load('sessionHistory');
  for (const step of [100, 25, 1]) assert.equal(formatAxisValue(0, step), '0');
  for (const step of [2.5, .5, .1]) assert.equal(formatAxisValue(0, step), '0.0');
  assert.equal(formatAxisValue(100, 25), '100');
  assert.equal(formatAxisValue(.25, .25), '0.25');
  assert.equal(formatAxisValue(.05, .05), '0.05');
  assert.equal(formatAxisValue(.075, .025), '0.075');
  assert.equal(formatAxisValue(.0025, .0025), '0.0025');
  assert.equal(formatAxisValue(-.00001, .1), '0.0');
  assert.equal(formatAxisValue(2.5e-12, 2.5e-12), '2.5E-12');
  assert.equal(precisionForRange(0, 358.17), 1); assert.equal(formatTelemetryValue(242.895, 0, 358.17), '242.9');
  assert.equal(precisionForRange(0, .8), 2); assert.equal(formatTelemetryValue(.4567, 0, .8), '0.46');
  assert.equal(precisionForRange(0, .08), 3); assert.equal(formatTelemetryValue(.04567, 0, .08), '0.046');
  assert.equal(formatTelemetryValue(.00567, 0, .008), '0.0057');
  assert.equal(formatTelemetryValue(-.02, -500, 800), '0.0', 'rounded zero must not display a minus sign');
  assert.equal(formatTelemetryValue(null, 0, 10), '—'); assert.equal(formatTelemetryValue(NaN, 0, 10), '—');
  assert.equal(formatTelemetryValue(.01, .01, .01), '0.010', 'constant small channels use magnitude');
  assert.match(formatTelemetryValue(1.234e-12, 0, 2e-12), /^1\.2E-12$/, 'very small ranges retain meaningful scientific notation');
  const adapting = new SessionHistory();
  adapting.append({...sample(1),values:Array(10).fill(0)}); adapting.append({...sample(2),values:Array(10).fill(.08)});
  assert.equal(adapting.displayPrecision(0), 3); const beforeRaw=adapting.value(1,0);
  const formatted=adapting.formatValue(0,beforeRaw); assert.equal(formatted,'0.080'); assert.equal(adapting.value(1,0),beforeRaw,'formatting must never round stored samples');
  adapting.trace(0,.02,.02); assert.equal(adapting.displayPrecision(0),3,'zoom does not affect display precision');
  adapting.append({...sample(3),values:Array(10).fill(.8)}); assert.equal(adapting.displayPrecision(0),2);
  adapting.append({...sample(4),values:Array(10).fill(8)}); assert.equal(adapting.displayPrecision(0),1);
  console.log('PASS: adaptive learned precision, one-DP normal values, small/constant ranges, scientific notation, negative zero and unchanged raw samples');
  for(const [low,high,value] of [[-.587,4.688,2],[-21,190,40],[-24,142,60]]) {
    const axis=load('verticalAxis').verticalAxis(low,high), raw=new SessionHistory(1);
    raw.append({...sample(1),values:[value]});const rendered=raw.trace(0,.02,.02,300,axis);
    assert.equal(rendered.axis,axis,'resolved axes must be reused rather than rounded again');
    const y=Number(rendered.path.match(/M[-\d.]+,([-\d.]+)/)[1]);assert.ok(Math.abs(y-load('verticalAxis').axisY(value,axis))<.001);
  }
  console.log('PASS: already-resolved axes retain exact bounds and sample coordinates when signed-range rounding would change them');
  const { fixedChartRange } = load('chartChannels');
  const balances = new SessionHistory(4), balanceIds=['Speed','PowerBalanceDelivered','WheelTorqueBalance','AeroBalance'];
  const balanceRows=[[50,-20,101,50],[51,500,0,null],[52,null,-300,100],[53,50,50,120]];
  balanceRows.forEach((values,i)=>balances.append({...sample(i+1),values}));
  const rawBalances=balanceRows.map((_,i)=>balances.sampleAt(i).values);
  for(const id of balanceIds.slice(1)) assert.deepEqual(fixedChartRange(id),{min:0,max:100});
  assert.equal(fixedChartRange('Throttle'),undefined,'ordinary channels retain learned scales');
  for(const resolution of [1,300]) {
    const clipped=balances.trace(1,balances.start,balances.end,resolution,fixedChartRange('PowerBalanceDelivered'));
    assert.deepEqual(clipped.axis.ticks.map(t=>t.value),[100,75,50,25,0]);
    assert.equal(clipped.min,-20);assert.equal(clipped.max,500,'learned extrema are preserved separately from display bounds');
    assert.ok(clipped.above.includes('V8'));assert.ok(clipped.below.includes('V56'));
    assert.ok([...clipped.path.matchAll(/[,](\d+\.\d+)/g)].every(m=>+m[1]>=8 && +m[1]<=52));
    assert.equal((clipped.path.match(/M/g)||[]).length,2,'nulls still break bounded traces');
  }
  const exactBounds=balances.trace(2,.04,.04,300,fixedChartRange('WheelTorqueBalance'));
  assert.equal(exactBounds.above,'');assert.equal(exactBounds.below,'','zero is on the scale, not an outlier');
  const denseOutliers=new SessionHistory(1);
  for(let i=1;i<=1000;i++)denseOutliers.append({...sample(i),values:[i%2?-100:200]});
  const reducedOutliers=denseOutliers.trace(0,0,20,2,{min:0,max:100});
  assert.equal((reducedOutliers.above.match(/M/g)||[]).length,2);assert.equal((reducedOutliers.below.match(/M/g)||[]).length,2,'edge marker count follows screen resolution, not sample count');
  assert.deepEqual(balanceRows.map((_,i)=>balances.sampleAt(i).values),rawBalances,'display clipping never changes recorded samples');
  const priorBalanceWindow=global.window;global.window={devicePixelRatio:1};
  try {
    const balanceComponent=new TelemetryComponent({history:balances,session:'balance-test',state:'Recorded session',parameters:[],channels:balanceIds.map(id=>({id}))},{runOutsideAngular:()=>{}},{nativeElement:{querySelector:()=>null}});
    balanceComponent.render();balanceComponent.inspectTime(.04);
    assert.equal(balanceComponent.cursorY(1),8);assert.equal(balanceComponent.cursorOverflow(1),true);assert.equal(balances.formatValue(1,balanceComponent.cursor.values[1]),'500.0');
    balanceComponent.inspectTime(.02);assert.equal(balanceComponent.cursorY(1),52);assert.equal(balanceComponent.cursor.values[1],-20);
    balanceComponent.zoom(.5);balanceComponent.render();
    assert.ok(balanceComponent.traces.slice(1).every(t=>t.axis.min===0 && t.axis.max===100),'balance axes remain fixed through zoom');
    assert.ok(balanceComponent.traces[0].axis.min>0,'other channel axes still follow their session ranges');
  } finally {global.window=priorBalanceWindow;}
  console.log('PASS: fixed balance bounds, both edge markers before decimation, null breaks, bounded marker density, zoom stability, original extrema and raw cursor values');
  const { verticalAxis, axisY } = load('verticalAxis');
  assert.deepEqual(verticalAxis(0,358.17).ticks.map(t=>t.value), [400,300,200,100,0]);
  assert.deepEqual(verticalAxis(0,100).ticks.map(t=>t.value), [100,75,50,25,0]);
  assert.deepEqual(verticalAxis(-169,105).ticks.map(t=>t.value), [200,100,0,-100,-200]);
  assert.deepEqual(verticalAxis(-500,800).ticks.map(t=>t.value), [1000,500,0,-500,-1000]);
  assert.deepEqual(verticalAxis(-16,140).ticks.map(t=>t.value), [150,100,50,0,-50]);
  assert.equal(axisY(0,verticalAxis(-169,105)),30);
  const noise = verticalAxis(-.0008,358.17);
  assert.deepEqual(noise.ticks.map(t=>t.value), [400,300,200,100,0]); assert.ok(noise.min<=-.0008, 'tiny sign noise remains covered without a whole negative grid interval');
  assert.deepEqual(verticalAxis(-.023,2754.87).ticks.map(t=>t.value), [3000,2000,1000,0]);
  for (const [low,high] of [[0,0],[50,50],[-.8,10.4],[.001,.008],[1000.001,1000.008],[-1e-8,3e-8],[-500,800],[1e12,1e12+1e8]]) {
    const axis=verticalAxis(low,high);
    assert.ok(axis.min <= low + Math.abs(low)*1e-10 && axis.max >= high - Math.abs(high)*1e-10, 'rounded axis encloses all session values');
    assert.ok(axis.max > axis.min && axis.ticks.length>=2 && axis.ticks.length<=5);
    for(let i=1;i<axis.ticks.length;i++) assert.ok(Math.abs((axis.ticks[i-1].value-axis.ticks[i].value)/axis.step-1)<1e-5, 'grid intervals are uniform');
    for(const tick of axis.ticks) assert.equal(tick.y,axisY(tick.value,axis));
  }
  const axisBefore=learned.trace(0,.02,.1).axis; assert.deepEqual(learned.trace(0,.06,.1).axis,axisBefore, 'rounded axes stay fixed across zoom');
  console.log('PASS: rounded grid intervals, centered bipolar signals, constant/tiny/offset ranges, complete range coverage and zoom stability');
  const costs = [];
  for (let pass = 0; pass < 40; pass++) {
    const t = performance.now();
    for (let c = 0; c < 10; c++) service.history.trace(c, 0, service.history.end, 1200);
    costs.push(performance.now() - t);
  }
  costs.sort((a, b) => a - b);
  console.log(`PASS: full 30-minute C# → JSON → client history, reconnect recovery, session reset, peaks, nulls, gaps and events. ${service.history.bytes} buffer bytes; ${wireBytes} wire bytes; ${ingestionMs.toFixed(1)} ms ingest; ten-chart overview (1200 buckets/channel) median ${costs[20].toFixed(1)} ms / p95 ${costs[38].toFixed(1)} ms.`);
  socket.receive({ type: 'telemetry', sample: { values: [1] } }); assert.equal(service.error, 'Unity sent an invalid message.');
  console.log('PASS: pairing, stale gating, range validation, confirmed/rejected commands and malformed messages');
  const directory = '/tmp/perrinn-live-link/30-minute-recording';
  const files = ['session.json', 'samples.bin', 'changes.jsonl'].map(name => ({ name, text: async () => fs.readFileSync(path.join(directory, name), 'utf8'), arrayBuffer: async () => { const b = fs.readFileSync(path.join(directory, name)); return b.buffer.slice(b.byteOffset, b.byteOffset + b.byteLength); } }));
  await service.openRecording(files);
  assert.equal(service.error, ''); assert.equal(service.state, 'Recorded session'); assert.equal(service.ready, false);
  assert.equal(service.history.length, 90000); assert.equal(service.history.events.length, 6); assert.equal(service.history.gaps.length, 1);
  assert.equal(service.parameters[0].value, 290000);
  await service.openRecording(files.slice(0, 2)); assert.match(service.error, /Choose one session folder/);
  const folderFiles = files.map(f => ({ ...f, webkitRelativePath: 'session-folder/' + f.name }));
  const nested = files.map(f => ({ ...f, webkitRelativePath: 'session-folder/other-session/' + f.name, text: async () => { throw new Error('Nested files must be ignored'); } }));
  await service.openRecording([...folderFiles, ...nested]);
  assert.equal(service.state, 'Recorded session'); assert.equal(service.error, ''); assert.equal(service.history.length, 90000);
  await service.openRecording(nested); assert.match(service.error, /Choose one session folder/, 'parent folders must not silently select a nested session');
  const incomplete = files.map(f => f.name !== 'session.json' ? f : { ...f, text: async () => (await f.text()).replace('"complete":true', '"complete":false') });
  await service.openRecording(incomplete); assert.match(service.error, /incomplete/);
  let finish;
  const pendingFile = files.map(f => f.name !== 'session.json' ? f : { ...f, text: () => new Promise(resolve => { finish = resolve; }) });
  const pendingImport = service.openRecording(pendingFile);
  service.connect('a'.repeat(32)); finish(await files[0].text()); await pendingImport;
  assert.equal(service.state, 'Connecting', 'cancelled import must not overwrite a new connection');
  const originalFetch = global.fetch, originalDocument = global.document;
  const sampleDirectory = path.join(client, 'src/assets/unity-live-link/sample');
  const initial = new TelemetryService({runOutsideAngular: fn => fn()});
  const initialComponent = new TelemetryComponent(initial, {runOutsideAngular: () => {}}, {nativeElement:{}});
  const sampleMetadata = JSON.parse(fs.readFileSync(path.join(sampleDirectory, 'session.json'), 'utf8'));
  assert.equal(sampleMetadata.channels.length,43); assert.deepEqual(sampleMetadata.channels.slice(10,13).map(c=>c.id),['PositionX','PositionY','PositionZ']);
  assert.equal(sampleMetadata.recordedSamples, 45395); assert.equal(sampleMetadata.complete, true);
  assert.equal(sampleMetadata.recordingPath, 'Bundled sample session');
  assert.ok(sampleMetadata.channels.every(c=>c.available), 'all new sample channels have active providers');
  assert.equal(fs.readFileSync(path.join(sampleDirectory, 'samples.bin')).length, 8 + 45395 * (25 + 4 * 43));
  const originalRecording = path.join(require('node:os').homedir(), 'Library/Application Support/Perrinn/Project 424/LiveLink', '20261015-141851-bb0beed849ba4d7797ca236cdf65d836');
  if (fs.existsSync(originalRecording)) assert.deepEqual(fs.readFileSync(path.join(sampleDirectory, 'samples.bin')), fs.readFileSync(path.join(originalRecording, 'samples.bin')), 'bundled telemetry must be an unchanged copy');
  const sampleResponse = url => { const u = new URL(url); assert.ok(u.pathname.startsWith('/subapp/assets/unity-live-link/sample/')); return new Response(fs.readFileSync(path.join(sampleDirectory, path.basename(u.pathname)))); };
  global.document = { baseURI: 'https://example.test/subapp/' };
  try {
    global.fetch = async url => sampleResponse(url);
    initialComponent.ngOnInit();
    while(initial.loadingSample) await new Promise(resolve => setImmediate(resolve));
    assert.equal(initial.sampleSession, true); assert.equal(initial.history.length,45395); assert.equal(initial.ready,false);
    initialComponent.ngOnDestroy();
    await service.openSample();
    assert.equal(service.sampleSession, true); assert.equal(service.loadingSample, false); assert.equal(service.history.length, 45395); assert.equal(service.ready, false);
    assert.equal(service.session, sampleMetadata.session); assert.equal(service.history.events.length, 0);
    assert.equal(load('chartChannels').chartIndices(service.channels).length,33);
    for (let c=0;c<41;c++) assert.ok(service.history.hasRange(c), 'each bundled channel must contain finite recorded values');
    const realMap = new (load('trackTrajectory').TrackTrajectory)();
    realMap.update(service.history, service.channels.map(c=>c.id));
    assert.ok(realMap.path.length>100, 'bundled real sample must display a trajectory');
    assert.ok(realMap.position(service.history.sampleAt(500)), 'bundled sample provides cursor positions');
    const bundledAnalysis=new (load('sessionAnalysis').SessionAnalysis)();
    bundledAnalysis.update(service.history,service.channels);
    assert.equal(bundledAnalysis.laps.length,3);
    assert.deepEqual(bundledAnalysis.laps.map(l=>l.complete),[true,true,true]);
    assert.deepEqual(bundledAnalysis.laps.map(l=>l.sectors.length),[5,5,5]);
    assert.ok(Math.abs(bundledAnalysis.laps[0].duration-299.4486389)<.001);
    assert.ok(Math.abs(bundledAnalysis.laps[1].duration-299.3357849)<.001);
    const realOverlay=new (load('lapOverlay').LapOverlay)();realOverlay.update(service.history,service.channels,bundledAnalysis.laps);
    assert.ok(realOverlay.max>20000 && realOverlay.max<22000,'distance domain is one lap, not cumulative session distance');
    realOverlay.inspect(1000);assert.ok(realOverlay.readings.filter(r=>r.sample).length>=2);
    assert.ok(new Set(realOverlay.readings.filter(r=>r.sample).map(r=>r.sample.time)).size>=2,'one distance resolves independent lap times');
    assert.equal(realOverlay.trace(0,load('verticalAxis').verticalAxis(0,400),0,realOverlay.max,300).filter(l=>l.path).length,3);
    const {verticalAxis:resolvedAxis,axisY:resolvedY}=load('verticalAxis');
    const firstTrack=realOverlay.tracks[0], at=firstTrack.distances.findIndex((d,i)=>i>100 && d>firstTrack.distances[i-1] && d<firstTrack.distances[i+1]);
    assert.ok(at>100);const exactDistance=firstTrack.distances[at], exactSample=service.history.sampleAt(firstTrack.rows[at]);
    for(const id of ['AeroSczFront','AeroSczRear','RideHeightFront','RideHeightRear']) {
      const c=service.channels.findIndex(channel=>channel.id===id), raw=service.history.range(c), axis=resolvedAxis(raw.min,raw.max);
      const curve=realOverlay.trace(c,axis,exactDistance,exactDistance,128)[0].path;
      const plotted=Number(curve.match(/M[-\d.]+,([-\d.]+)/)[1]);
      assert.ok(Math.abs(plotted-resolvedY(exactSample.values[c],axis))<.001,`${id}: trace uses the tick axis at the exact raw sample`);
      assert.ok(Math.abs(plotted-realOverlay.dotY(exactSample,c,axis))<.001,`${id}: cursor dot follows the trace`);
    }
    console.log('PASS: real recording front/rear downforce and ride-height traces exactly align with their axes and raw-sample cursor dots');



    global.fetch = async () => new Response('', {status:404});
    await service.openSample(); assert.equal(service.loadingSample, false); assert.match(service.error, /Cannot load the sample session/);
    const finishFetches = [];
    global.fetch = url => new Promise(resolve => finishFetches.push(() => resolve(sampleResponse(url))));
    const pendingSample = service.openSample(); service.connect('a'.repeat(32)); finishFetches.forEach(f => f()); await pendingSample;
    assert.equal(service.state, 'Connecting', 'cancelled sample load must not overwrite a new live connection');
  } finally { initial.ngOnDestroy(); global.fetch = originalFetch; global.document = originalDocument; }
  console.log('PASS: bundled real session, exact binary copy, base-path asset URLs, read-only sample, fetch errors and cancellation');
  console.log('PASS: completed binary recording import, metadata/count verification, missing/incomplete files and import cancellation');
  const trajectoryDirectory = '/tmp/perrinn-live-link/trajectory-recording';
  const trajectoryFiles = ['session.json','samples.bin','changes.jsonl'].map(name=>({name,text:async()=>fs.readFileSync(path.join(trajectoryDirectory,name),'utf8'),arrayBuffer:async()=>{const b=fs.readFileSync(path.join(trajectoryDirectory,name));return b.buffer.slice(b.byteOffset,b.byteOffset+b.byteLength);}}));
  const expanded = new TelemetryService({runOutsideAngular:fn=>fn()});
  await expanded.openRecording(trajectoryFiles);
  assert.equal(expanded.error,''); assert.equal(expanded.channels.length,13); assert.equal(expanded.history.channelCount,13); assert.equal(expanded.history.length,90000);
  assert.equal(expanded.history.sampleAt(1).values.length,13);
  const { TrackTrajectory } = load('trackTrajectory');
  const map = new TrackTrajectory(), mapStart=performance.now();
  map.update(expanded.history,expanded.channels.map(c=>c.id));
  assert.equal((map.path.match(/M/g)||[]).length,2,'missing positions break the map trajectory');
  assert.ok(map.path.length<90000*10,'map reduction bounds visual density');
  const raw=expanded.history.sampleAt(45000), dot=map.position(raw), selected=map.nearest(dot.x,dot.y,raw.time);
  assert.ok(Math.abs(selected.time-raw.time)<.1,'repeated laps prefer the cursor time at the same location');
  const mapMs=performance.now()-mapStart;
  map.update(new SessionHistory(),channels.map(c=>c.id)); assert.equal(map.path,''); assert.equal(map.position(raw),undefined,'legacy sessions do not invent a trajectory');
  const oldWindow=global.window; global.window={devicePixelRatio:1};
  try {
    const nav=new TelemetryComponent(expanded,{runOutsideAngular:()=>{}},{nativeElement:{querySelector:()=>null}});
    nav.render(); assert.equal(nav.full,true);
    assert.deepEqual(nav.chartGroups.flatMap(group=>group.indices),[0,11,1,2,3,6,4,5,7,8,9],'visible charts preserve original channel indices and hide world X/Z');
    assert.equal(nav.channelName('PositionY'),'Altitude'); assert.equal(nav.channelName('Speed'),'Speed');
    nav.span=300; nav.pan(600); nav.render();
    const capture=new Set(), overviewChart={blur:()=>{},getBoundingClientRect:()=>({left:10,top:0,width:300,height:120}),setPointerCapture:id=>capture.add(id),hasPointerCapture:id=>capture.has(id),releasePointerCapture:id=>capture.delete(id)};
    const pointer={button:0,isPrimary:true,pointerId:77,preventDefault:()=>{},currentTarget:overviewChart,clientX:10+(750-expanded.history.start)/(expanded.history.end-expanded.history.start)*300};
    nav.startNavigation(pointer); assert.ok(Math.abs(nav.start-600)<1e-8,'press inside selection preserves its grab offset');
    nav.moveNavigation({...pointer,clientX:pointer.clientX+50}); assert.ok(Math.abs(nav.start-(600+(expanded.history.end-expanded.history.start)/6))<1e-8); assert.equal(nav.end-nav.start,300);
    nav.moveNavigation({...pointer,pointerId:78,clientX:0}); assert.ok(nav.start>800,'other pointers cannot hijack a drag');
    nav.moveNavigation({...pointer,clientX:1000}); assert.equal(nav.end,1800,'outside drag clamps at session end'); nav.endNavigation(pointer); assert.equal(capture.size,0);
    const atTime=time=>({...pointer,clientX:10+(time-expanded.history.start)/(expanded.history.end-expanded.history.start)*300});
    nav.span=300;nav.pan(600);nav.startNavigation(atTime(600));assert.equal(nav.resizingNavigation,true);
    nav.moveNavigation(atTime(450));assert.equal(nav.start,450);assert.equal(nav.end,900,'left edge holds the right boundary');assert.equal(nav.span,450);
    nav.moveNavigation(atTime(1000));assert.ok(Math.abs(nav.end-nav.start-.2)<1e-8,'left edge cannot cross the minimum window');
    nav.moveNavigation(atTime(-500));assert.equal(nav.start,expanded.history.start,'left resize clamps at the session start');nav.endNavigation(pointer);
    nav.span=300;nav.pan(600);nav.startNavigation(atTime(900));nav.moveNavigation(atTime(1200));
    assert.equal(nav.start,600);assert.equal(nav.end,1200,'right edge holds the left boundary');assert.equal(nav.span,600);
    nav.moveNavigation(atTime(500));assert.ok(Math.abs(nav.end-nav.start-.2)<1e-8,'right edge cannot cross the minimum window');
    nav.moveNavigation(atTime(3000));assert.equal(nav.end,expanded.history.end,'right resize clamps at session end');nav.endNavigation(pointer);assert.equal(capture.size,0);
    assert.equal(nav.resizingNavigation,false);assert.equal(nav.follow,false);
    nav.span=300;nav.pan(600);nav.startNavigation(atTime(750));assert.equal(nav.resizingNavigation,false,'selection center still pans');nav.endNavigation(pointer);
    console.log('PASS: captured left/right overview resizing, fixed opposite boundary, minimum duration, session bounds, cleanup and pointer focus suppression');
    nav.span=300;nav.pan(600); nav.render();
    const target=expanded.history.sampleAt(5000), point=nav.track.position(target);
    nav.selectMap({currentTarget:{getBoundingClientRect:()=>({left:0,top:0,width:160,height:160})},clientX:point.x,clientY:point.y});
    assert.ok(nav.cursor.time>=nav.start && nav.cursor.time<=nav.end,'map selection brings its sample into the visible window');
    assert.ok(nav.mapDot); assert.equal(nav.follow,false); nav.ngOnDestroy();
  } finally {global.window=oldWindow;}
  expanded.connect('a'.repeat(32)); const expandedSocket=Socket.instances.at(-1); expandedSocket.onopen();
  expandedSocket.receive({...welcome,session:'expanded-stream',channels:JSON.parse(await trajectoryFiles[0].text()).channels});
  expandedSocket.receive({type:'history',total:1,eventTotal:0,events:[],samples:[{...sample(1),values:Array(13).fill(1)}]});
  expandedSocket.receive({type:'telemetry',sample:{...sample(1),values:Array(13).fill(2)}});
  assert.equal(expanded.error,''); assert.equal(expanded.history.channelCount,13); assert.equal(expanded.history.length,1); assert.equal(expanded.latest.values.length,13); expanded.disconnect();
  console.log(`PASS: 13-channel C# binary import and stream, 30-minute trajectory/gap projection and lap selection (${mapMs.toFixed(1)} ms), legacy placeholder, map cursor and captured overview dragging`);
  const unitySource=fs.readFileSync('Assets/Features/Unity Live Link/UnityLiveLink.cs','utf8');
  const streamNames=[...unitySource.match(/static readonly string\[\] Names = \{([\s\S]*?)\};/)[1].matchAll(/"([^"]+)"/g)].map(m=>m[1]);
  const presentation=load('chartChannels');
  assert.equal(streamNames.length,43); assert.equal(new Set(streamNames).size,43);
  assert.ok(presentation.chartChannels.every(([id])=>streamNames.includes(id)), 'every requested chart must be included in the Unity catalogue');
  const requested=streamNames.map(id=>({id,unit:'test',available:true,sourceHz:50}));
  const sorted=presentation.chartIndices(requested);
  assert.equal(sorted.length,33); assert.deepEqual(sorted.map(i=>requested[i].id),presentation.chartChannels.map(([id])=>id));
  assert.deepEqual(sorted.map(i=>requested[i].id).slice(-6,-3),['AeroFrontFlap','AeroDrsPosition','AeroScx']);
  assert.deepEqual(presentation.fixedChartRange('AeroDrsPosition'),{min:0,max:100});
  const grouped=presentation.chartGroups(requested);
  assert.deepEqual(grouped.map(g=>g.label),['Session & trajectory','Driver inputs','Vehicle dynamics','Chassis','Tire loads','Powertrain','Aerodynamics']);
  assert.deepEqual(grouped.flatMap(g=>g.indices),sorted,'grouping preserves every visible channel exactly once at its original index');
  const sparseGroups=presentation.chartGroups([{id:'Speed'},{id:'Brake'},{id:'PositionX'},{id:'CustomSensor'}]);
  assert.deepEqual(sparseGroups.map(g=>g.label),['Session & trajectory','Driver inputs','Other channels'],'empty groups disappear and unknown channels remain visible');
  assert.deepEqual(sparseGroups.flatMap(g=>g.indices),[0,1,3]);assert.deepEqual(presentation.chartGroups([]),[]);
  console.log('PASS: engineering groups, original channel indices, no duplicates, sparse catalogues, unknown channels and empty sessions');

  assert.equal(sorted[3],11,'altitude keeps its recorded world Y index in the session group');
  const fullService=new TelemetryService({runOutsideAngular:fn=>fn()});
  fullService.connect('a'.repeat(32)); const fullSocket=Socket.instances.at(-1); fullSocket.onopen();
  fullSocket.receive({...welcome,session:'full-catalogue',channels:requested});
  const fullRow={...sample(1),values:Array.from({length:43},(_,i)=>i===25?null:i+0.25)};
  fullSocket.receive({type:'history',total:1,eventTotal:0,events:[],samples:[fullRow]});
  assert.equal(fullService.error,''); assert.equal(fullService.history.channelCount,43); assert.deepEqual(fullService.history.sampleAt(0).values,fullRow.values);
  const header=Buffer.alloc(8+25+4*43); header.write('ULL2');header.writeInt32LE(43,4); header.writeInt32LE(1,8);header.writeInt32LE(10,12);header.writeDoubleLE(.02,16);header.writeDoubleLE(.02,24);
  fullRow.values.forEach((v,i)=>header.writeFloatLE(v??NaN,33+i*4));
  const fullMetadata={protocol:2,session:'full-file',complete:true,recordedSamples:1,recordedEvents:0,channels:requested,parameters:[]};
  await fullService.openRecording([{name:'session.json',text:async()=>JSON.stringify(fullMetadata)},{name:'samples.bin',arrayBuffer:async()=>header.buffer.slice(header.byteOffset,header.byteOffset+header.byteLength)},{name:'changes.jsonl',text:async()=>''}]);
  assert.equal(fullService.error,'');assert.equal(fullService.history.channelCount,43);assert.deepEqual(fullService.history.sampleAt(0).values,fullRow.values); fullService.disconnect();
  console.log('PASS: all 43 catalogue channels, 33 logically ordered charts, preserved indices, null balances, streaming and expanded binary import');

  const { SessionAnalysis, lapTime } = load('sessionAnalysis');
  const timing = new SessionHistory(streamNames.length), analysis = new SessionAnalysis();
  let data = Object.fromEntries(streamNames.map(id => [id, null]));
  const appendTiming = patch => {
    Object.assign(data, patch);
    const sequence = timing.length + 1;
    timing.append({sequence,frame:sequence,time:sequence*.02,simulationTime:sequence*.01,paused:false,values:streamNames.map(id=>data[id])});
  };
  appendTiming({LapDistance:null,LapTrigger:0,SectorTrigger:0,LapCompleteTrigger:0,Speed:10});
  analysis.update(timing,requested); assert.equal(analysis.laps.length,0);
  appendTiming({LapDistance:0,LapTrigger:1});
  appendTiming({LapDistance:10,SectorTrigger:1,SectorIndex:1,SectorTime:12.125,SectorValid:1});
  analysis.update(timing,requested); assert.equal(analysis.laps.length,1); assert.equal(analysis.laps[0].complete,false);
  appendTiming({LapDistance:30,SectorTrigger:2,SectorIndex:2,SectorTime:15.25,SectorValid:0});
  appendTiming({LapCompleteTrigger:1,LapTime:27.375,LapValid:0});
  appendTiming({LapDistance:0,LapTrigger:2});
  appendTiming({LapDistance:20});
  analysis.update(timing,requested);
  assert.equal(analysis.laps.length,2); assert.equal(analysis.laps[0].duration,27.375); assert.equal(analysis.laps[0].valid,false);
  assert.deepEqual(analysis.laps[0].sectors,[{index:1,time:12.125,valid:true},{index:2,time:15.25,valid:false}]);
  assert.equal(analysis.laps[1].complete,false); assert.equal(analysis.laps[0].end,.1); assert.equal(analysis.laps[1].start,.12);
  assert.equal(lapTime(59.9996),'1:00.000'); assert.equal(lapTime(), '—');
  analysis.update(timing,requested); assert.equal(analysis.laps[0].sectors.length,2,'incremental refresh does not duplicate triggers');
  const replayAnalysis=new SessionAnalysis(); replayAnalysis.update(timing,requested); assert.deepEqual(replayAnalysis.laps,analysis.laps,'history recovery gives the same timing as live ingestion');
  const oldWindow2=global.window; global.window={devicePixelRatio:1};
  try {
    const liveTiming={history:timing,channels:requested,parameters:[],session:'timing',state:'Recorded session',latest:timing.sampleAt(timing.length-1),disconnect:()=>{}};
    const view=new TelemetryComponent(liveTiming,{runOutsideAngular:()=>{}},{nativeElement:{querySelector:()=>null}});
    view.render(); assert.equal(view.distanceAxis,true);assert.equal(view.cursorTime,undefined,'opening distance mode does not retain a cursor'); view.toggleAxis(); view.render(); assert.equal(view.distanceAxis,false); view.toggleAxis(); view.render(); assert.equal(view.distanceAxis,true);assert.equal(view.cursorTime,undefined,'opening distance mode does not retain a cursor'); assert.ok(view.timeTicks.every(t=>t.label.endsWith(' m')));
    assert.equal(view.timeAtFraction(.4),12); view.inspectTime(10);assert.ok(Math.abs(view.cursorX-100)<1e-9);assert.equal(view.overlay.readings.length,2);
    view.hover({clientX:200,currentTarget:{getBoundingClientRect:()=>({left:0,width:300})}});assert.equal(view.cursorX,200);view.clearCursor();assert.equal(view.cursorTime,undefined);assert.equal(view.overlay.readings.length,0);
    view.selectLap(view.analysis.laps[0]);view.render();assert.equal(view.overlay.activeLap,1);assert.equal(view.start,0);assert.equal(view.end,30);assert.equal(view.follow,false);
    view.followLive();view.render();assert.equal(view.follow,true);assert.equal(view.cursorTime,undefined);assert.equal(view.end,20);
    view.ngOnDestroy();
    const driving = new SessionHistory(streamNames.length);
    for (let i=0;i<=500;i++) driving.append({sequence:i+1,frame:i,time:.02+i*.02,simulationTime:.02+i*.02,paused:false,values:streamNames.map(id=>id==='LapDistance'?i*i*.001:id==='Speed'?i:id==='LapTrigger'?1:0)});
    const distanceView=new TelemetryComponent({...liveTiming,history:driving,session:'distance-drag'},{runOutsideAngular:()=>{}},{nativeElement:{querySelector:()=>null}});
    distanceView.render();distanceView.distanceWindow(50,50);distanceView.render();
    const captures=new Set(), chart={blur:()=>{},getBoundingClientRect:()=>({left:10,width:500}),setPointerCapture:id=>captures.add(id),hasPointerCapture:id=>captures.has(id),releasePointerCapture:id=>captures.delete(id)};
    const pointerAt=d=>({button:0,isPrimary:true,pointerId:9,preventDefault:()=>{},currentTarget:chart,clientX:10+d/250*500});
    distanceView.startNavigation(pointerAt(75));distanceView.moveNavigation(pointerAt(100));distanceView.endNavigation(pointerAt(100));
    assert.ok(Math.abs(distanceView.start-75)<.001);
    assert.ok(Math.abs(distanceView.end-125)<.001,'distance pan preserves metres rather than elapsed duration');
    distanceView.startNavigation(pointerAt(75));distanceView.moveNavigation(pointerAt(60));distanceView.endNavigation(pointerAt(60));
    assert.ok(Math.abs(distanceView.start-60)<.001);assert.ok(Math.abs(distanceView.end-125)<.001,'distance resize preserves opposite boundary');
    distanceView.clearCursor(true);distanceView.zoom(.5);assert.ok(Math.abs(distanceView.end-distanceView.start-32.5)<.001);
    distanceView.start=0;distanceView.end=250;
    distanceView.startDrag(pointerAt(50));distanceView.hover(pointerAt(125));distanceView.finishDrag(pointerAt(125));
    assert.equal(distanceView.start,50);assert.equal(distanceView.end,125,'channel selection uses distance coordinates');assert.equal(distanceView.span,75);
    const scatterSample=driving.sampleAt(300);distanceView.inspectScatter(scatterSample);
    assert.equal(distanceView.cursor.sequence,scatterSample.sequence);assert.equal(distanceView.overlay.readings.find(r=>r.lap===distanceView.overlay.activeLap).sample.sequence,scatterSample.sequence,'distance scatter cursor retains the exact hovered raw row');
    distanceView.inspectScatter();assert.equal(distanceView.overlay.readings.length,0,'scatter leave clears per-lap readouts');
    distanceView.resetChart();distanceView.render();assert.equal(distanceView.start,0);assert.equal(distanceView.end,250,'reset shows the complete distance range');
    distanceView.panOverview(1);assert.ok(Number.isFinite(distanceView.start));assert.equal(captures.size,0);distanceView.ngOnDestroy();
  } finally {global.window=oldWindow2;}
  const legacyAnalysis=new SessionAnalysis();legacyAnalysis.update(new SessionHistory(),channels);assert.deepEqual(legacyAnalysis.laps,[]);
  analysis.update(new SessionHistory(),channels);assert.deepEqual(analysis.laps,[]);
  console.log('PASS: lap/sector counters, official simulation durations, validity, incomplete laps, incremental/recovered history, distance resets/inversion/traces/cursor, lap selection, live resume and legacy sessions');

  const { recordingGapBands } = load('recordingGaps');
  const gapHistory=new SessionHistory(3);
  gapHistory.append({sequence:1,frame:1,time:0,simulationTime:0,paused:false,values:[0,100,null]});
  gapHistory.append({sequence:2,frame:2,time:1,simulationTime:1,paused:false,values:[0,100,null]});
  const gapAxis={min:0,max:100,step:25,ticks:[]};
  const gapPoints=channel=>recordingGapBands(gapHistory,channel,gapAxis,0,1,t=>t)[0].points.split(' ').map(p=>p.split(',').map(Number));
  const lowBand=gapPoints(0), highBand=gapPoints(1), emptyBand=gapPoints(2);
  assert.equal(Math.max(...lowBand.map(p=>p[1]))-Math.min(...lowBand.map(p=>p[1])),6,'gap band is narrow rather than chart-height');
  assert.equal((lowBand[0][1]+lowBand[3][1])/2,52);assert.equal((highBand[0][1]+highBand[3][1])/2,8,'each channel centres its band on its own trace');
  assert.equal((emptyBand[0][1]+emptyBand[3][1])/2,30,'missing endpoints use a quiet central marker');
  const clippedBand=recordingGapBands(gapHistory,0,gapAxis,.25,.75,t=>t*100)[0];
  assert.ok(clippedBand.points.startsWith('0,'));assert.ok(clippedBand.points.includes('300,'));
  assert.equal(recordingGapBands(gapHistory,0,gapAxis,2,3,t=>t).length,0);
  console.log('PASS: narrow trace-centred per-channel gap bands, unavailable values, clipped windows and distance coordinates');

  const stoppingService=new TelemetryService({runOutsideAngular:fn=>fn()});
  stoppingService.connect('a'.repeat(32));let stopSocket=Socket.instances.at(-1);stopSocket.onopen();
  stopSocket.receive({...welcome,recordedSamples:1,canStopSimulation:true});stopSocket.receive(history(1,1,1));
  stopSocket.receive({type:'telemetry',sample:sample(1)});
  assert.equal(stoppingService.canStopSimulation,true);stoppingService.stopSimulation();
  const stopCommand=stopSocket.sent.at(-1);assert.equal(stopCommand.type,'stopSimulation');assert.equal(stoppingService.stopping,true);assert.equal(stoppingService.ready,false);
  const sentCount=stopSocket.sent.length;stoppingService.stopSimulation();assert.equal(stopSocket.sent.length,sentCount,'duplicate stop clicks send only one command');
  stopSocket.receive({type:'simulationStopped',requestId:'wrong'});assert.equal(stoppingService.stopping,true);
  stopSocket.receive({type:'simulationStopped',requestId:stopCommand.requestId,recordingError:''});
  assert.equal(stoppingService.state,'Disconnected');assert.equal(stoppingService.stopMessage,'Unity simulation stopped.');assert.equal(stoppingService.stopping,false);assert.equal(stoppingService.history.length,1);assert.equal(stoppingService.reconnect,false);
  stoppingService.connect('a'.repeat(32));stopSocket=Socket.instances.at(-1);stopSocket.onopen();stopSocket.receive({...welcome,recordedSamples:0});assert.equal(stoppingService.canStopSimulation,false,'older bridges do not offer stop');
  stopSocket.receive({...welcome,recordedSamples:0,canStopSimulation:true});stoppingService.stopSimulation();stopSocket.close();
  assert.equal(stoppingService.state,'Disconnected');assert.ok(stoppingService.error.includes('before confirming'));assert.equal(stoppingService.reconnect,false);assert.equal(stoppingService.stopping,false);
  stoppingService.ngOnDestroy();
  console.log('PASS: advertised stop capability, one authenticated command, correlated acknowledgement, retained charts, disconnect without retry and unconfirmed-stop errors');

  {
    const { LapOverlay }=load('lapOverlay'), overlayHistory=new SessionHistory(2);
    for(let i=0;i<6;i++)overlayHistory.append({sequence:i+1,frame:i,time:i*.02,simulationTime:i*.02,paused:false,values:[i%3*10,i<3?10+i:20+i-3]});
    const laps=[{number:1,start:0,end:.04,complete:true,sectors:[]},{number:2,start:.06,end:.1,complete:false,sectors:[]}];
    const overlay=new LapOverlay();overlay.update(overlayHistory,[{id:'LapDistance'},{id:'Speed'}],laps);
    overlay.inspect(10);assert.deepEqual(overlay.readings.map(r=>r.sample.sequence),[2,5]);assert.deepEqual(overlay.readings.map(r=>r.sample.values[1]),[11,21]);
    const layers=overlay.trace(1,{min:0,max:30,step:10,ticks:[]},0,20,300);
    assert.equal(layers.length,2);assert.ok(layers.every(l=>l.path.startsWith('M0.000,')&&l.path.includes('L150.000,')&&l.path.includes('L300.000,')),'separate lap paths align equal distances without joining lap resets');
    overlay.activeLap=2;assert.equal(overlay.inspect(10).sequence,5);overlay.inspect(25);assert.ok(overlay.readings.every(r=>!r.sample),'out-of-range lap distances stay blank');
    const sparse=new SessionHistory(2);sparse.append({sequence:1,frame:1,time:0,simulationTime:0,paused:false,values:[0,1]});sparse.append({sequence:2,frame:2,time:1,simulationTime:1,paused:false,values:[10,null]});
    overlay.update(sparse,[{id:'LapDistance'},{id:'Speed'}],[{number:1,start:0,end:1,complete:false,sectors:[]}]);assert.equal(overlay.inspect(5),undefined,'cursor does not bridge capture gaps');
    assert.equal(overlay.inspect(10).values[1],null,'null channels remain unavailable');
    console.log('PASS: lap-distance overlays, separate equal-distance samples and values, active-lap identity, partial ranges, gaps and nulls');
  }

  {
    const {EditorSimulationControl}=load('editorSimulationControl');
    const nativeInterval=global.setInterval, nativeClearInterval=global.clearInterval;
    let tick;
    global.setInterval=fn=>(tick=fn,123456); global.clearInterval=()=>{};
    const detector=new EditorSimulationControl(()=>assert.fail('probe must never connect telemetry'),()=>assert.fail('probe must not show startup errors'));
    try {
      assert.equal(detector.available,false); detector.watch(); let probe=Socket.instances.at(-1);
      probe.onopen();assert.deepEqual(probe.sent,[{type:'editorProbe'}]);
      probe.receive({type:'editorAvailable'});assert.equal(detector.available,true);assert.equal(detector.checking,false);assert.equal(probe.readyState,3);
      tick();probe=Socket.instances.at(-1);probe.close();assert.equal(detector.available,false,'closing Unity disables startup');
      tick();probe=Socket.instances.at(-1);probe.receive({type:'unknown'});assert.equal(detector.available,false,'a different local service is not an Editor');
      tick();probe=Socket.instances.at(-1);probe.receive({type:'editorAvailable'});assert.equal(detector.available,true,'opening Unity enables startup again');
      detector.starting=true;const count=Socket.instances.length;tick();assert.equal(Socket.instances.length,count,'presence checks cannot compete with startup');
    } finally {detector.dispose();global.setInterval=nativeInterval;global.clearInterval=nativeClearInterval;}
    console.log('PASS: Editor presence detection, closing/reopening, unknown peers, startup isolation and cleanup');
  }
  {
    const {EditorSimulationControl}=load('editorSimulationControl'), originalStorage=global.localStorage, storage=new Map();
    global.localStorage={getItem:key=>storage.get(key)||null,setItem:(key,value)=>storage.set(key,value),removeItem:key=>storage.delete(key)};
    const connected=[], errors=[], editorToken='c'.repeat(32), runtimeToken='d'.repeat(32);
    const editor=new EditorSimulationControl(token=>connected.push(token),error=>errors.push(error));
    const before=Socket.instances.length;editor.start();assert.equal(Socket.instances.length,before);assert.ok(errors.at(-1).includes('Pair once'));
    try {
      editor.start(editorToken);let peer=Socket.instances.at(-1);assert.equal(peer.url,'ws://127.0.0.1:42482/live');peer.onopen();
      assert.deepEqual(peer.sent.at(-1),{type:'editorHello',token:editorToken});assert.equal(peer.sent.filter(m=>m.type==='startSimulation').length,0,'starting requires successful Editor pairing');
      peer.receive({type:'editorReady'});const operation=peer.sent.at(-1);assert.equal(operation.type,'startSimulation');assert.equal(editor.paired,true);assert.equal(storage.get('perrinn.telemetry.editorToken'),editorToken);
      editor.start(editorToken);assert.equal(Socket.instances.at(-1),peer,'duplicate clicks do not create a second start');
      const nativeTimeout=global.setTimeout;let retryEditor;
      global.setTimeout=(fn,ms,...args)=>ms===500?(retryEditor=fn,0):nativeTimeout(fn,ms,...args);
      try {peer.close();assert.equal(editor.starting,true);assert.ok(retryEditor);retryEditor();}finally{global.setTimeout=nativeTimeout;}
      peer=Socket.instances.at(-1);peer.onopen();peer.receive({type:'editorReady'});assert.equal(peer.sent.at(-1).requestId,operation.requestId,'domain reload retries the same operation id');
      peer.receive({type:'editorStatus',requestId:'other',phase:'running',autopilot:true,token:runtimeToken});assert.deepEqual(connected,[]);
      peer.receive({type:'editorStatus',requestId:operation.requestId,phase:'running',autopilot:true,token:runtimeToken});assert.deepEqual(connected,[runtimeToken]);assert.equal(editor.starting,false);assert.equal(peer.readyState,3);
      const remembered=new EditorSimulationControl(token=>connected.push(token),error=>errors.push(error));assert.equal(remembered.paired,true);remembered.start();peer=Socket.instances.at(-1);peer.onopen();assert.equal(peer.sent.at(-1).token,editorToken);
      peer.receive({type:'editorReady'});peer.receive({type:'editorStatus',requestId:peer.sent.at(-1).requestId,phase:'error',error:'Autopilot refused alignment'});assert.ok(errors.at(-1).includes('alignment'));assert.equal(remembered.starting,false);assert.equal(connected.length,1);
      assert.equal(remembered.paired,true,'Autopilot failure preserves authenticated pairing');
      remembered.available=true;assert.equal(remembered.needsPairing,false);
      remembered.available=false;remembered.checking=false;assert.equal(remembered.needsPairing,false,'unavailable Editor hides pairing and preserves the saved token');
      remembered.available=true;assert.equal(remembered.needsPairing,false,'reopening Unity can reuse pairing');
      remembered.start();peer=Socket.instances.at(-1);peer.onopen();peer.receive({type:'editorStatus',phase:'error',error:'Editor pairing failed'});assert.equal(remembered.paired,false,'rejected saved credentials are cleared');assert.equal(remembered.needsPairing,true);assert.equal(storage.size,0);
      remembered.available=false;assert.equal(remembered.needsPairing,false,'unpaired state stays hidden until Editor detection');remembered.available=true;assert.equal(remembered.needsPairing,true,'detected Editor reveals required pairing');
      remembered.start(editorToken);peer=Socket.instances.at(-1);peer.onopen();peer.receive({type:'editorReady'});remembered.cancel();assert.equal(remembered.paired,true,'replacement token can pair normally');
      remembered.start(editorToken);peer=Socket.instances.at(-1);peer.close();assert.ok(errors.at(-1).includes('Cannot reach Unity Editor'));assert.equal(remembered.starting,false);remembered.cancel();
    }finally{editor.cancel();global.localStorage=originalStorage;}
    console.log('PASS: Editor pairing, remembered device token, domain-reload reconnect with one operation id, confirmed Autopilot/runtime handoff, rejected startup and cancellation');
  }

  {
    const {emptyPlan,target,setupError,comparison,deltaTime}=load('sessionSetup');
    const plan=emptyPlan();assert.equal(plan.length,2);assert.notEqual(plan[0],plan[1]);
    const params=[...parameters,{id:'autopilotPositionOffset',label:'Offset',unit:'m',value:.45,min:-10,max:10,available:true}];
    const change={parameter:'frontSpringRate',mode:'gain',amount:1.05};
    assert.equal(target(change,params),210000);assert.equal(setupError(change,params),'');
    assert.equal(target({...change,mode:'offset',amount:-10000},params),190000);
    assert.match(setupError({...change,amount:10},params),/Target/);
    assert.match(setupError({...change,amount:NaN},params),/Target/);
    assert.match(setupError({...change,parameter:'unknown'},params),/unavailable/);
    const baseline={duration:300,valid:true,sectors:[{index:1,time:60,valid:true}]};
    const test={duration:299.5,valid:true,sectors:[{index:1,time:59.8,valid:true}]};
    assert.equal(comparison(test,baseline),-.5);assert.equal(comparison(test,baseline,1),-.2);
    assert.equal(deltaTime(comparison(test,baseline)),'−0.500');
    assert.equal(deltaTime(comparison({...test,duration:301},baseline)),'+1.000');
    assert.equal(comparison({...test,valid:false},baseline),undefined);
    assert.equal(comparison(test,{...baseline,sectors:[{index:1,time:60,valid:false}]},1),undefined);
    assert.equal(comparison(undefined,baseline),undefined);
    const auto=new TelemetryService({runOutsideAngular:fn=>fn()});
    try {
      auto.connect('a'.repeat(32));const peer=Socket.instances.at(-1);peer.onopen();
      peer.receive({...welcome,session:'auto-complete',recordedSamples:0,canStopSimulation:true,lapPlan:plan});
      peer.receive({type:'sessionComplete',total:3,eventTotal:0});
      assert.equal(peer.sent.some(m=>m.type==='stopSimulation'),false,'wait for all final telemetry before stopping');
      peer.receive(history(1,3,3));
      const stop=peer.sent.at(-1);assert.equal(stop.type,'stopSimulation');assert.equal(auto.history.length,3);
      peer.receive({type:'simulationStopped',requestId:stop.requestId});
      assert.equal(auto.state,'Disconnected');assert.match(auto.stopMessage,/3 \/ 3/);assert.equal(auto.history.length,3);
      auto.connect('a'.repeat(32));const next=Socket.instances.at(-1);next.onopen();
      next.receive({...welcome,session:'next-run',recordedSamples:0,canStopSimulation:true});
      next.receive(history(1,1,1));assert.equal(next.sent.some(m=>m.type==='stopSimulation'),false,'completion state must not leak into the next run');
    } finally {auto.ngOnDestroy();}
    console.log('PASS: three-lap plans, gain/offset targets, range guards, baseline deltas, invalid timing, automatic stop after final history recovery and new-run reset');
  }

  {
    const {SimulationPace,simulationDuration}=load('simulationPace');
    const pace=new SimulationPace();
    const point=(time,simulationTime,paused=false)=>({...sample(1),time,simulationTime,paused});
    pace.update(point(0,0));assert.equal(pace.ratio,undefined);
    pace.update(point(.2,.64));assert.equal(pace.ratio,undefined,'avoid unstable startup ratios');
    pace.update(point(1,3.2));assert.ok(Math.abs(pace.ratio-3.2)<1e-9);
    pace.update(point(5,16));assert.ok(Math.abs(pace.ratio-3.2)<1e-9,'skipped snapshots do not alter the measured speed');
    pace.update(point(6,18));pace.update(point(10,26));assert.equal(pace.ratio,2,'recent window reflects actual slowdown');
    pace.update(point(11,26,true));assert.equal(pace.ratio,0,'pause reports zero immediately');
    pace.update(point(0,0));assert.equal(pace.ratio,undefined,'new clock restarts measurement');
    pace.update(point(1,2));pace.update(point(1,2));assert.equal(pace.ratio,2,'duplicate timestamps cannot divide by zero');
    pace.update(point(2,0));assert.equal(pace.ratio,undefined,'simulation reset cannot report a negative speed');
    assert.equal(simulationDuration(65.9),'1:05');assert.equal(simulationDuration(3661),'1:01:01');
    const measured=new TelemetryService({runOutsideAngular:fn=>fn()});
    try {
      measured.connect('a'.repeat(32));const peer=Socket.instances.at(-1);peer.onopen();
      peer.receive({...welcome,session:'measured-run',recordedSamples:0});
      peer.receive({type:'telemetry',sample:point(10,32)});assert.equal(measured.simulationRatio,undefined);
      peer.receive({type:'telemetry',sample:point(11,35.2)});assert.ok(Math.abs(measured.simulationRatio-3.2)<1e-9);
      measured.lastTelemetry=performance.now()-3000;assert.equal(measured.simulationRatio,undefined,'stale telemetry must not display a current speed');
      peer.receive({type:'telemetry',sample:point(12,35.2,true)});assert.equal(measured.simulationRatio,0);
      peer.receive({...welcome,session:'next-measured-run',recordedSamples:0});assert.equal(measured.simulationRatio,undefined,'new connections do not reuse the previous speed');
    } finally {measured.ngOnDestroy();}
    console.log('PASS: measured simulation/wall-clock ratio, rolling slowdown, sparse/duplicate snapshots, pause, resets, stale telemetry and simulation clock formatting');
  }

} finally { service.ngOnDestroy(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
