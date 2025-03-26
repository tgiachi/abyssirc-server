using AbyssIrc.Network.Interfaces.Commands;

namespace AbyssIrc.Server.Data.Events.Irc;

public class IrcMessageSentEvent(string Id, IIrcCommand Command);
