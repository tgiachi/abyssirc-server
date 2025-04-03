namespace AbyssIrc.Server.Core.Events.Commands;

public record QuitRequestEvent(string SessionId, string Reason);
