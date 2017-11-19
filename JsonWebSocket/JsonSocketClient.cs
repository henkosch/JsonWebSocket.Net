using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsonWebSocket
{
    public class JsonSocketClient : JsonSocket, IDisposable
    {
        ClientWebSocket ClientSocket { get { return socket as ClientWebSocket; } }

        public JsonSocketClient(CancellationToken ct = default(CancellationToken)) : base(null, new ClientWebSocket(), ct)
        {
        }

        public async Task Connect(string url)
        {
            await ClientSocket.ConnectAsync(new Uri(url), ct);
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}
