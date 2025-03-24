
using AbyssIrc.Signals.Interfaces.Listeners;

namespace AbyssIrc.Signals.Interfaces.Services;

public interface IAbyssIrcSignalEmitterService : IDisposable
{
    void Subscribe<TEvent>(IAbyssIrcSignalListener<TEvent> listener)
        where TEvent : class;

    void Unsubscribe<TEvent>(IAbyssIrcSignalListener<TEvent> listener)
        where TEvent : class;

    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : class;
}
