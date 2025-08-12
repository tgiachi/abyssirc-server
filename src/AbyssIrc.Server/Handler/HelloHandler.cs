using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Config;
using AbyssIrc.Server.Core.Data.Network;
using AbyssIrc.Server.Core.Interfaces.Listeners;
using AbyssIrc.Server.Core.Interfaces.Services;
using AbyssIrc.Server.Core.Types.Sessions;
using Serilog;

namespace AbyssIrc.Server.Handler;

public class HelloHandler : IIrcCommandListener
{
    private readonly ILogger _logger = Log.ForContext<HelloHandler>();

    private readonly AbyssIrcServerConfig _abyssIrcServerConfig;

    private readonly IUserManagerService _userManagerService;

    public HelloHandler(IUserManagerService userManagerService, AbyssIrcServerConfig abyssIrcServerConfig)
    {
        _userManagerService = userManagerService;
        _abyssIrcServerConfig = abyssIrcServerConfig;
    }

    public async Task HandleAsync(NetworkSessionData session, IIrcCommand command)
    {
        if (command is UserCommand userCommand)
        {
            await HandleUserAsync(session, userCommand);
        }

        if (command is NickCommand nickCommand)
        {
            await HandleNicknameAsync(session, nickCommand);
        }
    }

    private async Task HandleNicknameAsync(NetworkSessionData session, NickCommand command)
    {
        if (string.IsNullOrEmpty(command.Nickname))
        {
            session.SendMessages(ErrNeedMoreParams.Create(_abyssIrcServerConfig.Server.Network, string.Empty, command.Code));
            return;
        }

        if (await _userManagerService.NicknameExists(command.Nickname))
        {
            session.SendMessages(
                ErrAlreadyRegistered.Create(_abyssIrcServerConfig.Server.Hostname, command.Nickname, command.Code)
            );

            return;
        }

        session.Nickname = command.Nickname;

        session.AuthStatus |= SessionAuthStatusType.Nickname;
    }

    private async Task HandleUserAsync(NetworkSessionData session, UserCommand command)
    {
        if (string.IsNullOrEmpty(command.Username))
        {
            session.SendMessages(ErrNeedMoreParams.Create(_abyssIrcServerConfig.Server.Network, string.Empty, command.Code));
            return;
        }

        if (await _userManagerService.UserExists(command.Username))
        {
            session.SendMessages(ErrAlreadyRegistered.Create(_abyssIrcServerConfig.Server.Hostname, command.Username));

            return;
        }

        session.Username = command.Username;
        session.RealName = command.RealName;

        session.AuthStatus |= SessionAuthStatusType.Username;
    }
}
