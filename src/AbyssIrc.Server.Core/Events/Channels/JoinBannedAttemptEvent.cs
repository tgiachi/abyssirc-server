namespace AbyssIrc.Server.Core.Events.Channels;

public record JoinBannedAttemptEvent(string Nickname, string Channel);
