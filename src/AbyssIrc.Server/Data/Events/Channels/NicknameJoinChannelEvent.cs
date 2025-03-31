namespace AbyssIrc.Server.Data.Events.Channels;

public record NicknameJoinChannelEvent(string Nickname, string ChannelName);
