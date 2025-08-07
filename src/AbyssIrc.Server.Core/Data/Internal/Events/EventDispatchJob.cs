using AbyssIrc.Server.Core.Interfaces.EventBus;
using Serilog;

namespace AbyssIrc.Server.Core.Data.Internal.Events;

public abstract class EventDispatchJob
{
    public abstract Task ExecuteAsync();
}

/// <summary>
/// Generic implementation of event dispatch job
/// </summary>
public class EventDispatchJob<TEvent> : EventDispatchJob
    where TEvent : class
{
    private readonly IEventBusListener<TEvent> _listener;
    private readonly TEvent _event;

    private readonly ILogger _logger = Log.ForContext<EventDispatchJob<TEvent>>();

    public EventDispatchJob(IEventBusListener<TEvent> listener, TEvent @event)
    {
        _listener = listener;
        _event = @event;
    }

    public override async Task ExecuteAsync()
    {
        try
        {

            await _listener.HandleAsync(_event);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error executing event dispatch job for event type {EventType}", typeof(TEvent).Name);
            throw;
        }

    }
}
