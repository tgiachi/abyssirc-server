using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Interfaces.Listener;

namespace AbyssIrc.Server.Interfaces.Services.Server;

public interface IIrcManagerService : IAbyssStarStopService
{
    Task DispatchMessageAsync(string id, string command);

    void RegisterListener(IIrcCommand command, IIrcMessageListener listener);

    void RegisterListener(string commandCode, Func<string, IIrcCommand, Task> callback);

    Task StartAsync();
}
