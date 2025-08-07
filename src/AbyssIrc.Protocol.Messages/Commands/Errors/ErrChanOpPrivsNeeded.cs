using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents an IRC ERR_CHANOPRIVSNEEDED (482) error response
///     Returned when a user tries to perform an operation that requires channel operator privileges
/// </summary>
public class ErrChanOpPrivsNeeded : BaseIrcCommand
{
    public ErrChanOpPrivsNeeded() : base("482") => ErrorMessage = "You're not a channel operator";

    /// <summary>
    ///     The server name/source of the error
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The name of the channel where the operation was attempted
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     The error message explaining the lack of operator privileges
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    ///     Optional description of the operation that was attempted
    /// </summary>
    public string OperationDescription { get; set; }

    public override void Parse(string line)
    {
        // ERR_CHANOPRIVSNEEDED format: ":server 482 nickname #channel :You're not a channel operator"
        // Possible variations might include more detailed error messages

        if (!line.StartsWith(':'))
        {
            return; // Invalid format for server response
        }

        var parts = line.Split(' ', 5); // Maximum of 5 parts

        if (parts.Length < 5)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "482"
        Nickname = parts[2];
        ChannelName = parts[3];

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
        // Format: ":server 482 nickname #channel :You're not a channel operator"
        return $":{ServerName} 482 {Nickname} {ChannelName} :{ErrorMessage}";
    }

    /// <summary>
    ///     Creates an ERR_CHANOPRIVSNEEDED (482) reply
    /// </summary>
    /// <param name="serverName">Name of the server sending the error</param>
    /// <param name="nickname">Nickname of the user receiving the error</param>
    /// <param name="channelName">Name of the channel where the operation was attempted</param>
    /// <param name="errorMessage">Custom error message (optional)</param>
    /// <param name="operationDescription">Description of the attempted operation (optional)</param>
    public static ErrChanOpPrivsNeeded Create(
        string serverName,
        string nickname,
        string channelName,
        string errorMessage = null,
        string operationDescription = null
    )
    {
        return new ErrChanOpPrivsNeeded
        {
            ServerName = serverName,
            Nickname = nickname,
            ChannelName = channelName,
            ErrorMessage = errorMessage ?? "You're not a channel operator",
            OperationDescription = operationDescription
        };
    }
}
