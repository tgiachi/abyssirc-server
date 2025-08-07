using System.Collections;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Threading.Channels;
using AbyssIrc.Server.Core.Data.Internal.Events;
using AbyssIrc.Server.Core.Interfaces.EventBus;
using AbyssIrc.Server.Core.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Services;

public class EventBusService : IEventBusService, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<EventBusService>();
    private readonly ConcurrentDictionary<Type, object> _listeners = new();
    private readonly Channel<EventDispatchJob> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;
    private readonly Subject<object> _allEventsSubject = new();

    /// <summary>
    /// Observable that emits all events
    /// </summary>
    public IObservable<object> AllEventsObservable => _allEventsSubject;

    public EventBusService()
    {
        _channel = Channel.CreateUnbounded<EventDispatchJob>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            }
        );

        _processingTask = Task.Run(ProcessEventsAsync, _cts.Token);

        _logger.Information("EventBusService initialized with Channel");
    }

    /// <summary>
    /// Registers a listener for a specific event type
    /// </summary>
    public void Subscribe<TEvent>(IEventBusListener<TEvent> listener) where TEvent : class
    {
        var eventType = typeof(TEvent);
        var listeners = (ConcurrentBag<IEventBusListener<TEvent>>)_listeners.GetOrAdd(
            eventType,
            _ => new ConcurrentBag<IEventBusListener<TEvent>>()
        );

        listeners.Add(listener);

        _logger.Verbose(
            "Registered listener {ListenerType} for event {EventType}",
            listener.GetType().Name,
            eventType.Name
        );
    }

    /// <summary>
    /// Registers a function as a listener for a specific event type
    /// </summary>
    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var listener = new FunctionSignalListener<TEvent>(handler);
        Subscribe(listener);

        _logger.Verbose("Registered function handler for event {EventType}", typeof(TEvent).Name);
    }

    /// <summary>
    /// Unregisters a listener for a specific event type
    /// </summary>
    public void Unsubscribe<TEvent>(IEventBusListener<TEvent> listener) where TEvent : class
    {
        var eventType = typeof(TEvent);

        if (_listeners.TryGetValue(eventType, out var listenersObj))
        {
            var listeners = (ConcurrentBag<IEventBusListener<TEvent>>)listenersObj;
            var updatedListeners = new ConcurrentBag<IEventBusListener<TEvent>>(
                listeners.Where(l => !ReferenceEquals(l, listener))
            );

            _listeners.TryUpdate(eventType, updatedListeners, listeners);

            _logger.Verbose(
                "Unregistered listener {ListenerType} from event {EventType}",
                listener.GetType().Name,
                eventType.Name
            );
        }
    }

    /// <summary>
    /// Unregisters a function handler for a specific event type
    /// </summary>
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);

        if (_listeners.TryGetValue(eventType, out var listenersObj))
        {
            var listeners = (ConcurrentBag<IEventBusListener<TEvent>>)listenersObj;
            var updatedListeners = new ConcurrentBag<IEventBusListener<TEvent>>(
                listeners.Where(l => !(l is FunctionSignalListener<TEvent> functionListener) ||
                                     !functionListener.HasSameHandler(handler)
                )
            );

            _listeners.TryUpdate(eventType, updatedListeners, listeners);

            _logger.Verbose("Unregistered function handler for event {EventType}", eventType.Name);
        }
    }

    /// <summary>
    /// Publishes an event to all registered listeners asynchronously
    /// </summary>
    public async Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventType = typeof(TEvent);

        _allEventsSubject.OnNext(eventData);

        if (!_listeners.TryGetValue(eventType, out var listenersObj))
        {
            _logger.Verbose("No listeners registered for event {EventType}", eventType.Name);
            return;
        }

        var listeners = (ConcurrentBag<IEventBusListener<TEvent>>)listenersObj;

        _logger.Verbose(
            "Publishing event {EventType} to {ListenerCount} listeners",
            eventType.Name,
            listeners.Count
        );

        foreach (var listener in listeners)
        {
            var job = new EventDispatchJob<TEvent>(listener, eventData);
            await _channel.Writer.WriteAsync(job, cancellationToken);
        }
    }

    /// <summary>
    /// Returns total listener count
    /// </summary>
    public int GetListenerCount()
    {
        int total = 0;

        foreach (var kvp in _listeners)
        {
            if (kvp.Value is ICollection collection)
            {
                total += collection.Count;
            }
        }

        return total;
    }

    /// <summary>
    /// Returns listener count for a specific event type
    /// </summary>
    public int GetListenerCount<TEvent>() where TEvent : class
    {
        if (_listeners.TryGetValue(typeof(TEvent), out var listenersObj))
        {
            var listeners = (ConcurrentBag<IEventBusListener<TEvent>>)listenersObj;
            return listeners.Count;
        }

        return 0;
    }

    /// <summary>
    /// Waits for all pending events to be processed
    /// </summary>
    public async Task WaitForCompletionAsync()
    {
        _channel.Writer.Complete();
        await _processingTask;
    }

    /// <summary>
    /// Background processor for event dispatch jobs
    /// </summary>
    private async Task ProcessEventsAsync()
    {
        try
        {
            await foreach (var job in _channel.Reader.ReadAllAsync(_cts.Token))
            {
                try
                {
                    await job.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error while executing job {JobType}", job.GetType().Name);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Event processing was cancelled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error in event processing task");
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _channel.Writer.TryComplete();
        _processingTask.Wait();
        _cts.Dispose();
    }
}
