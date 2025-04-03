namespace AbyssIrc.Server.Core.Events.Opers;

public record OperHostMismatchEvent(string Usermask, string SessionId);
