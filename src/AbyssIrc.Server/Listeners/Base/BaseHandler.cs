using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Signals.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AbyssIrc.Server.Listeners.Base;

public abstract class BaseHandler
{
    protected ILogger Logger { get; }

    private readonly IAbyssSignalService _signalService;

    protected BaseHandler(ILogger<BaseHandler> logger, IAbyssSignalService signalService)
    {
        Logger = logger;
        _signalService = signalService;
    }

    protected Task SendMessageAsync(string id, IIrcCommand message)
    {
        return _signalService.PublishAsync(new SendIrcMessageEvent(id, message));
    }


    protected Task SendSignalAsync<T>(T signal) where T : class
    {
        return _signalService.PublishAsync(signal);
    }
}
