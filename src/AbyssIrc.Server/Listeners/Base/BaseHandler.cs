using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Server.Data.Events.TcpServer;
using AbyssIrc.Server.Data.Internal.Handlers;
using AbyssIrc.Server.Data.Internal.Sessions;
using AbyssIrc.Server.Interfaces.Services.System;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AbyssIrc.Server.Listeners.Base;

public abstract class BaseHandler
{
    protected ILogger Logger { get; }

    protected AbyssServerData ServerData { get; }

    protected AbyssIrcConfig ServerConfig { get; }

    private readonly IAbyssSignalService _signalService;

    private readonly ISessionManagerService _sessionManagerService;

    protected string Hostname => ServerData.Hostname;

    protected BaseHandler(
        ILogger<BaseHandler> logger, IServiceProvider serviceProvider
    )
    {
        Logger = logger;
        _signalService = serviceProvider.GetRequiredService<IAbyssSignalService>();
        _sessionManagerService = serviceProvider.GetRequiredService<ISessionManagerService>();
        ServerData = serviceProvider.GetRequiredService<AbyssServerData>();
        ServerConfig = serviceProvider.GetRequiredService<AbyssIrcConfig>();
    }

    protected Task SendIrcMessageAsync(string id, IIrcCommand message)
    {
        return _signalService.PublishAsync(new SendIrcMessageEvent(id, message));
    }


    protected void SubscribeSignal<TEvent>(IAbyssSignalListener<TEvent> listener)
        where TEvent : class
    {
        _signalService.Subscribe(listener);
    }


    protected Task SendSignalAsync<T>(T signal) where T : class
    {
        return _signalService.PublishAsync(signal);
    }


    protected List<IrcSession> GetSessions()
    {
        return _sessionManagerService.GetSessions();
    }

    /// <summary>
    ///   Get the session by the id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    protected IrcSession? GetSession(string id)
    {
        return _sessionManagerService.GetSession(id);
    }

    /// <summary>
    ///  Get the session by the query provided
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    protected IEnumerable<IrcSession> QuerySessions(Func<IrcSession, bool> query)
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
