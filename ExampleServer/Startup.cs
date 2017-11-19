using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;
using System.Threading;
using System.IO;
using JsonWebSocket;

namespace WebSocketServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseWebSockets();
            app.Map("/ws", ws => ws.UseJsonWebSockets(ClientConnected));
        }

        public void ClientConnected(JsonSocket socket)
        {
            Console.WriteLine("[{0}] Client connected.", socket.RemoteEndPoint);

            socket.OnReceivedBinaryBson += async (data) =>
            {
                Console.WriteLine("[{0}] Received binary Bson: {1}", socket.RemoteEndPoint, data);
                await socket.SendBinaryBson(data);
            };

            socket.OnDisconnected += () =>
            {
                Console.WriteLine("[{0}] Client disconnected.", socket.RemoteEndPoint);
            };
        }
    }
}
