using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Network;

namespace AbyssIrc.Server.Core.Interfaces.Listeners;

public interface IIrcCommandListener
{
    Task HandleAsync(NetworkSessionData session, IIrcCommand command);

}
