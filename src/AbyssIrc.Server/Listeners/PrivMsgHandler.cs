using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class PrivMsgHandler : BaseHandler, IIrcMessageListener
{
    public PrivMsgHandler(
        ILogger<PrivMsgHandler> logger, IServiceProvider serviceProvider
    ) : base(logger, serviceProvider)
    {
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);

        if (command is PrivMsgCommand privMsgCommand)
        {
            if (!privMsgCommand.IsUserMessage)
            {
                return HandleUserToUserMessage(session, privMsgCommand);
            }
        }

        return Task.CompletedTask;
    }

    private async Task HandleUserToUserMessage(IrcSession session, PrivMsgCommand command)
    {
        if (command.IsChannelMessage)
        {
            return;
        }

        var targetNickName = command.Target;

        var targetSession =
            QuerySessions(s => s.Nickname.Equals(targetNickName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

        if (targetSession == null)
        {
            await SendIrcMessageAsync(
                session.Id,
                new ErrNoSuchNick(ServerData.Hostname, session.Nickname, targetNickName)
            );
        }

        await SendIrcMessageAsync(
            targetSession.Id,
            new PrivMsgCommand(session.Nickname, targetSession.Nickname, command.Message)
        );

        await SendSignalAsync(new PrivMsgEvent(session.Nickname, targetNickName, command.Message));

        session.UpdateActivity();
    }
}
