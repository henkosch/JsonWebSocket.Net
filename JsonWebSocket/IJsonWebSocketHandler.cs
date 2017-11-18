using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsonWebSocket
{
    public interface IJsonWebSocketHandler
    {
        Task Start(WebSocket socket, CancellationToken ct);
    }
}
