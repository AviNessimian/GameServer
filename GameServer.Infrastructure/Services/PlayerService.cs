using GameServer.Core.Contracts;
using GameServer.Core.Models;

namespace GameServer.Infrastructure.Services;

public class PlayerService : IPlayerService
{
    private readonly ConcurrentBag<Player> _players = new ConcurrentBag<Player>();
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(ILogger<PlayerService> logger)
    {
        _logger = logger;
    }

    public Task<Player> Create(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException(nameof(deviceId));
        }

        var newPlayer = new Player
        {
            Id = Guid.NewGuid().ToString(),
            Coins = 0,
            DeviceId = deviceId
        };

        _players.Add(newPlayer);

        return Task.FromResult(newPlayer);
    }

    public Task<Player?> GetById(string playerId)
    {
         return Task.FromResult(_players.SingleOrDefault(_ => _.Id == playerId));
    }

    public Task<Player?> GetByDeviceId(string deviceId)
    {
        return Task.FromResult(_players.SingleOrDefault(_ => _.DeviceId == deviceId));
    }

    public Task<Player?> GetByConnectionId(string connectionId)
    {
        return Task.FromResult(_players.SingleOrDefault(_ => _.ConnectionId == connectionId));
    }

    public async Task SetConnectionId(string playerId, string connectionId)
    {
        var player = await GetById(playerId);
        if (player is null)
        {
            _logger.LogWarning("Player not found, {PlayerId}", playerId);
            return;
        };

        player.ConnectionId = connectionId;
    }
}