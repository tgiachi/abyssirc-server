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
    private readonly ISchedulerSystemService _schedulerSystemService;


    public PingPongHandler(
        ILogger<PingPongHandler> logger,
        ISchedulerSystemService schedulerSystemService, IServiceProvider serviceProvider
    ) : base(logger, serviceProvider)
    {
        _schedulerSystemService = schedulerSystemService;

        _schedulerSystemService.RegisterJob("ping_pong", PingConnectedClients, TimeSpan.FromSeconds(1));
        _schedulerSystemService.RegisterJob("disconneted_dead_clients", DisconnectedDeadClient, TimeSpan.FromSeconds(1));
    }

    private async Task DisconnectedDeadClient()
    {
        var deadSessions = GetSessions()
            .Where(s => s.LastPongReceived.AddSeconds(ServerConfig.Network.PingTimeout) < DateTime.Now && s.IsRegistered);

        foreach (var session in deadSessions)
        {
            Logger.LogWarning("Disconnecting dead session {Nickname}", session.Nickname);

            await SendIrcMessageAsync(
                session.Id,
                ErrorCommand.CreatePingTimeout(
                    ServerData.Hostname,
                    session.Nickname,
                    session.HostName,
                    ServerConfig.Network.PingTimeout
                )
            );

            await SendSignalAsync(new DisconnectedClientSessionEvent(session.Id));
        }
    }

    private async Task PingConnectedClients()
    {
        var needToPingSessions = GetSessions()
            .Where(s => s.LastPingSent.AddSeconds(ServerConfig.Network.PingInterval) < DateTime.Now && s.IsRegistered);

        // var currentTimestampInSeconds = DateTime.Now.ToUnixTimestamp();
        foreach (var session in needToPingSessions)
        {
            Logger.LogDebug("Pinging user {Nickname}", session.Nickname);
            await SendIrcMessageAsync(
                session.Id,
                new PingCommand(ServerData.Hostname, "TIMEOUTCHECK")
            );
            session.LastPingSent = DateTime.Now;
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
        var session = GetSession(id);
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

        if (session.LastPingSent.AddSeconds(ServerConfig.Network.PingInterval) < DateTime.Now)
        {
            Logger.LogWarning("Received PONG from session {Id} that was not pinged", id);
            return;
        }

        Logger.LogDebug("Received PONG from {Nickname}", session.Nickname);
        session.LastPongReceived = DateTime.Now;
    }

    private async Task HandlePingCommand(string id, PingCommand pingCommand)
    {
        var session = GetSession(id);
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
            new PongCommand(ServerConfig.Network.Host, pingCommand.Token)
        );
    }
}
