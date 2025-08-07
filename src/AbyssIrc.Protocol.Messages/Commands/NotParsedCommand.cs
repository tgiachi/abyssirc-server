using AbyssIrc.Protocol.Messages.Interfaces.Commands;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents a command that has not been parsed
/// </summary>
public class NotParsedCommand : IIrcCommand
{
    public string Message { get; private set; }

    public string Code { get; private set; }

    public void Parse(string line)
    {
        Code = line.Split(' ')[0];
        Message = line.Substring(Code.Length).Trim();
    }

    public string Write()
    {
        return $"{Code} {Message}";
    }
}
