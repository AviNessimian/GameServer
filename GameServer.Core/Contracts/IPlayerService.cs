using GameServer.Core.Models;

namespace GameServer.Core.Contracts;

public interface IPlayerService
{
    Task<Player?> GetById(string playerId);
    Task<Player?> GetByDeviceId(string deviceId);
    Task<Player?> GetByConnectionId(string connectionId);
    Task<Player> Create(string deviceId);
    Task SetConnectionId(string playerId, string connectionId);
}
