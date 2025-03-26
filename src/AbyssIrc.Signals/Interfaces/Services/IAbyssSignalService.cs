
using AbyssIrc.Signals.Interfaces.Listeners;

namespace AbyssIrc.Signals.Interfaces.Services;

public interface IAbyssSignalService : IDisposable
{

    IObservable<object> AllEventsObservable { get; }

    void Subscribe<TEvent>(IAbyssSignalListener<TEvent> listener)
        where TEvent : class;

    void Subscribe<TEvent>(Func<TEvent, Task> handler)
        where TEvent : class;

    void Unsubscribe<TEvent>(IAbyssSignalListener<TEvent> listener)
        where TEvent : class;

    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class;
}
