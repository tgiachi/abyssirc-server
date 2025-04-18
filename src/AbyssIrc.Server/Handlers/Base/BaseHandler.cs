using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Data.Configs;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Data.Events.TcpServer;
using AbyssIrc.Server.Data.Internal.Handlers;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AbyssIrc.Server.Handlers.Base;

public abstract class BaseHandler
{
    protected ILogger Logger { get; }

    protected AbyssServerData ServerData { get; }

    protected AbyssIrcConfig ServerConfig { get; }

    private readonly IAbyssSignalService _signalService;

    private readonly ISessionManagerService _sessionManagerService;

    private readonly IServiceProvider _serviceProvider;

    protected string Hostname => ServerData.Hostname;

    protected BaseHandler(
        ILogger<BaseHandler> logger, IServiceProvider serviceProvider
    )
    {
        Logger = logger;
        _serviceProvider = serviceProvider;
        _signalService = serviceProvider.GetRequiredService<IAbyssSignalService>();
        _sessionManagerService = serviceProvider.GetRequiredService<ISessionManagerService>();
        ServerData = serviceProvider.GetRequiredService<AbyssServerData>();
        ServerConfig = serviceProvider.GetRequiredService<AbyssIrcConfig>();
    }

    protected async Task SendIrcMessageAsync(string id, IIrcCommand message)
    {
        await _serviceProvider.GetRequiredService<ITcpService>().SendIrcMessagesAsync(id, message);
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
    ///  Get the session by the nickname
    /// </summary>
    /// <param name="nick"></param>
    /// <returns></returns>
    protected IrcSession? GetSessionByNickname(string nick)
    {
        return _sessionManagerService.GetSessionByNickname(nick);
    }

    /// <summary>
    ///   Get the session manager service
    /// </summary>
    /// <returns></returns>
    protected ISessionManagerService GetSessionManagerService()
    {
        return _sessionManagerService;
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
    protected Task DisconnectClientSession(string id, string nickName)
    {
        return _signalService.PublishAsync(new DisconnectedClientSessionEvent(id));
    }
}
