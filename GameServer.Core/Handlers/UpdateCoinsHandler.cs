using GameServer.Core.Bases;
using GameServer.Core.Contracts;

namespace GameServer.Core.Handlers;

public class UpdateCoinsHandler : IHandler
{
    private readonly ILogger<UpdateCoinsHandler> _logger;
    private readonly IPlayerService _playerService;
    private readonly IConnectionsStore _connectionsStore;

    public UpdateCoinsHandler(
        ILogger<UpdateCoinsHandler> logger,
        IPlayerService playerService,
        IConnectionsStore connectionsStore)
    {
        _logger = logger;
        _playerService = playerService;
        _connectionsStore = connectionsStore;
    }

    public string RoutePath => "UpdateCoins";

    public async Task Handle(string connectionId, string coins, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionId, nameof(connectionId));

        var connection = _connectionsStore.Get(connectionId);

        if (!int.TryParse(coins, out int amount) || amount <= 0)
        {
            _logger.LogWarning("invalid coins input, {Coins}", coins);
            await connection.Send("Error: invalid coins input", cancellationToken);
            return;
        }

        var player = await _playerService.GetByConnectionId(connectionId);
        if (player is null)
        {
            _logger.LogWarning("Connection {ConnectionId} not found", connectionId);
            await connection.Send("Error: Connection not found", cancellationToken);
            return;
        }

        player.Coins += amount;

        await connection.Send(player.Coins.ToString(), cancellationToken);
    }
}
