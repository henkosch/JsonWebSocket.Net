using Microsoft.AspNetCore.Builder;
using JsonWebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace WebSocketServer
{
    public class WebSocketHandler : JsonWebSocketHandlerBase
    {
        protected override async Task ReceivedTextJson(dynamic data)
        {
            Console.WriteLine("Received text json: {0}", data);
            await SendTextJson(data);
        }

        protected override async Task ReceivedTextString(string data)
        {
            Console.WriteLine("Received text string: {0}", data);
            await SendTextString(data);
        }

        protected override async Task ReceivedBinaryBson(dynamic data)
        {
            Console.WriteLine("Received binary bson: {0}", data);
            await SendBinaryBson(data);
        }

        protected override async Task ReceivedBinaryData(byte[] data)
        {
            Console.WriteLine("Received binary data: {0} bytes", data.Length);
            await SendBinaryData(data);
        }
    }
}
