using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC KILL command used by operators to disconnect clients
///     Format: "KILL nickname :reason" or ":source KILL nickname :reason"
/// </summary>
public class KillCommand : BaseIrcCommand
{
    public KillCommand() : base("KILL")
    {
    }

    /// <summary>
    ///     The source of the KILL command (server or operator nickname)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The nickname of the target to be killed
    /// </summary>
    public string TargetNickname { get; set; }

    /// <summary>
    ///     The reason for the kill
    /// </summary>
    public string Reason { get; set; }

    public override void Parse(string line)
    {
        // Handle source prefix if present
        var parseLine = line;
        if (line.StartsWith(':'))
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
            {
                return; // Invalid format
            }

            Source = line.Substring(1, spaceIndex - 1);
            parseLine = line.Substring(spaceIndex + 1).TrimStart();
        }

        // Split into parts
        var parts = parseLine.Split(' ', 3);

        // First token should be "KILL"
        if (parts.Length == 0 || parts[0].ToUpper() != "KILL")
        {
            return;
        }

        // Extract target nickname
        if (parts.Length > 1)
        {
            TargetNickname = parts[1];
        }

        // Extract reason if present
        if (parts.Length > 2)
        {
            Reason = parts[2].StartsWith(':') ? parts[2].Substring(1) : parts[2];
        }
    }

    public override string Write()
    {
        if (!string.IsNullOrEmpty(Source))
        {
            return string.IsNullOrEmpty(Reason)
                ? $":{Source} KILL {TargetNickname}"
                : $":{Source} KILL {TargetNickname} :{Reason}";
        }

        return string.IsNullOrEmpty(Reason)
            ? $"KILL {TargetNickname}"
            : $"KILL {TargetNickname} :{Reason}";
    }

    /// <summary>
    ///     Creates a KILL command for disconnecting a client
    /// </summary>
    /// <param name="targetNickname">Nickname of the client to disconnect</param>
    /// <param name="reason">Reason for the kill</param>
    /// <returns>A properly formatted KILL command</returns>
    public static KillCommand Create(string targetNickname, string reason)
    {
        return new KillCommand
        {
            TargetNickname = targetNickname,
            Reason = reason
        };
    }

    /// <summary>
    ///     Creates a KILL command with a source (for server or operator initiated kills)
    /// </summary>
    /// <param name="source">Source of the kill (server or operator)</param>
    /// <param name="targetNickname">Nickname of the client to disconnect</param>
    /// <param name="reason">Reason for the kill</param>
    /// <returns>A properly formatted KILL command with source</returns>
    public static KillCommand CreateWithSource(string source, string targetNickname, string reason)
    {
        return new KillCommand
        {
            Source = source,
            TargetNickname = targetNickname,
            Reason = reason
        };
    }
}
