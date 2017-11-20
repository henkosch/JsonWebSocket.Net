using JsonWebSocket;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JsonWebSocket
{
    public interface ISocketHandler
    {
        bool ClientCanConnect(HttpContext context);
        void ClientConnected(JsonSocket socket);
        Task Run(JsonSocket socket, CancellationToken ct);
        void ClientDisconnected(JsonSocket socket);
    }
}
