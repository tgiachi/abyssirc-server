using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Protocol.Messages.Commands.Base;

public abstract class BaseIrcCommand : IIrcCommand
{
    protected BaseIrcCommand(string code) => Code = code;

    public string Code { get; }

    public virtual void Parse(string line)
    {
    }

    public virtual string Write()
    {
        return string.Empty;
    }
}
