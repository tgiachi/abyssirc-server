
namespace AbyssIrc.Signals.Interfaces.Listeners;

public interface IAbyssIrcSignalListener<in TEvent>
    where TEvent : class
{
    Task OnEventAsync(TEvent signalEvent);
}
