using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Listener;
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
