namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface IEventDispatcherService
{
    void SubscribeToEvent(string eventName, Action<object?> eventHandler);
    void UnsubscribeFromEvent(string eventName, Action<object?> eventHandler);
}
