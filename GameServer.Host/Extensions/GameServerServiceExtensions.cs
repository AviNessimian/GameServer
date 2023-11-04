using GameServer.Core.Bases;
using GameServer.Core.Contracts;
using GameServer.Core.Handlers;
using GameServer.Host.Middlewares;
using GameServer.Infrastructure.Services;
using GameServer.Infrastructure.WebSockets;

namespace GameServer.Host.Extensions;

public static class GameServerServiceExtensions
{
    public static IServiceCollection AddGameServer(this IServiceCollection services, IConfiguration Configuration)
    {
        services.AddOptions();
        services.Configure<WebSocketConnsOptions>(Configuration.GetSection("WebSocketConns"));

        services.AddHostedService<PlayersSeedService>();

        services.TryAddSingleton<IConnectionsStore, WebSocketConnsStore>();
        services.TryAddSingleton<IPlayerService, PlayerService>();

        services.AddSingleton<IHandler, LoginHandler>();
        services.AddSingleton<IHandler, UpdateCoinsHandler>();
        
        return services;
    }

    public static IApplicationBuilder MapWebSocketConnections(this IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.UseMiddleware<WebSocketMiddleware>();
        return app;
    }
}