using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Network;
using AbyssIrc.Server.Core.Interfaces.Listeners;
using Serilog;

namespace AbyssIrc.Server.Handler;

public class HelloHandler : IIrcCommandListener
{
    private readonly ILogger _logger = Log.ForContext<HelloHandler>();

    public Task HandleAsync(NetworkSessionData session, IIrcCommand command)
    {
        _logger.Information("Hit the road jack");
        return Task.CompletedTask;
    }
}
