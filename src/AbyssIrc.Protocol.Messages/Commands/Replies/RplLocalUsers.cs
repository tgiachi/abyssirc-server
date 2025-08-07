using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents the RPL_LOCALUSERS (265) numeric reply
///     Provides information about local user count on the server
/// </summary>
public class RplLocalUsers : BaseIrcCommand
{
    public RplLocalUsers() : base("265")
    {
    }

    /// <summary>
    ///     The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     Current number of local users
    /// </summary>
    public int CurrentLocalUsers { get; set; }

    /// <summary>
    ///     Maximum number of local users ever recorded
    /// </summary>
    public int MaxLocalUsers { get; set; }

    /// <summary>
    ///     Optional informative message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     Parses the RPL_LOCALUSERS numeric reply
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 265 nickname :Current local users 42, max 50
        // Or: :server.com 265 nickname 42 50 :Current local users 42, max 50

        var parts = line.Split(' ');

        if (parts.Length < 4)
        {
            return;
        }

        // Extract server name if present
        if (line.StartsWith(':'))
        {
            ServerName = parts[0].TrimStart(':');
        }

        // Extract nickname
        Nickname = parts[2];

        // Try to parse current and max users
        if (int.TryParse(parts[3], out var currentUsers))
        {
            CurrentLocalUsers = currentUsers;

            // Check if max users is also a number
            if (parts.Length > 4 && int.TryParse(parts[4], out var maxUsers))
            {
                MaxLocalUsers = maxUsers;
            }
        }

        // Extract message if present
        var colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + parts[2].Length + parts[3].Length + 2);
        if (colonIndex != -1)
        {
            Message = line.Substring(colonIndex + 1);
        }
    }

    /// <summary>
    ///     Converts the reply to its string representation
    /// </summary>
    /// <returns>Formatted RPL_LOCALUSERS message</returns>
    public override string Write()
    {
        // Format with server name
        if (!string.IsNullOrEmpty(ServerName))
        {
            return string.IsNullOrEmpty(Message)
                ? $":{ServerName} 265 {Nickname} {CurrentLocalUsers} {MaxLocalUsers}"
                : $":{ServerName} 265 {Nickname} {CurrentLocalUsers} {MaxLocalUsers} :{Message}";
        }

        // Format without server name
        return string.IsNullOrEmpty(Message)
            ? $"265 {Nickname} {CurrentLocalUsers} {MaxLocalUsers}"
            : $"265 {Nickname} {CurrentLocalUsers} {MaxLocalUsers} :{Message}";
    }

    /// <summary>
    ///     Creates a RPL_LOCALUSERS reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="currentLocalUsers">Current number of local users</param>
    /// <param name="maxLocalUsers">Maximum number of local users</param>
    /// <param name="message">Optional informative message</param>
    public static RplLocalUsers Create(
        string serverName,
        string nickname,
        int currentLocalUsers,
        int maxLocalUsers,
        string message = null
    )
    {
        return new RplLocalUsers
        {
            ServerName = serverName,
            Nickname = nickname,
            CurrentLocalUsers = currentLocalUsers,
            MaxLocalUsers = maxLocalUsers,
            Message = message ?? $"Current local users {currentLocalUsers}, max {maxLocalUsers}"
        };
    }
}
