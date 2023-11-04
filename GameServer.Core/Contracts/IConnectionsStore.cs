namespace GameServer.Core.Contracts;

public interface IConnectionsStore
{
    bool IsConnectionExists(string connectionId);
    IConnection Get(string connectionId);
    void Add(IConnection connection);
    void Remove(string connectionId);
}
