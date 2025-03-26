using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Interfaces.Listener;

namespace AbyssIrc.Server.Interfaces.Services.Server;

public interface IIrcManagerService
{
    Task DispatchMessageAsync(string id, IIrcCommand command);

    void RegisterListener(IIrcCommand command, IIrcMessageListener listener);
}
