using AbyssIrc.Network.Interfaces.Commands;


namespace AbyssIrc.Network.Commands;

/// <summary>
///   Represents a command that has not been parsed
/// </summary>
public class NotParsedCommand : IIrcCommand
{
    private string _command;
    private string _arguments;

    public string Code => _command;

    public string Message => _arguments;

    public void Parse(string line)
    {
        _command = line.Split(' ')[0];
        _arguments = line.Substring(_command.Length).Trim();
    }

    public string Write()
    {
        return $"{_command} {_arguments}";
    }
}
