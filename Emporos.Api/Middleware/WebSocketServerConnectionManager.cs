using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emporos.Middleware
{
    public class WebSocketServerConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets  = new ConcurrentDictionary<string, WebSocket>();

        public ConcurrentDictionary<string, WebSocket> GetAllSockets()
        {
          return _sockets;
        }

        public async Task BroadcastMessage(string message)
        {
          foreach(WebSocket socket in _sockets.Values){
            var buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
          }
        }

        public string AddSocket(WebSocket socket)
        {
          Console.WriteLine($"adding Conn");
          string ConnId = Guid.NewGuid().ToString();
          _sockets.TryAdd(ConnId, socket);
          Console.WriteLine($"Conn added {ConnId}");

          return ConnId;
        }
    }
}