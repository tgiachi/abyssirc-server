using System.Collections.Concurrent;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Internal;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Services;

public class SessionManagerService
    : ISessionManagerService, IAbyssIrcSignalListener<ClientConnectedEvent>, IAbyssIrcSignalListener<ClientDisconnectedEvent>
{
    private readonly ILogger _logger = Log.ForContext<SessionManagerService>();

    private readonly ConcurrentDictionary<string, IrcSession> _sessions = new();

    private readonly IAbyssIrcSignalEmitterService _signalEmitterService;

    public SessionManagerService(IAbyssIrcSignalEmitterService signalEmitterService)
    {
        _signalEmitterService = signalEmitterService;
        _signalEmitterService.Subscribe<ClientConnectedEvent>(this);
        _signalEmitterService.Subscribe<ClientDisconnectedEvent>(this);
    }

    public async Task OnEventAsync(ClientConnectedEvent signalEvent)
    {
        AddSession(signalEvent);
        await _signalEmitterService.PublishAsync(new SessionAddedEvent(signalEvent.Id));
    }

    private void AddSession(ClientConnectedEvent @event)
    {
        _logger.Information("Adding session {SessionId}", @event.Id);

        var ipAddress = @event.Endpoint.Split(':').First();
        var port = @event.Endpoint.Split(':').Last();

        _sessions.TryAdd(
            @event.Id,
            new IrcSession()
            {
                Id = @event.Id,
                IpAddress = ipAddress,
                Port = int.Parse(port)
            }
        );
    }

    public async Task RemoveSessionAsync(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            _logger.Information("Removing session {SessionId}", sessionId);

            await _signalEmitterService.PublishAsync(new SessionRemovedEvent(sessionId));
        }
    }

    public Task OnEventAsync(ClientDisconnectedEvent signalEvent)
    {
        return RemoveSessionAsync(signalEvent.Id);
    }

    public IrcSession GetSession(string id)
    {
        return _sessions.GetValueOrDefault(id);
    }
}
