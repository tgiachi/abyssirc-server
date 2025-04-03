using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Server.Core.Interfaces.Listener;

public interface IIrcMessageListener
{
    Task OnMessageReceivedAsync(string id, IIrcCommand command);
}
