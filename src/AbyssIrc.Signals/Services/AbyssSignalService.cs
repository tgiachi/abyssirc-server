using System;
using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using AbyssIrc.Signals.Data.Configs;
using AbyssIrc.Signals.Data.Internal;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Signals.Services;

public class AbyssSignalService : IAbyssSignalService
{
    private readonly ILogger _logger = Log.ForContext<AbyssSignalService>();

    private readonly ConcurrentDictionary<Type, object> _listeners = new();
    private readonly ActionBlock<EventDispatchJob> _dispatchBlock;
    private readonly CancellationTokenSource _cts = new();


    private readonly Subject<object> _allEventsSubject = new Subject<object>();

    /// <summary>
    /// Observable  that emits all events
    /// </summary>
    public IObservable<object> AllEventsObservable => _allEventsSubject;

    public AbyssSignalService(AbyssIrcSignalConfig config)
    {
        var executionOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = config.DispatchTasks,
            CancellationToken = _cts.Token
        };

        _dispatchBlock = new ActionBlock<EventDispatchJob>(
            job => job.ExecuteAsync(),
            executionOptions
        );

        _logger.Information(
            "Signal emitter initialized with {ParallelTasks} dispatch tasks",
            config.DispatchTasks
        );
    }

    /// <summary>
    /// Register a listener for a specific event type
    /// </summary>
    public void Subscribe<TEvent>(IAbyssSignalListener<TEvent> listener)
        where TEvent : class
    {
        var eventType = typeof(TEvent);

        // Get or create a list of listeners for this event type
        var listeners = (ConcurrentBag<IAbyssSignalListener<TEvent>>)_listeners.GetOrAdd(
            eventType,
            _ => new ConcurrentBag<IAbyssSignalListener<TEvent>>()
        );

        listeners.Add(listener);

        _logger.Verbose(
            "Registered listener {ListenerType} for event {EventType}",
            listener.GetType().Name,
            eventType.Name
        );
    }

    /// <summary>
    /// Register a function as a listener for a specific event type
    /// </summary>
    public void Subscribe<TEvent>(Func<TEvent, Task> handler)
        where TEvent : class
    {
        // Create a function wrapper that implements IAbyssSignalListener<TEvent>
        var listener = new FunctionSignalListener<TEvent>(handler);
        Subscribe<TEvent>(listener);

        _logger.Verbose(
            "Registered function handler for event {EventType}",
            typeof(TEvent).Name
        );
    }

    /// <summary>
    /// Unregisters a listener for a specific event type
    /// </summary>
    public void Unsubscribe<TEvent>(IAbyssSignalListener<TEvent> listener)
        where TEvent : class
    {
        var eventType = typeof(TEvent);

        if (_listeners.TryGetValue(eventType, out var listenersObj))
        {
            var listeners = (ConcurrentBag<IAbyssSignalListener<TEvent>>)listenersObj;

            // Create a new bag without the listener
            var updatedListeners = new ConcurrentBag<IAbyssSignalListener<TEvent>>(
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
    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler)
        where TEvent : class
    {
        var eventType = typeof(TEvent);

        if (_listeners.TryGetValue(eventType, out var listenersObj))
        {
            var listeners = (ConcurrentBag<IAbyssSignalListener<TEvent>>)listenersObj;

            // Create a new bag without the function handler
            var updatedListeners = new ConcurrentBag<IAbyssSignalListener<TEvent>>(
                listeners.Where(
                    l => !(l is FunctionSignalListener<TEvent> functionListener) ||
                         !functionListener.HasSameHandler(handler)
                )
            );

            _listeners.TryUpdate(eventType, updatedListeners, listeners);

            _logger.Verbose(
                "Unregistered function handler for event {EventType}",
                eventType.Name
            );
        }
    }

    /// <summary>
    /// Emits an event to all registered listeners asynchronously
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

        var listeners = (ConcurrentBag<IAbyssSignalListener<TEvent>>)listenersObj;

        _logger.Verbose(
            "Emitting event {EventType} to {ListenerCount} listeners",
            eventType.Name,
            listeners.Count
        );

        // Dispatch jobs to process the event for each listener
        foreach (var listener in listeners)
        {
            var job = new EventDispatchJob<TEvent>(listener, eventData);
            await _dispatchBlock.SendAsync(job, cancellationToken);
        }
    }

    /// <summary>
    /// Waits for all queued events to be processed
    /// </summary>
    public async Task WaitForCompletionAsync()
    {
        _dispatchBlock.Complete();
        await _dispatchBlock.Completion;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

/// <summary>
/// Adapter class that wraps a function to implement IAbyssSignalListener
/// </summary>
internal class FunctionSignalListener<TEvent> : IAbyssSignalListener<TEvent>
    where TEvent : class
{
    private readonly Func<TEvent, Task> _handler;

    public FunctionSignalListener(Func<TEvent, Task> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public Task OnEventAsync(TEvent signalEvent)
    {
        return _handler(signalEvent);
    }

    /// <summary>
    /// Checks if this wrapper contains the same handler function
    /// </summary>
    public bool HasSameHandler(Func<TEvent, Task> handler)
    {
        return _handler.Equals(handler);
    }
}
