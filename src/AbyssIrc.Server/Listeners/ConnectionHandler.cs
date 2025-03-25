using System.Net;
using System.Net.Sockets;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Listeners;

public class ConnectionHandler
    : BaseHandler, IAbyssIrcSignalListener<SessionAddedEvent>, IAbyssIrcSignalListener<SessionRemovedEvent>
{
    private readonly AbyssIrcConfig _config;
    private readonly ISessionManagerService _sessionManagerService;

    public ConnectionHandler(
        IAbyssIrcSignalEmitterService signalEmitterService, AbyssIrcConfig config,
        ISessionManagerService sessionManagerService
    ) : base(signalEmitterService)
    {
        _config = config;
        _sessionManagerService = sessionManagerService;
        signalEmitterService.Subscribe<SessionAddedEvent>(this);
        signalEmitterService.Subscribe<SessionRemovedEvent>(this);
    }

    public async Task OnEventAsync(SessionAddedEvent signalEvent)
    {
        var session = _sessionManagerService.GetSession(signalEvent.Id);
        await SendMessageAsync(
            signalEvent.Id,
            NoticeAuthCommand.Create(_config.Network.Host, "*** Looking up your hostname...")
        );

        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(session.IpAddress);

            if (hostEntry != null && !string.IsNullOrEmpty(hostEntry.HostName))
            {
                await Task.Delay(3000);
                await SendMessageAsync(
                    signalEvent.Id,
                    NoticeAuthCommand.Create(_config.Network.Host, $"*** Found your hostname: {hostEntry.HostName}")
                );

                session.HostName = hostEntry.HostName;
            }
            else
            {
                await SendMessageAsync(
                    signalEvent.Id,
                    NoticeAuthCommand.Create(_config.Network.Host, "*** Could not resolve your hostname")
                );
            }
        }
        catch (SocketException)
        {
            await SendMessageAsync(
                signalEvent.Id,
                NoticeAuthCommand.Create(_config.Network.Host, "*** Could not resolve your hostname")
            );
        }
    }

    public Task OnEventAsync(SessionRemovedEvent signalEvent)
    {
        return Task.CompletedTask;
    }
}
