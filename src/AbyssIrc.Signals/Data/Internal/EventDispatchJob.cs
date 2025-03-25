using AbyssIrc.Signals.Interfaces.Listeners;
using Serilog;

namespace AbyssIrc.Signals.Data.Internal;

internal abstract class EventDispatchJob
{
    public abstract Task ExecuteAsync();
}

/// <summary>
/// Generic implementation of event dispatch job
/// </summary>
internal class EventDispatchJob<TEvent> : EventDispatchJob
    where TEvent : class
{
    private readonly IAbyssSignalListener<TEvent> _listener;
    private readonly TEvent _event;
    private static readonly ILogger Logger = Log.ForContext<EventDispatchJob<TEvent>>();

    public EventDispatchJob(IAbyssSignalListener<TEvent> listener, TEvent @event)
    {
        _listener = listener;
        _event = @event;
    }

    public override async Task ExecuteAsync()
    {
        try
        {
            await _listener.OnEventAsync(_event);
        }
        catch (Exception ex)
        {
            Logger.Error(
                ex,
                "Error dispatching event {EventType} to listener {ListenerType}",
                typeof(TEvent).Name,
                _listener.GetType().Name
            );
        }
    }
}
