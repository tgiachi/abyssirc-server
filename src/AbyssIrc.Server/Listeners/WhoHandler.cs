using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Listeners.Base;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class WhoHandler : BaseHandler, IIrcMessageListener
{
    public WhoHandler(ILogger<WhoHandler> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        return Task.CompletedTask;
    }
}
