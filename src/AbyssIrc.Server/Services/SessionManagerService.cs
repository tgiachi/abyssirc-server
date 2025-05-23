using System.Collections.Concurrent;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Client;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Data.Events.Sessions;
using AbyssIrc.Server.Data.Internal;

using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;


namespace AbyssIrc.Server.Services;

public class SessionManagerService
    : ISessionManagerService, IAbyssSignalListener<ClientConnectedEvent>, IAbyssSignalListener<ClientDisconnectedEvent>,
        IAbyssSignalListener<IrcMessageReceivedEvent>
{
    public int MaxSessions { get; private set; }
    private readonly ILogger _logger;

    private readonly ConcurrentDictionary<string, IrcSession> _sessions = new();

    private readonly IAbyssSignalService _signalService;

    public SessionManagerService(ILogger<SessionManagerService> logger, IAbyssSignalService signalService)
    {
        _signalService = signalService;
        _logger = logger;
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
            _logger.LogInformation("Removing session {SessionId}", sessionId);

            await _signalService.PublishAsync(new SessionRemovedEvent(sessionId, session));
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
            _logger.LogWarning("Session {SessionId} already exists", id);
            return;
        }

        _logger.LogInformation("Adding session {SessionId}", id);

        var ipAddress = ipEndPoint.Split(':').First();
        var port = ipEndPoint.Split(':').Last();

        MaxSessions += 1;

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

    public IrcSession? GetSessionByNickname(string nickname)
    {
        return _sessions.Values.FirstOrDefault(session => session.Nickname == nickname);
    }

    public List<string> GetSessionIdsByNicknames(params string[] nicknames)
    {
        return nicknames.Select(GetSessionByNickname).OfType<IrcSession>().Select(session => session.Id).ToList();
    }
}
