const assert = require('node:assert/strict');
const { spawn } = require('node:child_process');
const { once } = require('node:events');
const path = require('node:path');
const Ws = require(path.resolve(process.env.CLIENT_WEB || '../client-web', 'node_modules/ws'));
const mono = '/Applications/Unity/Hub/Editor/2021.3.45f2/Unity.app/Contents/MonoBleedingEdge/bin/mono';
const server = spawn(mono, ['/tmp/perrinn-live-link/TransportHarness.exe']);
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
const bounded = promise => Promise.race([promise, delay(5000).then(() => { throw new Error('Test timeout'); })]);
function socket(origin = 'http://localhost:4200', endpoint = '/live') {
  const ws = new Ws('ws://127.0.0.1:42481' + endpoint, { origin });
  ws.on('error', () => {});
  return ws;
}
async function rejected(origin, endpoint) {
  const ws = socket(origin, endpoint);
  await bounded(once(ws, 'error'));
  assert.notEqual(ws.readyState, Ws.OPEN);
}
(async () => {
  await bounded(once(server.stdout, 'data'));
  await rejected('https://unrelated.example', '/live');
  await rejected('http://localhost:4200', '/wrong');
  let ws = socket(); await bounded(once(ws, 'open'));
  const text = JSON.stringify({ type: 'setParameter', requestId: 'test', value: 230000 });
  let reply = once(ws, 'message'); ws.send(text);
  assert.equal(String((await bounded(reply))[0]), text, 'text command round trip');
  reply = once(ws, 'message'); ws.send('fragmented ', { fin: false }); ws.send('message', { fin: true });
  assert.equal(String((await bounded(reply))[0]), 'fragmented message', 'fragmented messages');
  const burst = new Promise(resolve => ws.on('message', bytes => { if (String(bytes) === '{"sample":9999}') resolve(); }));
  ws.send('burst'); await bounded(burst);
  await rejected('http://localhost:4200', '/live'); // only one active client
  const closed = once(ws, 'close'); ws.send('x'.repeat(4096)); await bounded(closed);
  await delay(100);
  ws = socket(); await bounded(once(ws, 'open'));
  reply = once(ws, 'message'); ws.send('reconnected'); assert.equal(String((await bounded(reply))[0]), 'reconnected');
  const binaryClosed = once(ws, 'close'); ws.send(Buffer.from([1, 2, 3])); await bounded(binaryClosed);
  console.log('PASS: origin/path rejection, text round trip, fragmentation, bounded latest telemetry, single client, oversized/binary rejection, reconnect');
})().catch(error => { console.error(error); process.exitCode = 1; }).finally(() => server.kill());
