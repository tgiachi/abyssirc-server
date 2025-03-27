using System.Collections.Concurrent;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Data.Events.Sessions;
using AbyssIrc.Server.Data.Internal;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Services;

public class SessionManagerService
    : ISessionManagerService, IAbyssSignalListener<ClientConnectedEvent>, IAbyssSignalListener<ClientDisconnectedEvent>,
        IAbyssSignalListener<IrcMessageReceivedEvent>
{
    private readonly ILogger _logger = Log.ForContext<SessionManagerService>();

    private readonly ConcurrentDictionary<string, IrcSession> _sessions = new();

    private readonly IAbyssSignalService _signalService;

    public SessionManagerService(IAbyssSignalService signalService)
    {
        _signalService = signalService;
        _signalService.Subscribe<ClientConnectedEvent>(this);
        _signalService.Subscribe<ClientDisconnectedEvent>(this);
        _signalService.Subscribe<IrcMessageReceivedEvent>(this);
    }

    public async Task OnEventAsync(ClientConnectedEvent signalEvent)
    {
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

    public Task OnEventAsync(IrcMessageReceivedEvent signalEvent)
    {
        if (_sessions.ContainsKey(signalEvent.Id))
        {
            GetSession(signalEvent.Id).UpdateActivity();
            return Task.CompletedTask;
        }


        return Task.CompletedTask;
    }

    public void AddSession(string id, string ipEndPoint, IrcSession? session = null)
    {
        if (_sessions.ContainsKey(id))
        {
            _logger.Warning("Session {SessionId} already exists", id);
            return;
        }

        _logger.Information("Adding session {SessionId}", id);

        var ipAddress = ipEndPoint.Split(':').First();
        var port = ipEndPoint.Split(':').Last();

        _sessions.TryAdd(
            id,
            new IrcSession(id, ipAddress, int.Parse(port))
        );

        _signalService.PublishAsync(new SessionAddedEvent(id));
    }

    public IrcSession GetSession(string id)
    {
        return _sessions.GetValueOrDefault(id);
    }

    public List<IrcSession> GetSessions()
    {
        return _sessions.Values.ToList();
    }
}
