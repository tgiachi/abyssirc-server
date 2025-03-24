using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Interfaces.Listener;

namespace AbyssIrc.Server.Interfaces.Services;

public interface IIrcManagerService
{
    Task DispatchMessageAsync(string id, IIrcCommand command);

    void RegisterListener(string command, IIrcMessageListener listener);
}
