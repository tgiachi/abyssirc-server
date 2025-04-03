using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands.Errors;

/// <summary>
///     Represents ERR_TOOMANYTARGETS (407) numeric reply
/// </summary>
public class ErrTooManyTargets : BaseIrcCommand
{
    public ErrTooManyTargets() : base("407")
    {
    }

    /// <summary>
    ///     The nickname of the client receiving this reply
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     The server name sending this reply
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target that caused the error
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    ///     Optional numeric indicating the target limit
    /// </summary>
    public int? TargetLimit { get; set; }

    public override void Parse(string line)
    {
        // Example: :server.com 407 nickname target :Too many recipients. Message not delivered
        var parts = line.Split(' ', 4);

        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');
        // parts[1] should be "407"
        Nickname = parts[2];
        Target = parts[3].Split(' ')[0];
    }

    public override string Write()
    {
        var limitText = TargetLimit.HasValue ? $". Target limit is {TargetLimit}" : "";
        return $":{ServerName} {Code} {Nickname} {Target} :Too many recipients{limitText}. Message not delivered";
    }

    /// <summary>
    ///     Creates an ERR_TOOMANYTARGETS reply
    /// </summary>
    public static ErrTooManyTargets Create(string serverName, string nickname, string target, int? targetLimit = null)
    {
        return new ErrTooManyTargets
        {
            ServerName = serverName,
            Nickname = nickname,
            Target = target,
            TargetLimit = targetLimit
        };
    }
}
