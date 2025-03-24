namespace AbyssIrc.Server.Interfaces.Services;

public interface ITcpService
{
    Task StartAsync();

    Task StopAsync();

    Task ParseCommandAsync(string sessionId, string command);

    Task SendMessagesAsync(string sessionId, List<string> messages);


}
