namespace GameServer.Core.Bases;

public interface IHandler
{
    public string RoutePath { get; }
    Task Handle(string connectionId, string message, CancellationToken cancellationToken = default);
}
