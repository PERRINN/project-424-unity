using System;
using System.Threading;
using Perrinn424.LiveLink;

class TransportHarness
{
    static void Main()
    {
        using (var server = new LocalWebSocketServer(42481, new[] { "http://localhost:4200" }))
        {
            Console.WriteLine("READY");
            var until = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < until)
            {
                LocalWebSocketServer.Incoming message;
                if (server.TryRead(out message))
                {
                    if (message.json == "burst")
                    {
                        for (int n = 0; n < 10000; n++) server.Publish(message.client, "{\"sample\":" + n + "}");
                    }
                    else if (message.json == "receipt")
                    {
                        if (server.ReplyWithReceipt(message.client, "delivered").GetAwaiter().GetResult()) server.Disconnect(message.client);
                    }
                    else server.Reply(message.client, message.json);
                }
                Thread.Sleep(1);
            }
        }
    }
}
