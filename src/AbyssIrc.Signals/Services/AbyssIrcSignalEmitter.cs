using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using AbyssIrc.Signals.Data.Configs;
using AbyssIrc.Signals.Data.Internal;
using AbyssIrc.Signals.Interfaces.Listeners;
using AbyssIrc.Signals.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Signals.Services;

public class AbyssIrcSignalEmitter : IAbyssIrcSignalEmitterService
{
    private readonly ILogger _logger = Log.ForContext<AbyssIrcSignalEmitter>();

    private readonly ConcurrentDictionary<Type, object> _listeners = new();
    private readonly ActionBlock<EventDispatchJob> _dispatchBlock;
    private readonly CancellationTokenSource _cts = new();


    public AbyssIrcSignalEmitter(AbyssIrcSignalConfig config)
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
    public void Subscribe<TEvent>(IAbyssIrcSignalListener<TEvent> listener)
        where TEvent : class
    {
        var eventType = typeof(TEvent);

        // Get or create a list of listeners for this event type
        var listeners = (ConcurrentBag<IAbyssIrcSignalListener<TEvent>>)_listeners.GetOrAdd(
            eventType,
            _ => new ConcurrentBag<IAbyssIrcSignalListener<TEvent>>()
        );

        listeners.Add(listener);

        _logger.Debug(
            "Registered listener {ListenerType} for event {EventType}",
            listener.GetType().Name,
            eventType.Name
        );
    }

    /// <summary>
    /// Unregisters a listener for a specific event type
    /// </summary>
    public void Unsubscribe<TEvent>(IAbyssIrcSignalListener<TEvent> listener)
        where TEvent : class
    {
        var eventType = typeof(TEvent);

        if (_listeners.TryGetValue(eventType, out var listenersObj))
        {
            var listeners = (ConcurrentBag<IAbyssIrcSignalListener<TEvent>>)listenersObj;

            // Create a new bag without the listener
            var updatedListeners = new ConcurrentBag<IAbyssIrcSignalListener<TEvent>>(
                listeners.Where(l => !ReferenceEquals(l, listener))
            );

            _listeners.TryUpdate(eventType, updatedListeners, listeners);

            _logger.Debug(
                "Unregistered listener {ListenerType} from event {EventType}",
                listener.GetType().Name,
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

        if (!_listeners.TryGetValue(eventType, out var listenersObj))
        {
            _logger.Debug("No listeners registered for event {EventType}", eventType.Name);
            return;
        }

        var listeners = (ConcurrentBag<IAbyssIrcSignalListener<TEvent>>)listenersObj;

        _logger.Debug(
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
