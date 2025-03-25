using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Listeners.Base;

public abstract class BaseHandler
{
    protected ILogger Logger { get; } = Log.ForContext<BaseHandler>();

    private readonly IAbyssSignalService _signalService;

    protected BaseHandler(IAbyssSignalService signalService)
    {
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
