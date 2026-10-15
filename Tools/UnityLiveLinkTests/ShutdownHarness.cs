using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Perrinn424.LiveLink;

class ShutdownHarness
{
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static object Field(object value, string name) { return value.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(value); }
    static void Wait(Func<bool> condition)
    {
        var until = Stopwatch.StartNew();
        while (!condition()) { Check(until.ElapsedMilliseconds < 2000, "connection setup timed out"); Thread.Sleep(1); }
    }
    static TcpClient Connect(bool complete)
    {
        var client = new TcpClient(); client.Connect("127.0.0.1", 42481);
        client.ReceiveTimeout = 2000;
        string headers = complete ? "GET /live HTTP/1.1\r\nHost: localhost\r\nOrigin: http://localhost:4200\r\nUpgrade: websocket\r\nConnection: Upgrade\r\nSec-WebSocket-Version: 13\r\nSec-WebSocket-Key: AAECAwQFBgcICQoLDA0ODw==\r\n\r\n" : "GET /live HTTP/1.1\r\n";
        byte[] bytes = Encoding.ASCII.GetBytes(headers); client.GetStream().Write(bytes, 0, bytes.Length);
        if (complete)
        {
            var response = new StringBuilder();
            while (!response.ToString().EndsWith("\r\n\r\n")) { int b = client.GetStream().ReadByte(); Check(b >= 0, "handshake closed early"); response.Append((char)b); }
            Check(response.ToString().StartsWith("HTTP/1.1 101"), "WebSocket handshake");
        }
        return client;
    }
    static void Stop(LocalWebSocketServer server)
    {
        var watch = Stopwatch.StartNew(); server.Dispose(); server.Dispose();
        Check(watch.ElapsedMilliseconds < 2000, "shutdown must interrupt I/O rather than wait for handshake/keepalive timers");
        Check(((Task)Field(server, "accepting")).IsCompleted, "Dispose returned with the accept loop alive");
        foreach (Task task in (List<Task>)Field(server, "connections")) Check(task.IsCompleted, "Dispose returned with a connection worker alive");
        Check(server.ClientId == 0, "active connection retained after shutdown");
    }
    static void Main()
    {
        for (int run = 0; run < 20; run++)
        {
            Stop(new LocalWebSocketServer(42481, new[] { "http://localhost:4200" }));
            var server = new LocalWebSocketServer(42481, new[] { "http://localhost:4200" });
            using (var client = Connect(false))
            {
                Wait(() => { lock (Field(server, "gate")) return ((HashSet<TcpClient>)Field(server, "clients")).Count > 0; });
                Stop(server);
                Check(client.GetStream().ReadByte() == -1, "partial handshake was not closed");
            }
            server = new LocalWebSocketServer(42481, new[] { "http://localhost:4200" });
            using (var client = Connect(true))
            {
                Wait(() => server.ClientId != 0);
                var receipts = new List<Task<bool>>();
                for (int i = 0; i < 40; i++) receipts.Add(server.ReplyWithReceipt(server.ClientId, new string('x', 8192)));
                Stop(server);
                foreach (var receipt in receipts) Check(receipt.IsCompleted, "shutdown left a reply receipt pending");
            }
        }
        Console.WriteLine("PASS: 60 shutdown/rebind cycles, idle accept, partial handshakes, active WebSockets, queued receipts, all worker tasks joined and idempotent disposal");
    }
}
