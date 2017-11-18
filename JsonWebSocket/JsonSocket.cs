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
    public class JsonSocket
    {
        WebSocket socket;
        CancellationToken ct;

        public WebSocket Socket { get { return socket; } }

        public bool Connected { get { return socket.State == WebSocketState.Open; } }

        public JsonSocket(WebSocket socket, CancellationToken ct = default(CancellationToken))
        {
            this.socket = socket;
            this.ct = ct;
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

        public async Task SendClose()
        {
            await socket.SendAsync(new ArraySegment<byte>(), WebSocketMessageType.Close, true, ct);
        }
    }
}
