using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Errors;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Events.Core;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Data.Events.Opers;
using AbyssIrc.Server.Listeners.Base;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class ServerCommandsListener : BaseHandler, IIrcMessageListener
{
    public ServerCommandsListener(ILogger<ServerCommandsListener> logger, IServiceProvider serviceProvider) : base(
        logger,
        serviceProvider
    )
    {
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);

        if (command is RestartCommand restartCommand)
        {
            await OnRestartRequestAsync(session, restartCommand);
        }
    }


    private async Task OnRestartRequestAsync(IrcSession session, RestartCommand command)
    {
        if (session.IsOperator)
        {
            Logger.LogWarning("!!!! Restart request received from {Name}: {Reason}", session.Nickname, command.Reason);

            await SendSignalAsync(new ServerRestartRequestEvent(command.Reason));
        }
        else
        {
            Logger.LogWarning("!!!! Restart request received from {Name} but user is not an operator", session.Nickname);

            await SendSignalAsync(new UnauthorizedOperationEvent(session.HostName, session.Nickname, command.Code));
            await SendIrcMessageAsync(
                session.Id,
                ErrNoPrivileges.Create(Hostname, session.Nickname)
            );
        }
    }
}
