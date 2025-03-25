using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Data.Events;
using AbyssIrc.Server.Data.Events.Irc;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Listeners.Base;

public abstract class BaseHandler
{
    protected ILogger Logger { get; } = Log.ForContext<BaseHandler>();

    private readonly IAbyssIrcSignalEmitterService _signalEmitterService;

    protected BaseHandler(IAbyssIrcSignalEmitterService signalEmitterService)
    {
        _signalEmitterService = signalEmitterService;
    }

    protected Task SendMessageAsync(string id, IIrcCommand message)
    {
        return _signalEmitterService.PublishAsync(new SendIrcMessageEvent(id, message));
    }


    protected Task SendSignalAsync<T>(T signal) where T : class
    {
        return _signalEmitterService.PublishAsync(signal);
    }

}
