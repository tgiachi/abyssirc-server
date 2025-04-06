using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Protocol.Messages.Data.Internal;

public class IrcCommandDefinitionData
{
    public IIrcCommand Command { get; set; }

    public IrcCommandDefinitionData(IIrcCommand command)
    {
        Command = command;
    }

    public IrcCommandDefinitionData()
    {

    }
}
