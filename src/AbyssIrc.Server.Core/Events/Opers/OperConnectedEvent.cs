namespace AbyssIrc.Server.Core.Events.Opers;

public record OperConnectedEvent(string Usermask, string SessionId);
