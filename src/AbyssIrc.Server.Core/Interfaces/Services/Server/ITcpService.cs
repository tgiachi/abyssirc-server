using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Types;

namespace AbyssIrc.Server.Core.Interfaces.Services.Server;

public interface ITcpService
{
    Task StartAsync();

    Task StopAsync();

    Task ParseCommandAsync(TcpServerType serverType, string sessionId, string command);

    Task SendMessagesAsync(string sessionId, List<string> messages);

    Task SendIrcMessagesAsync(string sessionId, params IIrcCommand[] commands);

    void Disconnect(string sessionId);
}
