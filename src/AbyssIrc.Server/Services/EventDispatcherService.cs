using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Signals.Interfaces.Services;

namespace AbyssIrc.Server.Services;

public class EventDispatcherService : IEventDispatcherService
{
    private readonly Dictionary<string, List<Action<object?>>> _eventHandlers = new();

    public EventDispatcherService(IAbyssSignalService signalService)
    {
        signalService.AllEventsObservable.Subscribe(OnEvent);
    }

    private void OnEvent(object obj)
    {
        DispatchEvent(obj.GetType().Name.ToSnakeCase(), obj);
    }


    private void DispatchEvent(string eventName, object? eventData = null)
    {
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
