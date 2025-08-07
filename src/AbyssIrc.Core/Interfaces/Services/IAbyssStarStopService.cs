namespace AbyssIrc.Core.Interfaces.Services;

public interface IAbyssStarStopService : IAbyssService
{
    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
