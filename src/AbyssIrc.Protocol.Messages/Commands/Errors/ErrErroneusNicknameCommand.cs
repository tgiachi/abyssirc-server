using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_ERRONEUSNICKNAME (432) error response
///     Returned when a client tries to use a nickname that contains invalid characters
/// </summary>
public class ErrErroneusNicknameCommand : BaseIrcCommand
{
    public ErrErroneusNicknameCommand() : base("432") => ErrorMessage = "Erroneous nickname";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname (usually * for unregistered connections)
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The invalid nickname that was attempted
    /// </summary>
    public string InvalidNick { get; set; }

    /// <summary>
    ///     The error message explaining the issue
    /// </summary>
    public string ErrorMessage { get; set; }

    public override void Parse(string line)
    {
        // ERR_ERRONEUSNICKNAME format: ":server 432 nickname invalid_nick :Erroneous nickname"

        if (!line.StartsWith(":"))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 5); // Maximum of 5 parts

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "432"
        Nickname = parts[2];
        InvalidNick = parts[3];

        // Extract the error message (removes the leading ":")
        if (parts[4].StartsWith(":"))
        {
            ErrorMessage = parts[4].Substring(1);
        }
        else
        {
            ErrorMessage = parts[4];
        }
    }

    public override string Write()
    {
        // Format: ":server 432 nickname invalid_nick :Erroneous nickname"
        return $":{ServerName} 432 {Nickname} {InvalidNick} :{ErrorMessage}";
    }
}
