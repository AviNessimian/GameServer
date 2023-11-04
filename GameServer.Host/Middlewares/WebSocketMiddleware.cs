using GameServer.Core.Bases;
using GameServer.Core.Contracts;
using GameServer.Infrastructure.WebSockets;

namespace GameServer.Host.Middlewares;

internal class WebSocketMiddleware
{
    private readonly ILogger<WebSocketMiddleware> _logger;
    private readonly WebSocketConnsOptions _options;
    private readonly IConnectionsStore _connectionsStore;
    private readonly Dictionary<string, IHandler> _handlers;

    public WebSocketMiddleware(
        RequestDelegate next,
        ILogger<WebSocketMiddleware> logger,
        IOptions<WebSocketConnsOptions> options,
        IConnectionsStore connectionsStore,
        IEnumerable<IHandler> handlers)
    {
        _logger = logger;
        _options = options.Value;
        _connectionsStore = connectionsStore;
        _handlers = handlers.ToDictionary(_ => _.RoutePath.ToLower(), _ => _);
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (!IsAllowedOrigin(context))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        IConnection webSocketConnection = new WebSocketConn(webSocket, _options, (connectionId, binaryMassage) =>
        {
            var massage = Encoding.UTF8.GetString(binaryMassage);
            if (string.IsNullOrWhiteSpace(massage) || !massage.Contains('/'))
            {
                _logger.LogWarning("route not found, {Massage}", massage);
                return;
            }

            var routeSplit = massage.Split('/');
            var route = routeSplit[0].ToLower();
            if (!_handlers.ContainsKey(route))
            {
                _logger.LogWarning("route handler not exists, {Massage}", massage);
                return;
            }

            _handlers[route].Handle(connectionId, routeSplit[1]).GetAwaiter().GetResult();
        });

        _connectionsStore.Add(webSocketConnection);

        await webSocketConnection.ReceiveUntilClosed();

        _connectionsStore.Remove(webSocketConnection.ConnectionId);
    }

    private bool IsAllowedOrigin(HttpContext context)
    {
        if (_options.AllowedAllOrigins) return true;

        var origin = context.Request.Headers["Origin"].ToString();
        var isAllowed = _options.AllowedOrigins.Contains(origin);

        if (!isAllowed)
        {
            _logger.LogWarning("Origin not allowed, {Origin}", origin);
        }

        return isAllowed;
    }
}