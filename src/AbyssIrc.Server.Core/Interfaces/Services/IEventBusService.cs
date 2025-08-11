using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Server.Core.Interfaces.EventBus;

namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface IEventBusService : IAbyssService
{
    /// <summary>
    ///  Observable that emits all events dispatched through the system.
    /// </summary>
    IObservable<object> AllEventsObservable { get; }


    /// <summary>
    ///  Registers a listener for a specific event type.
    /// </summary>
    /// <param name="listener"></param>
    /// <typeparam name="TEvent"></typeparam>
    void Subscribe<TEvent>(IEventBusListener<TEvent> listener)
        where TEvent : class;


    /// <summary>
    ///  Registers a listener for a specific event type.
    /// </summary>
    /// <param name="handler"></param>
    /// <typeparam name="TEvent"></typeparam>
    void Subscribe<TEvent>(Func<TEvent, Task> handler)
        where TEvent : class;

    /// <summary>
    ///  Unregisters a listener for a specific event type.
    /// </summary>
    /// <param name="listener"></param>
    /// <typeparam name="TEvent"></typeparam>
    void Unsubscribe<TEvent>(IEventBusListener<TEvent> listener)
        where TEvent : class;

    /// <summary>
    ///  Dispatches an event to all registered listeners asynchronously.
    /// </summary>
    /// <param name="eventData"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TEvent"></typeparam>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class;

    /// <summary>
    /// Gets the current number of registered listeners for all event types.
    /// </summary>
    /// <returns>The total number of registered listeners.</returns>
    int GetListenerCount();

    /// <summary>
    /// Gets the current number of registered listeners for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <returns>The number of registered listeners for the specified event type.</returns>
    int GetListenerCount<TEvent>() where TEvent : class;


    /// <summary>
    ///  Waits for all dispatched events to be processed.
    /// </summary>
    /// <returns></returns>
    public Task WaitForCompletionAsync();
}
