namespace AbyssIrc.Server.Interfaces;

public interface ITcpService
{
    Task StartAsync();

    Task StopAsync();
}
