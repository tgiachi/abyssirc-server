using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Interfaces.Listener;

namespace AbyssIrc.Server.Listeners;

public class QuitMessageHandler : IIrcMessageListener
{
    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        return Task.FromResult<IIrcCommand?>(null);
    }
}
