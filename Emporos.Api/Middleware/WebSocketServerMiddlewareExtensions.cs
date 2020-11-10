using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Emporos.Middleware
{
  public static class WebSocketServerMiddlewareExtensions
  {
    public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<WebsocketServerMiddleware>();
    }
    
    public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
    {
      services.AddSingleton<WebSocketServerConnectionManager>();
      return services;
    }
  }
}