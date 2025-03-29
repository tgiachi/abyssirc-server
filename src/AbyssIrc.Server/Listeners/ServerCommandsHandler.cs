using AbyssIrc.Core.Events.Core;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Listener;
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
        Logger.LogWarning("!!!! Restart request received from {Name}: {Reason}", session.Nickname, command.Reason);

        await SendSignalAsync(new ServerRestartRequestEvent(command.Reason));
    }
}
