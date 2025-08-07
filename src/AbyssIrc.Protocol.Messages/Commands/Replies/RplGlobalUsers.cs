using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Replies;

/// <summary>
///     Represents the RPL_GLOBALUSERS (266) numeric reply
///     Provides information about global user count across the network
/// </summary>
public class RplGlobalUsers : BaseIrcCommand
{
    public RplGlobalUsers() : base("266")
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
    ///     Current number of global users
    /// </summary>
    public int CurrentGlobalUsers { get; set; }

    /// <summary>
    ///     Maximum number of global users ever recorded
    /// </summary>
    public int MaxGlobalUsers { get; set; }

    /// <summary>
    ///     Optional informative message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     Parses the RPL_GLOBALUSERS numeric reply
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 266 nickname :Current global users 142, max 200
        // Or: :server.com 266 nickname 142 200 :Current global users 142, max 200

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
            CurrentGlobalUsers = currentUsers;

            // Check if max users is also a number
            if (parts.Length > 4 && int.TryParse(parts[4], out var maxUsers))
            {
                MaxGlobalUsers = maxUsers;
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
    /// <returns>Formatted RPL_GLOBALUSERS message</returns>
    public override string Write()
    {
        // Format with server name
        if (!string.IsNullOrEmpty(ServerName))
        {
            return string.IsNullOrEmpty(Message)
                ? $":{ServerName} 266 {Nickname} {CurrentGlobalUsers} {MaxGlobalUsers}"
                : $":{ServerName} 266 {Nickname} {CurrentGlobalUsers} {MaxGlobalUsers} :{Message}";
        }

        // Format without server name
        return string.IsNullOrEmpty(Message)
            ? $"266 {Nickname} {CurrentGlobalUsers} {MaxGlobalUsers}"
            : $"266 {Nickname} {CurrentGlobalUsers} {MaxGlobalUsers} :{Message}";
    }

    /// <summary>
    ///     Creates a RPL_GLOBALUSERS reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="currentGlobalUsers">Current number of global users</param>
    /// <param name="maxGlobalUsers">Maximum number of global users</param>
    /// <param name="message">Optional informative message</param>
    public static RplGlobalUsers Create(
        string serverName,
        string nickname,
        int currentGlobalUsers,
        int maxGlobalUsers,
        string message = null
    )
    {
        return new RplGlobalUsers
        {
            ServerName = serverName,
            Nickname = nickname,
            CurrentGlobalUsers = currentGlobalUsers,
            MaxGlobalUsers = maxGlobalUsers,
            Message = message ?? $"Current global users {currentGlobalUsers}, max {maxGlobalUsers}"
        };
    }
}
