using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Types;

namespace AbyssIrc.Server.Interfaces.Services.System;

public interface ITcpService
{
    Task StartAsync();

    Task StopAsync();

    Task ParseCommandAsync(TcpServerType serverType, string sessionId, string command);

    Task SendMessagesAsync(string sessionId, List<string> messages);

    Task SendIrcMessagesAsync(string sessionId, params IIrcCommand[] commands);

    void Disconnect(string sessionId);
}
