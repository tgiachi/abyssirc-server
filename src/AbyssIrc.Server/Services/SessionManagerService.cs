using System.Collections.Concurrent;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Data.Events.Sessions;
using AbyssIrc.Server.Data.Internal;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Services;

public class SessionManagerService
    : ISessionManagerService, IAbyssSignalListener<ClientConnectedEvent>, IAbyssSignalListener<ClientDisconnectedEvent>
{
    private readonly ILogger _logger = Log.ForContext<SessionManagerService>();

    private readonly ConcurrentDictionary<string, IrcSession> _sessions = new();

    private readonly IAbyssSignalService _signalService;

    public SessionManagerService(IAbyssSignalService signalService)
    {
        _signalService = signalService;
        _signalService.Subscribe<ClientConnectedEvent>(this);
        _signalService.Subscribe<ClientDisconnectedEvent>(this);
    }

    public async Task OnEventAsync(ClientConnectedEvent signalEvent)
    {
        AddSession(signalEvent);
    }

    private void AddSession(ClientConnectedEvent @event)
    {
        AddSession(@event.Id, @event.Endpoint);
        _signalService.PublishAsync(new SessionAddedEvent(@event.Id));
    }

    public async Task RemoveSessionAsync(string sessionId)
    {
        if (_sessions.TryRemove(sessionId, out var session))
        {
            _logger.Information("Removing session {SessionId}", sessionId);

            await _signalService.PublishAsync(new SessionRemovedEvent(sessionId));
        }
    }

    public Task OnEventAsync(ClientDisconnectedEvent signalEvent)
    {
        return RemoveSessionAsync(signalEvent.Id);
    }

    public void AddSession(string id, string ipEndPoint, IrcSession? session = null)
    {
        _logger.Information("Adding session {SessionId}", id);

        var ipAddress = ipEndPoint.Split(':').First();
        var port = ipEndPoint.Split(':').Last();

        _sessions.TryAdd(
            id,
            new IrcSession()
            {
                Id = id,
                IpAddress = ipAddress,
                Port = int.Parse(port),
                LastPing = DateTime.Now,
            }
        );
    }

    public IrcSession GetSession(string id)
    {
        return _sessions.GetValueOrDefault(id);
    }
}
