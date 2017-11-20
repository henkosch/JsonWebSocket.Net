using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

                using (WebSocket socket = await context.WebSockets.AcceptWebSocketAsync())
                {
                    JsonSocket jsonSocket = new JsonSocket(context, socket, context.RequestAborted);

                    try
                    {
                        clientHandler(jsonSocket);
                    }
                    catch
                    {
                        socket.Dispose();
                    }

                    await jsonSocket.StartReceiving();
                }
            });
        }

        public static IApplicationBuilder UseJsonWebSocketHandler<T>(this IApplicationBuilder app) where T : ISocketHandler
        {
            return app.Use(async (context, next) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 426;
                    await context.Response.WriteAsync("WebSocket Request Required");
                    return;
                }

                var socketHandler = (T)context.RequestServices.GetService(typeof(T));

                if (socketHandler == null)
                {
                    var message = "Could not get {0} instance for handling the request. Please use IServiceCollection.AddScoped<{1}>() in Startup.ConfigureServices to enable it!";
                    throw new InvalidOperationException(string.Format(message, typeof(T).FullName, typeof(T).Name));
                }

                if (!socketHandler.ClientCanConnect(context))
                {
                    return;
                }

                using (WebSocket socket = await context.WebSockets.AcceptWebSocketAsync())
                {
                    JsonSocket jsonSocket = new JsonSocket(context, socket, context.RequestAborted);

                    socketHandler.ClientConnected(jsonSocket);

                    try
                    {
                        Func<Task> handleSocket = async () =>
                        {
                            try
                            {
                                await socketHandler.Run(jsonSocket, context.RequestAborted);
                            }
                            catch
                            {
                                socket.Dispose();
                            }
                        };

                        await Task.WhenAll(
                            handleSocket(),
                            jsonSocket.StartReceiving());
                    }
                    finally
                    {
                        socketHandler.ClientDisconnected(jsonSocket);
                    }
                }
            });
        }
    }
}
