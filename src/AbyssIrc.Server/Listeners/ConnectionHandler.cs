using System.Net;
using System.Net.Sockets;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Data.Events.Sessions;
using AbyssIrc.Server.Interfaces.Listener;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Listeners.Base;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class ConnectionHandler
    : BaseHandler, IAbyssSignalListener<SessionAddedEvent>, IAbyssSignalListener<SessionRemovedEvent>
{
    private readonly AbyssIrcConfig _config;
    private readonly ISessionManagerService _sessionManagerService;

    public ConnectionHandler(
        ILogger<ConnectionHandler> logger,
        IAbyssSignalService signalService, AbyssIrcConfig config,
        ISessionManagerService sessionManagerService
    ) : base(logger, signalService)
    {
        _config = config;
        _sessionManagerService = sessionManagerService;
        signalService.Subscribe<SessionAddedEvent>(this);
        signalService.Subscribe<SessionRemovedEvent>(this);
    }

    public async Task OnEventAsync(SessionAddedEvent signalEvent)
    {
        var session = _sessionManagerService.GetSession(signalEvent.Id);
        await SendIrcMessageAsync(
            signalEvent.Id,
            NoticeAuthCommand.Create(_config.Network.Host, "*** Looking up your hostname...")
        );

        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(session.IpAddress);

            if (hostEntry != null && !string.IsNullOrEmpty(hostEntry.HostName))
            {
                await SendIrcMessageAsync(
                    signalEvent.Id,
                    NoticeAuthCommand.Create(_config.Network.Host, $"*** Found your hostname: {hostEntry.HostName}")
                );

                session.HostName = hostEntry.HostName;
            }
            else
            {
                session.HostName = session.IpAddress;
                await SendIrcMessageAsync(
                    signalEvent.Id,
                    NoticeAuthCommand.Create(_config.Network.Host, "*** Could not resolve your hostname")
                );
            }
        }
        catch (SocketException)
        {
            await SendIrcMessageAsync(
                signalEvent.Id,
                NoticeAuthCommand.Create(_config.Network.Host, "*** Could not resolve your hostname")
            );
        }
        finally
        {

        }
    }

    public Task OnEventAsync(SessionRemovedEvent signalEvent)
    {
        return Task.CompletedTask;
    }


}
