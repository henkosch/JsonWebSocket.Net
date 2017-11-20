using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using JsonWebSocket;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Connect("ws://localhost:5000/ws").Wait();
        }

        static async Task Connect(string url)
        {
            using (var socket = new JsonSocketClient())
            {
                socket.OnReceived += (data) =>
                {
                    Console.WriteLine("Received: {0} {1}", data.Type, data.Data);
                };

                await socket.Connect(url);

                Console.WriteLine("Connected to {0}.", url);

                await Task.WhenAll(
                    socket.StartReceiving(),
                    StartSending(socket)
                );

                Console.WriteLine("Disconnected.");
            }
        }

        static async Task StartSending(JsonSocket socket)
        {
            while (socket.Connected)
            {
                dynamic data = new { name = "Sample", time = DateTime.Now };
                await socket.SendBinaryBson(data);
                await Task.Delay(1000);
            }
        }        
    }
}
