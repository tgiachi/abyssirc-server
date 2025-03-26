namespace AbyssIrc.Core.Interfaces.Services;

public interface IAbyssStarStopService
{
    Task StartAsync();

    Task StopAsync();
}
