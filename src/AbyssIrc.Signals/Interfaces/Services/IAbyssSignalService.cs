
using AbyssIrc.Signals.Interfaces.Listeners;

namespace AbyssIrc.Signals.Interfaces.Services;

public interface IAbyssSignalService : IDisposable
{
    void Subscribe<TEvent>(IAbyssSignalListener<TEvent> listener)
        where TEvent : class;

    void Unsubscribe<TEvent>(IAbyssSignalListener<TEvent> listener)
        where TEvent : class;

    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class;
}
