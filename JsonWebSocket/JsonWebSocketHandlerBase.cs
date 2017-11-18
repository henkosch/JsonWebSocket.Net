using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsonWebSocket
{
    public class JsonWebSocketHandlerBase : IJsonWebSocketHandler
    {
        JsonSocket jsonSocket;

        public int ReceiveBufferSize { get; set; } = 102400;
        
        public async Task Start(WebSocket socket, CancellationToken ct)
        {
            jsonSocket = new JsonSocket(socket, ct);

            var buffer = new ArraySegment<byte>(new byte[ReceiveBufferSize]);

            while (jsonSocket.Connected)
            {
                var received = await jsonSocket.Receive(buffer);
                await Received(received);
                switch (received.Type)
                {
                    case ReceivedDataType.TextJson:
                        await ReceivedTextJson(received.TextJson);
                        break;
                    case ReceivedDataType.TextString:
                        await ReceivedTextString(received.TextString);
                        break;
                    case ReceivedDataType.BinaryBson:
                        await ReceivedBinaryBson(received.BinaryBson);
                        break;
                    case ReceivedDataType.BinaryData:
                        await ReceivedBinaryData(received.BinaryData);
                        break;
                    case ReceivedDataType.Close:
                        await ReceivedClose();
                        break;
                }
            }
        }

        protected async Task SendTextJson(object data)
        {
            await jsonSocket.SendTextJson(data);
        }

        protected async Task SendTextString(string data)
        {
            await jsonSocket.SendTextString(data);
        }

        protected async Task SendBinaryBson(object data)
        {
            await jsonSocket.SendBinaryBson(data);
        }

        protected async Task SendBinaryData(byte[] data)
        {
            await jsonSocket.SendBinaryData(data);
        }

        protected async Task SendClose()
        {
            await jsonSocket.SendClose();
        }

        protected virtual Task Received(ReceivedData data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceivedTextJson(dynamic data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceivedTextString(string data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceivedBinaryBson(dynamic data)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ReceivedBinaryData(byte[] data)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task ReceivedClose()
        {
            await jsonSocket.SendClose();
        }
    }
}
