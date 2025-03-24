using AbyssIrc.Signals.Interfaces.Events;

namespace AbyssIrc.Signals.Interfaces.Listeners;

public interface IAbyssIrcSignalListener<in TEvent>
    where TEvent : IAbyssIrcSignalEvent
{
    Task OnEventAsync(TEvent signalEvent);
}
