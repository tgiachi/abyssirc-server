namespace AbyssIrc.Server.Core.Events.Server;

public record ServerSetTopicRequestEvent(string Channel, string Topic);
