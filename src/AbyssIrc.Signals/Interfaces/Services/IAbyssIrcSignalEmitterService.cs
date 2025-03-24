using AbyssIrc.Signals.Interfaces.Events;
using AbyssIrc.Signals.Interfaces.Listeners;

namespace AbyssIrc.Signals.Interfaces.Services;

public interface IAbyssIrcSignalEmitterService : IDisposable
{
    void Subscribe<TEvent>(IAbyssIrcSignalListener<TEvent> listener)
        where TEvent : IAbyssIrcSignalEvent;

    void Unsubscribe<TEvent>(IAbyssIrcSignalListener<TEvent> listener)
        where TEvent : IAbyssIrcSignalEvent;

    Task PublishAsync<TEvent>(TEvent eventData, CancellationToken cancellationToken = default)
        where TEvent : IAbyssIrcSignalEvent;
}
