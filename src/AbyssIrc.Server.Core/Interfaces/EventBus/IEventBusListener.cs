namespace AbyssIrc.Server.Core.Interfaces.EventBus;

public interface IEventBusListener<in TEvent>
{
    /// <summary>
    /// Handles the received event.
    /// </summary>
    /// <param name="event">The event to handle.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
