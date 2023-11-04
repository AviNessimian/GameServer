using GameServer.Core.Bases;
using GameServer.Core.Contracts;

namespace GameServer.Core.Handlers;

public class LoginHandler : IHandler
{
    private readonly ILogger<LoginHandler> _logger;
    private readonly IPlayerService _playerService;
    private readonly IConnectionsStore _connectionsStore;

    public LoginHandler(
        ILogger<LoginHandler> logger,
        IPlayerService playerService,
        IConnectionsStore connectionsStore)
    {
        _logger = logger;
        _playerService = playerService;
        _connectionsStore = connectionsStore;
    }

    public string RoutePath => "Login";

    public async Task Handle(string connectionId, string deviceId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionId, nameof(connectionId));

        var conn = _connectionsStore.Get(connectionId);

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            await conn.Send("Error: DeviceId is empty", cancellationToken);
            return;
        }

        var player  = await _playerService.GetByDeviceId(deviceId);
        if (player is null)
        {
            await conn.Send("Error: Device not found", cancellationToken);
            return;
        }

        if (!string.IsNullOrEmpty(player.ConnectionId) && _connectionsStore.IsConnectionExists(player.ConnectionId))
        {
            _logger.LogInformation("Player {PlayerId} is already connected", player.ConnectionId);
            await conn.Send("Error: Player already connected", cancellationToken);
            return;
        }

        await _playerService.SetConnectionId(player.Id, connectionId);
        await conn.Send(player.Id, cancellationToken);
    }
}
