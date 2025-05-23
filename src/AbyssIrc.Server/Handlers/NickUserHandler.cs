using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Commands.Replies;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Protocol.Messages.Types;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Handlers.Base;
using AbyssIrc.Signals.Interfaces.Listeners;

namespace AbyssIrc.Server.Handlers;

public class NickUserHandler : BaseHandler, IIrcMessageListener, IAbyssSignalListener<ClientDisconnectedEvent>
{
    private readonly HashSet<string> _readySessions = new();


    public NickUserHandler(
        ILogger<NickUserHandler> logger, IServiceProvider serviceProvider
    ) : base(
        logger,
        serviceProvider
    )
    {
        SubscribeSignal(this);
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);
        if (command is UserCommand userCommand)
        {
            await HandleUserCommand(session, userCommand);
        }

        if (command is NickCommand nickCommand)
        {
            await HandleNickCommand(session, nickCommand);
        }

        if (command is IsonCommand isonCommand)
        {
            await HandleIsonCommand(session, isonCommand);
        }

        if (command is ModeCommand { TargetType: ModeTargetType.User } modeCommand)
        {
            await HandleModeCommand(session, modeCommand);
        }
    }

    private async Task HandleModeCommand(IrcSession session, ModeCommand command)
    {
        foreach (var modeChange in command.ModeChanges)
        {
            if (modeChange.IsAdding)
            {
                session.AddMode(modeChange.Mode);
            }
            else
            {
                session.RemoveMode(modeChange.Mode);
            }
        }

        await SendIrcMessageAsync(
            session.Id,
            ModeCommand.CreateWithModes(
                Hostname,
                session.Nickname,
                session.ModesString.Select(s => new ModeChangeType(true, s)).ToArray()
            )
        );
    }

    private async Task HandleIsonCommand(IrcSession session, IsonCommand command)
    {
        var response = new RplIson()
        {
            Nickname = session.Nickname,
            ServerName = Hostname
        };

        foreach (var nickname in command.Nicknames)
        {
            var sessionWithSameNick = QuerySessions(s => s.Nickname == nickname).FirstOrDefault();
            if (sessionWithSameNick != null)
            {
                response.OnlineNicknames.Add(sessionWithSameNick.Nickname);
            }
        }

        if (response.OnlineNicknames.Count > 0)
        {
            await SendIrcMessageAsync(session.Id, response);
        }
        else
        {
            await SendIrcMessageAsync(
                session.Id,
                ErrNoSuchNick.Create(ServerData.Hostname, session.Nickname, command.Nicknames.FirstOrDefault())
            );
        }
    }

    private async Task HandleUserCommand(IrcSession session, UserCommand userCommand)
    {
        session.Username = userCommand.Username;
        session.RealName = userCommand.RealName ?? string.Empty;

        session.IsUserSent = true;

        Logger.LogDebug("User command received: {Username} {RealName}", userCommand.Username, userCommand.RealName);

        await CheckClientReady(session);
    }

    private async Task HandleNickCommand(IrcSession session, NickCommand nickCommand)
    {
        var sessionWithSameNicks = QuerySessions(s => s.Nickname == nickCommand.Nickname);

        if (sessionWithSameNicks.Any())
        {
            Logger.LogWarning("Client {Nickname} already exists", nickCommand.Nickname);
            if (!session.IsRegistered)
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNicknameInUse.CreateForUnregistered(ServerData.Hostname, nickCommand.Nickname)
                );
            }
            else
            {
                await SendIrcMessageAsync(
                    session.Id,
                    ErrNicknameInUse.Create(ServerData.Hostname, session.Nickname, nickCommand.Nickname)
                );
            }


            return;
        }


        session.Nickname = nickCommand.Nickname;
        session.IsNickSent = true;
        Logger.LogDebug("Nick command received: {Nickname}", nickCommand.Nickname);

        await CheckClientReady(session);
    }

    private async Task CheckClientReady(IrcSession session)
    {
        if (session.IsRegistered)
        {
            _readySessions.Add(session.Id);
            Logger.LogInformation(
                "Client {Nickname} ({Username}) is now registered",
                session.Nickname,
                session.Username
            );


            await SendSignalAsync(new ClientReadyEvent(session.Id));
        }
    }

    public Task OnEventAsync(ClientDisconnectedEvent signalEvent)
    {
        _readySessions.Remove(signalEvent.Id);
        return Task.CompletedTask;
    }
}
