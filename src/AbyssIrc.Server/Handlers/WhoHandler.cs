using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Handlers.Base;

namespace AbyssIrc.Server.Handlers;

public class WhoHandler : BaseHandler, IIrcMessageListener
{
    private readonly IChannelManagerService _channelManagerService;


    public WhoHandler(
        ILogger<WhoHandler> logger, IServiceProvider serviceProvider, IChannelManagerService channelManagerService
    ) : base(logger, serviceProvider)
    {
        _channelManagerService = channelManagerService;
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        if (command is WhoCommand whoCommand)
        {
            return HandleWhoCommandAsync(GetSession(id), whoCommand);
        }

        if (command is WhoIsCommand whoIsCommand)
        {
            return HandleWhoIsCommandAsync(GetSession(id), whoIsCommand);
        }

        return Task.CompletedTask;
    }

    private async Task HandleWhoCommandAsync(IrcSession session, WhoCommand command)
    {
    }

    private async Task HandleWhoIsCommandAsync(IrcSession session, WhoIsCommand command)
    {
    }
}
