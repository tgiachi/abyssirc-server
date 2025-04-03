using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Listeners.Base;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class TimeHandler : BaseHandler, IIrcMessageListener
{
    public TimeHandler(ILogger<TimeHandler> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        if (command is TimeCommand timeCommand)
        {
            var session = GetSession(id);
            await HandleTimeCommand(session, timeCommand);
        }
    }

    private async Task HandleTimeCommand(IrcSession session, TimeCommand command)
    {
        var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


        await SendIrcMessageAsync(
            session.Id,
            RplTime.Create(
                Hostname,
                session.Nickname,
                time
            )
        );
    }
}
