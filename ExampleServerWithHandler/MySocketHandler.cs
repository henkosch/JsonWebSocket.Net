using JsonWebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleServerWithHandler
{
    public class MySocketHandler : ISocketHandler
    {
        public bool ClientCanConnect(HttpContext context)
        {
            return true;
        }

        public void ClientConnected(JsonSocket socket)
        {
            Console.WriteLine("[{0}] Connected.", socket.RemoteEndPoint);

            socket.OnReceivedBinaryBson += async (data) =>
            {
                Console.WriteLine("[{0}] Received binary Bson: {1}", socket.RemoteEndPoint, data);
                await socket.SendBinaryBson(data);
            };
        }

        public void ClientDisconnected(JsonSocket socket)
        {
            Console.WriteLine("[{0}] Disconnected.", socket.RemoteEndPoint);
        }

        public async Task Run(JsonSocket socket, CancellationToken ct)
        {
            for (var i = 0; i < 10; i++)
            {
                await socket.SendBinaryBson(new { counter = i });
                await Task.Delay(500);
            }
        }
    }
}
