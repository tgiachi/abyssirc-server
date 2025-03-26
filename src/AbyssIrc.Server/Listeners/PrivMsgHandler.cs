using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Errors;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class PrivMsgHandler : BaseHandler, IIrcMessageListener
{
    private readonly AbyssIrcConfig _abyssIrcConfig;

    public PrivMsgHandler(
        ILogger<PrivMsgHandler> logger, IAbyssSignalService signalService, ISessionManagerService sessionManagerService,
        AbyssIrcConfig abyssIrcConfig
    ) : base(logger, signalService, sessionManagerService)
    {
        _abyssIrcConfig = abyssIrcConfig;
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);
        var privMessage = (PrivMsgCommand)command;

        if (!privMessage.Target.StartsWith("@"))
        {
            return HandleUserToUserMessage(session, privMessage);
        }

        return Task.CompletedTask;
    }

    private async Task HandleUserToUserMessage(IrcSession session, PrivMsgCommand command)
    {
        var targetNickName = command.Target;

        var targetSession =
            GetSessionQuery(s => s.Nickname.Equals(targetNickName, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

        if (targetSession == null)
        {
            await SendIrcMessageAsync(
                session.Id,
                new ErrNoSuchNick(_abyssIrcConfig.Network.Host, session.Nickname, targetNickName)
            );
        }

        await SendIrcMessageAsync(
            targetSession.Id,
            new PrivMsgCommand(session.Nickname, targetSession.Nickname, command.Message)
        );

        await SendSignalAsync(new PrivMsgEvent(session.Nickname, targetNickName, command.Message));
    }
}
