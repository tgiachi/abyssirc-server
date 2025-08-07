namespace AbyssIrc.Core.Interfaces.Services;

public interface IAbyssStarStopService : IAbyssService
{
    Task StartAsync();

    Task StopAsync();
}
