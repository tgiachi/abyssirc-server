using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Internal;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class NickUserHandler : BaseHandler, IIrcMessageListener
{
    private readonly ISessionManagerService _sessionManagerService;

    public NickUserHandler(
        ILogger<NickUserHandler> logger, IAbyssSignalService signalService, ISessionManagerService sessionManagerService
    ) : base(
        logger,
        signalService
    )
    {
        _sessionManagerService = sessionManagerService;
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = _sessionManagerService.GetSession(id);
        if (command is UserCommand userCommand)
        {
            await HandleUserCommand(session, userCommand);
        }


        if (command is NickCommand nickCommand)
        {
            await HandleNickCommand(session, nickCommand);
        }
    }

    private async Task HandleUserCommand(IrcSession session, UserCommand userCommand)
    {
        session.Username = userCommand.Username;
        session.RealName = userCommand.RealName;

        Logger.LogDebug("User command received: {Username} {RealName}", userCommand.Username, userCommand.RealName);
    }

    private async Task HandleNickCommand(IrcSession session, NickCommand nickCommand)
    {
        session.Nickname = nickCommand.Nickname;
        Logger.LogDebug("Nick command received: {Nickname}", nickCommand.Nickname);
    }
}
