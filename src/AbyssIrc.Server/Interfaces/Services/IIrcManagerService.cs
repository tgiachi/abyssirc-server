using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Server.Interfaces.Services;

public interface IIrcManagerService
{
    Task DispatchMessageAsync(string id, IIrcCommand command);
}
