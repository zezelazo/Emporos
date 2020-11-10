using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Emporos.Middleware
{
    public interface IWebSocketServerConnectionManager
    {
        ConcurrentDictionary<string, WebSocket> GetAllSockets();

        string AddSocket(WebSocket socket);
    }
}