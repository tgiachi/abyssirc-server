using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Network;
using AbyssIrc.Server.Core.Interfaces.Listeners;

namespace AbyssIrc.Server.Handler;

public class CapabilityHandler : IIrcCommandListener
{
    public Task HandleAsync(NetworkSessionData session, IIrcCommand command)
    {
        return Task.CompletedTask;
    }
}
