namespace AbyssIrc.Server.Data.Events.Client;

public record ClientDisconnectedEvent(string Id, string Endpoint);
