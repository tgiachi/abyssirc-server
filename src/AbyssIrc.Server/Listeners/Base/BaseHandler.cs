using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Data.Events.TcpServer;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AbyssIrc.Server.Listeners.Base;

public abstract class BaseHandler
{
    protected ILogger Logger { get; }

    private readonly IAbyssSignalService _signalService;

    private readonly ISessionManagerService _sessionManagerService;

    protected BaseHandler(
        ILogger<BaseHandler> logger, IAbyssSignalService signalService, ISessionManagerService sessionManagerService
    )
    {
        Logger = logger;
        _signalService = signalService;
        _sessionManagerService = sessionManagerService;
    }

    protected Task SendIrcMessageAsync(string id, IIrcCommand message)
    {
        return _signalService.PublishAsync(new SendIrcMessageEvent(id, message));
    }


    protected Task SendSignalAsync<T>(T signal) where T : class
    {
        return _signalService.PublishAsync(signal);
    }


    /// <summary>
    ///   Get the session by the id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected IrcSession GetSession(string id)
    {
        return _sessionManagerService.GetSession(id);
    }

    /// <summary>
    ///  Get the session by the query provided
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    protected IEnumerable<IrcSession> GetSessionQuery(Func<IrcSession, bool> query)
    {
        return _sessionManagerService.GetSessions().Where(query).ToList();
    }

    /// <summary>
    ///  Disconnect the client session
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected Task DisconnectClientSession(string id)
    {
        return _signalService.PublishAsync(new DisconnectedClientSessionEvent(id));
    }
}
