namespace AbyssIrc.Server.Data.Events.Irc;

public record PrivMsgEvent(string Source, string Target, string Message);
