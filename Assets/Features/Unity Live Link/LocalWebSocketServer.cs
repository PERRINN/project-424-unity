using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Perrinn424.LiveLink
{
    // Network workers only handle copied messages. No Unity API is used here.
    public sealed class LocalWebSocketServer : IDisposable
    {
        public struct Incoming { public int client; public string json; }
        readonly TcpListener listener;
        readonly HashSet<string> origins;
        readonly CancellationTokenSource stop = new CancellationTokenSource();
        readonly ConcurrentQueue<Incoming> incoming = new ConcurrentQueue<Incoming>();
        readonly object gate = new object();
        readonly Task accepting;
        readonly List<Task> connections = new List<Task>();
        readonly HashSet<TcpClient> clients = new HashSet<TcpClient>();
        int disposed;
        Connection current;
        int nextId;
        int handshakes;
        public string Error { get; private set; }
        public int ClientId { get { lock (gate) return current == null ? 0 : current.id; } }

        sealed class Outgoing
        {
            public byte[] bytes;
            public TaskCompletionSource<bool> receipt;
        }
        sealed class Connection : IDisposable
        {
            public int id;
            public TcpClient tcp;
            public WebSocket socket;
            public readonly ConcurrentQueue<Outgoing> replies = new ConcurrentQueue<Outgoing>();
            public byte[] latest;
            public readonly SemaphoreSlim signal = new SemaphoreSlim(0, 1);
            public void Wake() { try { signal.Release(); } catch (SemaphoreFullException) { } }
            public void Dispose()
            {
                socket?.Abort(); tcp.Close(); Wake();
                Outgoing reply;
                while (replies.TryDequeue(out reply)) reply.receipt?.TrySetResult(false);
            }
        }

        public LocalWebSocketServer(int port, IEnumerable<string> allowedOrigins)
        {
            origins = new HashSet<string>(allowedOrigins, StringComparer.Ordinal);
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start(4);
            accepting = AcceptLoop();
        }

        public bool TryRead(out Incoming message) { return incoming.TryDequeue(out message); }
        public void Reply(int id, string json) { EnqueueReply(id, json, null); }
        public Task<bool> ReplyWithReceipt(int id, string json)
        {
            var receipt = new TaskCompletionSource<bool>();
            EnqueueReply(id, json, receipt);
            return receipt.Task;
        }
        void EnqueueReply(int id, string json, TaskCompletionSource<bool> receipt)
        {
            lock (gate)
            {
                if (current == null || current.id != id) { receipt?.TrySetResult(false); return; }
                if (current.replies.Count >= 32) { current.Dispose(); receipt?.TrySetResult(false); return; }
                current.replies.Enqueue(new Outgoing { bytes = Encoding.UTF8.GetBytes(json), receipt = receipt });
                current.Wake();
            }
        }
        public void Publish(int id, string json)
        {
            lock (gate)
            {
                if (current == null || current.id != id) return;
                // A slow client retains only one unsent telemetry snapshot.
                Interlocked.Exchange(ref current.latest, Encoding.UTF8.GetBytes(json));
                current.Wake();
            }
        }
        public void Disconnect(int id)
        {
            lock (gate) { if (current != null && current.id == id) current.Dispose(); }
        }

        async Task AcceptLoop()
        {
            while (!stop.IsCancellationRequested)
            {
                TcpClient tcp = null;
                try
                {
                    tcp = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    tcp.NoDelay = true;
                    lock (gate)
                    {
                        if (current != null || stop.IsCancellationRequested || handshakes >= 4) tcp.Close();
                        else
                        {
                            handshakes++;
                            clients.Add(tcp);
                            connections.RemoveAll(task => task.IsCompleted);
                            connections.Add(Serve(tcp));
                        }
                        tcp = null;
                    }
                }
                catch (Exception ex)
                {
                    tcp?.Close();
                    if (!stop.IsCancellationRequested) Error = ex.Message;
                }
            }
        }

        async Task Serve(TcpClient tcp)
        {
            Connection connection = null;
            try
            {
                var stream = tcp.GetStream();
                string key;
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
                {
                    timeout.CancelAfter(5000);
                    using (timeout.Token.Register(() => tcp.Close()))
                        key = await Handshake(stream, timeout.Token).ConfigureAwait(false);
                }
                if (key == null) return;
                lock (gate)
                {
                    if (current != null || stop.IsCancellationRequested) return;
                    connection = new Connection { id = ++nextId, tcp = tcp };
                    current = connection;
                }
                string accept;
                using (var sha = SHA1.Create())
                    accept = Convert.ToBase64String(sha.ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                byte[] response = Encoding.ASCII.GetBytes("HTTP/1.1 101 Switching Protocols\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Accept: " + accept + "\r\n\r\n");
                await stream.WriteAsync(response, 0, response.Length, stop.Token).ConfigureAwait(false);
                connection.socket = WebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(10));
                var read = ReadLoop(connection);
                var write = WriteLoop(connection);
                await Task.WhenAny(read, write).ConfigureAwait(false);
                connection.Dispose();
                // Observe both workers; aborting the socket ends the other operation.
                await Task.WhenAll(read, write).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!stop.IsCancellationRequested && !(ex is OperationCanceledException)) Error = ex.Message;
            }
            finally
            {
                lock (gate) { handshakes--; clients.Remove(tcp); if (current == connection) current = null; }
                connection?.Dispose();
                // Dispose after both socket loops finish, including the WebSocket keepalive timer.
                connection?.socket?.Dispose();
                connection?.signal.Dispose();
                tcp.Close();
            }
        }

        async Task<string> Handshake(NetworkStream stream, CancellationToken token)
        {
            var bytes = new List<byte>(512);
            var one = new byte[1];
            while (bytes.Count < 4096)
            {
                if (await stream.ReadAsync(one, 0, 1, token).ConfigureAwait(false) == 0) return null;
                bytes.Add(one[0]);
                int n = bytes.Count;
                if (n >= 4 && bytes[n - 4] == 13 && bytes[n - 3] == 10 && bytes[n - 2] == 13 && bytes[n - 1] == 10) break;
            }
            var lines = Encoding.ASCII.GetString(bytes.ToArray()).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var line in lines)
            {
                int colon = line.IndexOf(':');
                if (colon > 0) headers[line.Substring(0, colon)] = line.Substring(colon + 1).Trim();
            }
            string origin, upgrade, version, key, connection;
            if (bytes.Count >= 4096 || lines[0] != "GET /live HTTP/1.1" ||
                !headers.TryGetValue("Origin", out origin) || !origins.Contains(origin) ||
                !headers.TryGetValue("Upgrade", out upgrade) || !upgrade.Equals("websocket", StringComparison.OrdinalIgnoreCase) ||
                !headers.TryGetValue("Connection", out connection) || connection.IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) < 0 ||
                !headers.TryGetValue("Sec-WebSocket-Version", out version) || version != "13" ||
                !headers.TryGetValue("Sec-WebSocket-Key", out key)) return null;
            try { if (Convert.FromBase64String(key).Length != 16) return null; }
            catch (FormatException) { return null; }
            return key;
        }

        async Task ReadLoop(Connection connection)
        {
            var buffer = new byte[4096];
            while (!stop.IsCancellationRequested && connection.socket.State == WebSocketState.Open)
            {
                int size = 0;
                WebSocketReceiveResult result;
                do
                {
                    result = await connection.socket.ReceiveAsync(new ArraySegment<byte>(buffer, size, buffer.Length - size), stop.Token).ConfigureAwait(false);
                    if (result.MessageType != WebSocketMessageType.Text) return;
                    size += result.Count;
                    if (size >= buffer.Length) return;
                } while (!result.EndOfMessage);
                if (incoming.Count >= 64) return;
                incoming.Enqueue(new Incoming { client = connection.id, json = new UTF8Encoding(false, true).GetString(buffer, 0, size) });
            }
        }

        async Task WriteLoop(Connection connection)
        {
            while (!stop.IsCancellationRequested && connection.socket.State == WebSocketState.Open)
            {
                await connection.signal.WaitAsync(stop.Token).ConfigureAwait(false);
                Outgoing reply;
                while (connection.replies.TryDequeue(out reply))
                {
                    try { await Send(connection, reply.bytes).ConfigureAwait(false); reply.receipt?.TrySetResult(true); }
                    catch { reply.receipt?.TrySetResult(false); throw; }
                }
                byte[] bytes = Interlocked.Exchange(ref connection.latest, null);
                if (bytes != null) await Send(connection, bytes).ConfigureAwait(false);
            }
        }
        async Task Send(Connection connection, byte[] bytes)
        {
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
            {
                timeout.CancelAfter(2000);
                await connection.socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, timeout.Token).ConfigureAwait(false);
            }
        }
        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            stop.Cancel();
            listener.Stop();
            lock (gate)
            {
                current?.Dispose();
                // Include connections still waiting for HTTP headers, not just the active WebSocket.
                foreach (var client in new List<TcpClient>(clients)) client.Close();
            }
            // ConfigureAwait(false) keeps every network continuation independent of Unity's main thread.
            // Join the accept loop first so no connection can be added after taking this snapshot.
            accepting.GetAwaiter().GetResult();
            Task[] pending;
            lock (gate) pending = connections.ToArray();
            Task.WhenAll(pending).GetAwaiter().GetResult();
            stop.Dispose();
        }
    }
}
