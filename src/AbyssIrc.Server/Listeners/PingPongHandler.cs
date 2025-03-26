using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.TcpServer;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class PingPongHandler : BaseHandler, IIrcMessageListener
{
    private readonly AbyssIrcConfig _abyssIrcConfig;
    private readonly ISchedulerSystemService _schedulerSystemService;

    private readonly ISessionManagerService _sessionManagerService;

    public PingPongHandler(
        ILogger<PingPongHandler> logger, IAbyssSignalService signalService, AbyssIrcConfig abyssIrcConfig,
        ISchedulerSystemService schedulerSystemService, ISessionManagerService sessionManagerService
    ) : base(logger, signalService)
    {
        _abyssIrcConfig = abyssIrcConfig;
        _schedulerSystemService = schedulerSystemService;
        _sessionManagerService = sessionManagerService;

        _schedulerSystemService.RegisterJob("ping_pong", PingConnectedClients, TimeSpan.FromSeconds(1));
        _schedulerSystemService.RegisterJob("disconneted_dead_clients", DisconnectedDeadClient, TimeSpan.FromSeconds(1));
    }

    private async Task DisconnectedDeadClient()
    {
        var deadSessions = _sessionManagerService.GetSessions()
            .Where(s => s.LastPong.AddSeconds(_abyssIrcConfig.Network.PingTimeout) < DateTime.Now && s.IsRegistered);

        foreach (var session in deadSessions)
        {
            Logger.LogWarning("Disconnecting dead session {Nickname}", session.Nickname);

            await SendIrcMessageAsync(
                session.Id,
                ErrorCommand.CreatePingTimeout(
                    _abyssIrcConfig.Network.Host,
                    session.Nickname,
                    session.HostName,
                    _abyssIrcConfig.Network.PingTimeout
                )
            );

            await SendSignalAsync(new DisconnectedClientSessionEvent(session.Id));
        }
    }

    private async Task PingConnectedClients()
    {
        var needToPingSessions = _sessionManagerService.GetSessions()
            .Where(s => s.LastPing.AddSeconds(_abyssIrcConfig.Network.PingInterval) < DateTime.Now && s.IsRegistered);

        // var currentTimestampInSeconds = DateTime.Now.ToUnixTimestamp();
        foreach (var session in needToPingSessions)
        {
            Logger.LogDebug("Pinging user {Nickname}", session.Nickname);
            await SendIrcMessageAsync(
                session.Id,
                new PingCommand(_abyssIrcConfig.Network.Host, "TIMEOUTCHECK")
            );
            session.LastPing = DateTime.Now;
        }
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        return command switch
        {
            PingCommand pingCommand => HandlePingCommand(id, pingCommand),
            PongCommand pongCommand => HandlePongCommand(id, pongCommand),
            _                       => Task.CompletedTask
        };
    }

    private async Task HandlePongCommand(string id, PongCommand pongCommand)
    {
        var session = _sessionManagerService.GetSession(id);
        if (session == null)
        {
            Logger.LogWarning("Received PONG from unknown session {Id}", id);
            return;
        }

        if (!session.IsRegistered)
        {
            Logger.LogWarning("Received PONG from unregistered session {Id}", id);
            return;
        }

        if (session.LastPing.AddSeconds(_abyssIrcConfig.Network.PingInterval) < DateTime.Now)
        {
            Logger.LogWarning("Received PONG from session {Id} that was not pinged", id);
            return;
        }

        Logger.LogDebug("Received PONG from {Nickname}", session.Nickname);
        session.LastPong = DateTime.Now;
    }

    private async Task HandlePingCommand(string id, PingCommand pingCommand)
    {
        var session = _sessionManagerService.GetSession(id);
        if (session == null)
        {
            Logger.LogWarning("Received PING from unknown session {Id}", id);
            return;
        }

        if (!session.IsRegistered)
        {
            Logger.LogWarning("Received PING from unregistered session {Id}", id);
            return;
        }

        Logger.LogDebug("Received PING from {Nickname}", session.Nickname);
        await SendIrcMessageAsync(
            id,
            new PongCommand(_abyssIrcConfig.Network.Host, pingCommand.Token)
        );
    }
}
