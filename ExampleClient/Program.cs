using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using JsonWebSocket;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Connect("ws://localhost:5000/ws").Wait();
        }

        static async Task Connect(string url)
        {
            using (var socket = new ClientWebSocket())
            {
                await socket.ConnectAsync(new Uri(url), CancellationToken.None);
                Console.WriteLine("Connected.");

                var jsonSocket = new JsonSocket(socket);

                await Task.WhenAll(Send(jsonSocket), Receive(jsonSocket));

                Console.WriteLine("Disconnected.");
            }
        }

        static async Task Send(JsonSocket jsonSocket)
        {
            while (jsonSocket.Connected)
            {
                dynamic dataObject = new { name = "Name", time = DateTime.Now };
                await jsonSocket.SendBinaryBson(dataObject);
                await Task.Delay(1000);
            }
        }

        static async Task Receive(JsonSocket jsonSocket)
        {
            while (jsonSocket.Connected)
            {
                var buffer = new ArraySegment<byte>(new byte[8192]);
                var data = await jsonSocket.Receive(buffer);
                Console.WriteLine("[Receive] Received: {0} {1}", data.Type, data.Data);
            }
        }
    }
}
