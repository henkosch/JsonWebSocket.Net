using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JsonWebSocket
{
    public class JsonSocket
    {
        protected HttpContext context;
        protected WebSocket socket;

        protected CancellationToken ct;

        public WebSocket Socket { get { return socket; } }

        public bool Connected { get { return socket.State == WebSocketState.Open; } }

        public JsonSocket(HttpContext context, WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            this.context = context;
            this.socket = socket;
            this.ct = ct;
        }

        public event Action<Exception> OnReceiveError;
        public event Action<ReceivedData> OnReceived;
        public event Action<object> OnReceivedTextJson;
        public event Action<string> OnReceivedTextString;
        public event Action<object> OnReceivedBinaryBson;
        public event Action<byte[]> OnReceivedBinaryData;
        public event Action OnReceivedClose;
        public event Action OnDisconnected;   
        
        public HttpContext Context { get { return context; } }
        public string ConnectionId { get { return context?.Connection.Id; } }
        public EndPoint RemoteEndPoint { get { return context != null ? new IPEndPoint(context.Connection.RemoteIpAddress, context.Connection.RemotePort) : null; } }

        public async Task StartReceiving(int bufferSize = 102400)
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[bufferSize]);

                while (Connected)
                {
                    var received = await Receive(buffer);

                    OnReceived?.Invoke(received);

                    switch (received.Type)
                    {
                        case ReceivedDataType.TextJson:
                            OnReceivedTextJson?.Invoke(received.TextJson);
                            break;
                        case ReceivedDataType.TextString:
                            OnReceivedTextString?.Invoke(received.TextString);
                            break;
                        case ReceivedDataType.BinaryBson:
                            OnReceivedBinaryBson?.Invoke(received.BinaryBson);
                            break;
                        case ReceivedDataType.BinaryData:
                            OnReceivedBinaryData?.Invoke(received.BinaryData);
                            break;
                        case ReceivedDataType.Close:
                            OnReceivedClose?.Invoke();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                OnReceiveError?.Invoke(e);
            }
            finally
            {
                OnDisconnected?.Invoke();
            }
        }

        public async Task<ReceivedData> Receive(ArraySegment<byte> buffer)
        {
            using (var stream = new MemoryStream())
            {
                var messageType = await ReceiveUntilEnd(buffer, stream);
                switch (messageType)
                {
                    case WebSocketMessageType.Text:
                        try
                        {
                            return new ReceivedData(ReceivedDataType.TextJson, ParseJson(stream));
                        }
                        catch (Exception)
                        {
                            return new ReceivedData(ReceivedDataType.TextString, Encoding.UTF8.GetString(stream.ToArray()));
                        }
                    case WebSocketMessageType.Binary:
                        try
                        {
                            return new ReceivedData(ReceivedDataType.BinaryBson, ParseBson(stream));
                        }
                        catch (Exception)
                        {
                            return new ReceivedData(ReceivedDataType.BinaryData, stream.ToArray());
                        }
                    default:
                        return null;
                }
            }
        }

        async Task<WebSocketMessageType> ReceiveUntilEnd(ArraySegment<byte> buffer, Stream stream)
        {
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(buffer, ct);
                stream.Write(buffer.Array, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);
            stream.Seek(0, SeekOrigin.Begin);
            return result.MessageType;
        }

        object ParseBson(Stream stream)
        {
            using (var reader = new BsonDataReader(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize(reader);
            }
        }

        object ParseJson(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize(reader);
            }
        }

        void WriteJsonToStream(Stream stream, object data)
        {
            using (var streamWriter = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, data);
            }
        }

        void WriteBsonToStream(Stream stream, object data)
        {
            using (var writer = new BsonDataWriter(stream))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, data);
            }
        }

        public async Task SendTextJson(object data)
        {
            using (var stream = new MemoryStream())
            {
                WriteJsonToStream(stream, data);
                await socket.SendAsync(new ArraySegment<byte>(stream.ToArray()), WebSocketMessageType.Text, true, ct);
            }
        }

        public async Task SendTextString(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, ct);
        }

        public async Task SendBinaryBson(object data)
        {
            using (var stream = new MemoryStream())
            {
                WriteBsonToStream(stream, data);
                await socket.SendAsync(new ArraySegment<byte>(stream.ToArray()), WebSocketMessageType.Binary, true, ct);
            }
        }

        public async Task SendBinaryData(byte[] data)
        {
            await socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, ct);
        }

        public async Task SendClose(WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure, string message = null)
        {
            await socket.CloseAsync(status, message, ct);
        }
    }
}
