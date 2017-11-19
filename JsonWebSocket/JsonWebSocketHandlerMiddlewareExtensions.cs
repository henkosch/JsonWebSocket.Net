using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace JsonWebSocket
{
    public static class JsonWebSocketHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonWebSockets(this IApplicationBuilder app, Action<JsonSocket> clientHandler)
        {
            return app.Use(async (context, next) =>
            {
                if (!context.WebSockets.IsWebSocketRequest) return;

                WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();

                JsonSocket jsonSocket = new JsonSocket(context, socket, context.RequestAborted);

                clientHandler(jsonSocket);

                await jsonSocket.StartReceiving();
            });
        }
    }
}
