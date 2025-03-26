using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class PrivMsgHandler : BaseHandler, IIrcMessageListener
{
    public PrivMsgHandler(ILogger<PrivMsgHandler> logger, IAbyssSignalService signalService) : base(logger, signalService)
    {
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        return Task.CompletedTask;
    }
}
