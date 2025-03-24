using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events;

namespace AbyssIrc.Server.Interfaces.Listener;

public interface IIrcMessageListener
{
    Task<IIrcCommand?> OnMessageReceivedAsync(string id, IIrcCommand command);
}
