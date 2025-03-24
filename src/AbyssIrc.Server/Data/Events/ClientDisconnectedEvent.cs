namespace AbyssIrc.Server.Data.Events;

public record ClientDisconnectedEvent(string Id, string Endpoint);
