using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Emporos.Middleware
{
  public class WebsocketServerMiddleware
  {

    private readonly RequestDelegate _next;
    private readonly WebSocketServerConnectionManager _manager;

    public WebsocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
    {
      _next = next;
      _manager = manager;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      if (context.WebSockets.IsWebSocketRequest)
      {
        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("WebSocekt connected");

        string ConnId = _manager.AddSocket(webSocket);

        await SendConnIDAsync(webSocket, ConnId);

        await ReceiveMessaget(webSocket, async (result, buffer, webSocket) =>
        {
          if (result.MessageType == WebSocketMessageType.Text)
          {
            Console.WriteLine("Received message");
            Console.WriteLine($"message: {Encoding.UTF8.GetString(buffer, 0, result.Count)}");
            await RouteJSONMessageAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
            return;
          }
          else if (result.MessageType == WebSocketMessageType.Close)
          {
            string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;
            Console.WriteLine("Received close message");
            _manager.GetAllSockets().TryRemove(id, out WebSocket webSocketTes);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            return;
          }
        });
      }
      else
      {
        Console.WriteLine("Hello from the 2rd request delegate");
        await _next(context);
      }
    }

    private async Task SendConnIDAsync(WebSocket socket, string connID)
    {
      var buffer = Encoding.UTF8.GetBytes($"ConnID: {connID}");
      await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    private async Task ReceiveMessaget(WebSocket socket, Action<WebSocketReceiveResult, byte[], WebSocket> handleMessage)
    {
      var buffer = new byte[1024 * 4];

      while (socket.State == WebSocketState.Open)
      {
        var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);

        handleMessage(result, buffer, socket);
      }
    }
    public async Task RouteJSONMessageAsync(string message)
    {
      var routeOb = JsonConvert.DeserializeObject<dynamic>(message);
      if (Guid.TryParse(routeOb.To.ToString(), out Guid guidOutput))
      {
        Console.WriteLine("Targeted");
        var sock = _manager.GetAllSockets().FirstOrDefault(s => s.Key == routeOb.To.ToString());
        if (sock.Value != null)
        {
          if (sock.Value.State == WebSocketState.Open)
          {
            await sock.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()),
            WebSocketMessageType.Text, true, CancellationToken.None);
          }
          else
          {
            Console.WriteLine("Invalid Rec");

          }
        }
      }
      else
      {
        Console.WriteLine("broadcast");
        foreach (var socket in _manager.GetAllSockets())
        {
          if (socket.Value.State == WebSocketState.Open)
          {
            await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()),
            WebSocketMessageType.Text, true, CancellationToken.None);
          }
        }
      }
    }

  }


}