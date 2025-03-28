using System.Net;
using System.Net.Sockets;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Data.Events.Sessions;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class ConnectionHandler
    : BaseHandler, IAbyssSignalListener<SessionAddedEvent>, IAbyssSignalListener<SessionRemovedEvent>
{
    public ConnectionHandler(
        ILogger<ConnectionHandler> logger,
        IServiceProvider serviceProvider
    ) : base(logger, serviceProvider)
    {
        SubscribeSignal<SessionAddedEvent>(this);
        SubscribeSignal<SessionRemovedEvent>(this);
    }

    public async Task OnEventAsync(SessionAddedEvent signalEvent)
    {
        var session = GetSession(signalEvent.Id);
        await SendIrcMessageAsync(
            signalEvent.Id,
            NoticeAuthCommand.Create(ServerData.Hostname, "*** Looking up your hostname...")
        );

        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(session.IpAddress);

            if (hostEntry != null && !string.IsNullOrEmpty(hostEntry.HostName))
            {
                await SendIrcMessageAsync(
                    signalEvent.Id,
                    NoticeAuthCommand.Create(ServerData.Hostname, $"*** Found your hostname: {hostEntry.HostName}")
                );

                session.HostName = hostEntry.HostName;
            }
            else
            {
                session.HostName = session.IpAddress;
                await SendIrcMessageAsync(
                    signalEvent.Id,
                    NoticeAuthCommand.Create(ServerData.Hostname, "*** Could not resolve your hostname")
                );
            }
        }
        catch (SocketException)
        {
            await SendIrcMessageAsync(
                signalEvent.Id,
                NoticeAuthCommand.Create(ServerData.Hostname, "*** Could not resolve your hostname")
            );
        }
    }

    public Task OnEventAsync(SessionRemovedEvent signalEvent)
    {
        return Task.CompletedTask;
    }
}
