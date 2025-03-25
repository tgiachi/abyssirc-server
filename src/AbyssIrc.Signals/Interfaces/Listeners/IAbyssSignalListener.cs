
namespace AbyssIrc.Signals.Interfaces.Listeners;

public interface IAbyssSignalListener<in TEvent>
    where TEvent : class
{
    Task OnEventAsync(TEvent signalEvent);
}
