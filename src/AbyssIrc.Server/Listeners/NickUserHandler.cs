using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Errors;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Data.Internal;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class NickUserHandler : BaseHandler, IIrcMessageListener, IAbyssSignalListener<ClientDisconnectedEvent>
{
    private readonly ISessionManagerService _sessionManagerService;
    private readonly HashSet<string> _readySessions = new();
    private readonly AbyssIrcConfig _config;


    public NickUserHandler(
        ILogger<NickUserHandler> logger, IAbyssSignalService signalService, ISessionManagerService sessionManagerService,
        AbyssIrcConfig config
    ) : base(
        logger,
        signalService,
        sessionManagerService
    )
    {
        _sessionManagerService = sessionManagerService;
        _config = config;
        signalService.Subscribe(this);
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

        await CheckClientReady(session);
    }

    private async Task HandleNickCommand(IrcSession session, NickCommand nickCommand)
    {
        var sessionWithSameNicks = GetSessionQuery(s => s.Nickname == nickCommand.Nickname);

        if (sessionWithSameNicks.Any())
        {
            Logger.LogWarning("Client {Nickname} already exists", nickCommand.Nickname);
            if (!session.IsRegistered)
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNicknameInUse.CreateForUnregistered(_config.Network.Host, nickCommand.Nickname)
                );
            }
            else
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNicknameInUse.Create(_config.Network.Host, session.Nickname, nickCommand.Nickname)
                );
            }

            return;
        }

        session.Nickname = nickCommand.Nickname;
        Logger.LogDebug("Nick command received: {Nickname}", nickCommand.Nickname);

        await CheckClientReady(session);
    }

    private async Task CheckClientReady(IrcSession session)
    {
        if (!string.IsNullOrEmpty(session.Username) &&
            !string.IsNullOrEmpty(session.Nickname) &&
            _readySessions.Add(session.Id))
        {
            Logger.LogInformation(
                "Client {Nickname} ({Username}) is now registered",
                session.Nickname,
                session.Username
            );


            session.IsRegistered = true;
            await SendSignalAsync(new ClientReadyEvent(session.Id));
        }
    }

    public Task OnEventAsync(ClientDisconnectedEvent signalEvent)
    {
        _readySessions.Remove(signalEvent.Id);
        return Task.CompletedTask;
    }
}
