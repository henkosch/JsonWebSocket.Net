# JsonWebSocket.Net
Simple .NET Standard library for sending json and bson messages through WebSocket. 

As it is built on the .NET Standard platform it can be used to easily build communication channels between Legacy .NET Framework applications and .NET Core plaform apps or Universal Windows Platform apps. You can also use it to build WebSocket server for web browser based or other clients.

# NuGet Package
https://www.nuget.org/packages/Henko.JsonWebSocket/

# WebSocket server example usage
You can build simple WebSocket servers with a few lines of code both on the .NET Framework platform and .NET Core platform using Asp.Net Core.

```csharp
using JsonWebSocket;

public static IWebHost BuildWebHost(string[] args) =>
  WebHost.CreateDefaultBuilder(args)
    .UseWebSockets()
    .Map("/ws", ws => ws.UseJsonWebSockets(socket =>
    {
      Console.WriteLine("[{0}] Client connected.", socket.RemoteEndPoint);
      socket.OnReceivedBinaryBson += async (data) =>
      {
        Console.WriteLine("[{0}] Received binary Bson: {1}", socket.RemoteEndPoint, data);
        await socket.SendBinaryBson(data);
      };
      socket.OnDisconnected += () =>
      {
        Console.WriteLine("[{0}] Client disconnected.", socket.RemoteEndPoint);
      };
    }))
    .UseUrls("http://0.0.0.0:5000/")
    .Build();
```

# WebSocket client example usage
Connect to any WebSocket server from a .Net application easily.
```csharp
using JsonWebSocket;

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
```

# JsonSocket

This class represents the WebSocket connection in both directions. You can use this to send or receive text, json or binary bson data and to get information about the connection.

```csharp
bool Connected { get; }
HttpContext Context { get; }
WebSocket Socket { get; }
string ConnectionId { get; }
EndPoint RemoteEndPoint { get; }

Task StartReceiving(int bufferSize = 102400);

Task SendTextJson(object data);
Task SendTextString(string data);
Task SendBinaryBson(object data);
Task SendBinaryData(byte[] data);
Task SendClose();

event Action<ReceivedData> OnReceived;
event Action<object> OnReceivedTextJson;
event Action<string> OnReceivedTextString;
event Action<object> OnReceivedBinaryBson;
event Action<byte[]> OnReceivedBinaryData;
event Action OnReceivedClose;
event Action<Exception> OnReceiveError;
event Action OnDisconnected;
```
