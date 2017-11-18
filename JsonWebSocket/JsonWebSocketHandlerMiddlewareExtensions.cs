using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;

namespace JsonWebSocket
{
    public static class JsonWebSocketHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseJsonWebSockets<T>(this IApplicationBuilder app, string route = "/ws") where T : IJsonWebSocketHandler, new()
        {
            return app.Map(route, AcceptWebSocketConnection<T>);
        }

        static void AcceptWebSocketConnection<T>(IApplicationBuilder app) where T : IJsonWebSocketHandler, new()
        {
            app.Use(async (context, next) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    return;
                }

                WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();

                var handler = new T();

                await handler.Start(socket, context.RequestAborted);
            });
        }
    }
}
