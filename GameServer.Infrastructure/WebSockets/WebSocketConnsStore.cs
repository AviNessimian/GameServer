using GameServer.Core.Contracts;

namespace GameServer.Infrastructure.WebSockets;

public class WebSocketConnsStore : IConnectionsStore
{
    private readonly ConcurrentDictionary<string, IConnection> _connections
        = new ConcurrentDictionary<string, IConnection>();

    public bool IsConnectionExists(string playerId) => _connections.ContainsKey(playerId);

    public void Add(IConnection connection)
        => _connections.TryAdd(connection.ConnectionId, connection);

    public void Remove(string playerId)
        => _connections.TryRemove(playerId, out var connection);

    public Task SendToAllAsync(string message, CancellationToken cancellationToken)
    {
        var connsTasks = _connections.Values.Select(conn => conn.Send(message, cancellationToken));
        return Task.WhenAll(connsTasks);
    }

    public IConnection Get(string playerId)
    {
        if (_connections.TryGetValue(playerId, out var connection))
        {
            return connection;
        }

        throw new Exception($"Connection not found for the id {playerId}");
    }
}
