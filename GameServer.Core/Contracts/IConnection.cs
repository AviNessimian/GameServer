namespace GameServer.Core.Contracts;

public interface IConnection
{
    string ConnectionId { get; }
    bool IsClosed { get; }
    bool IsOpen { get; }

    Task Send(string message, CancellationToken cancellationToken = default);
    Task ReceiveUntilClosed();

}
