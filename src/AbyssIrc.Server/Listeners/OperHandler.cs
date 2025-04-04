using AbyssIrc.Protocol.Messages;
using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Protocol.Messages.Types;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Events.Commands;
using AbyssIrc.Server.Core.Events.Opers;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Core.Utils.Hosts;
using AbyssIrc.Server.Listeners.Base;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class OperHandler : BaseHandler, IIrcMessageListener
{
    public OperHandler(ILogger<OperHandler> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);

        if (command is OperCommand operCommand)
        {
            return HandleOperMessage(session, operCommand);
        }

        if (command is KillCommand killCommand)
        {
            return HandleKillMessage(session, killCommand);
        }

        return Task.CompletedTask;
    }

    private async Task HandleKillMessage(IrcSession session, KillCommand killCommand)
    {
        if (!session.IsOperator)
        {
            await SendSignalAsync(new KillAttemptNoPrivEvent(session.UserMask, session.Id));
            await SendIrcMessageAsync(session.Id, ErrNoPrivileges.Create(Hostname, session.Nickname));
            return;
        }

        var targetSession = GetSessionByNickname(killCommand.TargetNickname);

        if (targetSession == null)
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchNick.Create(Hostname, session.Nickname, killCommand.TargetNickname)
            );
            return;
        }

        if (targetSession.IsOperator)
        {
            await SendIrcMessageAsync(session.Id, ErrCantKillServer.Create(Hostname, session.Nickname));
            return;
        }

        await SendIrcMessageAsync(
            targetSession.Id,
            KillCommand.Create(targetSession.Nickname, killCommand.Reason)
        );

        await SendSignalAsync(new QuitRequestEvent(targetSession.Id, "KILLED: " + killCommand.Reason));
    }

    private async Task HandleOperMessage(IrcSession session, OperCommand command)
    {
        var operUsername = ServerConfig.Opers.Users.FirstOrDefault(u => u.Username == command.Username);

        if (operUsername == null)
        {
            await SendSignalAsync(new OperUsernamePasswordWrongAttemptEvent(session.UserMask, session.Id));

            await SendIrcMessageAsync(session.Id, ErrNoOperHost.Create(Hostname, session.Nickname));

            return;
        }

        if (!HostMaskUtils.IsHostMaskMatch(operUsername.Host, session.UserMask))
        {
            await SendSignalAsync(new OperHostMismatchEvent(session.UserMask, session.Id));

            await SendIrcMessageAsync(session.Id, ErrPasswdMismatch.Create(Hostname, session.Nickname));

            return;
        }

        if (operUsername.Password != command.Password)
        {
            await SendSignalAsync(new OperUsernamePasswordWrongAttemptEvent(session.UserMask, session.Id));

            await SendIrcMessageAsync(session.Id, ErrPasswdMismatch.Create(Hostname, session.Nickname));

            return;
        }

        session.AddMode('o');

        await SendSignalAsync(new OperConnectedEvent(session.UserMask, session.Id));

        await SendIrcMessageAsync(
            session.Id,
            RplYoureOper.Create(Hostname, session.Nickname, session.Username)
        );

        await SendIrcMessageAsync(
            session.Id,
            ModeCommand.CreateWithModes(Hostname, session.Nickname, new ModeChangeType(true, 'o'))
        );
    }
}
