using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Core.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Services;

public class EventDispatcherService : IEventDispatcherService
{
    private readonly Dictionary<string, List<Action<object?>>> _eventHandlers = new();

    private readonly ILogger _logger = Log.ForContext<EventDispatcherService>();

    public EventDispatcherService(IEventBusService eventBusService)
    {
        eventBusService.AllEventsObservable.Subscribe(OnEvent);
    }

    private void OnEvent(object obj)
    {
        DispatchEvent(obj.GetType().Name.ToSnakeCase().Replace("_event", ""), obj);
    }


    private void DispatchEvent(string eventName, object? eventData = null)
    {
        _logger.Debug("Dispatching event {EventName}", eventName);
        if (!_eventHandlers.TryGetValue(eventName, out var eventHandler))
        {
            return;
        }

        foreach (var handler in eventHandler)
        {
            handler(eventData);
        }
    }

    public void SubscribeToEvent(string eventName, Action<object?> eventHandler)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var eventHandlers))
        {
            eventHandlers = new List<Action<object?>>();
            _eventHandlers.Add(eventName, eventHandlers);
        }

        eventHandlers.Add(eventHandler);
    }

    public void UnsubscribeFromEvent(string eventName, Action<object?> eventHandler)
    {
        if (!_eventHandlers.TryGetValue(eventName, out var eventHandlers))
        {
            return;
        }

        eventHandlers.Remove(eventHandler);
    }
}
