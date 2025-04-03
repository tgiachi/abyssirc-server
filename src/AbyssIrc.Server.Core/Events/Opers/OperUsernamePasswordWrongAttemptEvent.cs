namespace AbyssIrc.Server.Core.Events.Opers;

public record OperUsernamePasswordWrongAttemptEvent(string Usermask, string SessionId);
