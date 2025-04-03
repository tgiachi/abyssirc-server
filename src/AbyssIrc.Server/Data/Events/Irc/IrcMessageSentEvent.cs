using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Server.Data.Events.Irc;

public class IrcMessageSentEvent(string Id, IIrcCommand Command);
