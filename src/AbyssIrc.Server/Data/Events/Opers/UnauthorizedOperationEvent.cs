namespace AbyssIrc.Server.Data.Events.Opers;

public record UnauthorizedOperationEvent(string Hostname, string Nickname, string Command);
