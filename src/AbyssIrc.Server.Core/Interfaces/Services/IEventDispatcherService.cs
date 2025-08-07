using AbyssIrc.Core.Interfaces.Services;

namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface IEventDispatcherService : IAbyssService
{
    void SubscribeToEvent(string eventName, Action<object?> eventHandler);
    void UnsubscribeFromEvent(string eventName, Action<object?> eventHandler);
}
