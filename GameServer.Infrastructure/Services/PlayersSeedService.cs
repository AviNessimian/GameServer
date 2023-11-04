using GameServer.Core.Contracts;


namespace GameServer.Infrastructure.Services;

public class PlayersSeedService : IHostedService
{
    private readonly Task _completedTask = Task.CompletedTask;
    private readonly IPlayerService _playerService;

    public PlayersSeedService(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _playerService.Create("1");
        await _playerService.Create("2");
        await _playerService.Create("3");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _completedTask;
    }
}
