using System.Text;
using AbyssIrc.Network.Commands.Base;
using AbyssIrc.Network.Types;

namespace AbyssIrc.Network.Commands;

/// <summary>
///     Represents an IRC MODE command for setting or querying modes
/// </summary>
public class ModeCommand : BaseIrcCommand
{
    public ModeCommand() : base("MODE")
    {
    }

    /// <summary>
    ///     Source of the MODE command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Target of the mode command (channel or nickname)
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    ///     Type of the mode target
    /// </summary>
    public ModeTargetType TargetType { get; private set; }

    /// <summary>
    ///     List of mode changes
    /// </summary>
    public List<ModeChangeType> ModeChanges { get; set; } = new();

    /// <summary>
    ///     Determines the mode target type based on the first character
    /// </summary>
    /// <param name="target">Target of the mode command</param>
    /// <returns>Mode target type</returns>
    private ModeTargetType DetermineTargetType(string target)
    {
        // Channel prefixes as per RFC 1459
        char[] channelPrefixes = ['#', '&', '+', '!'];

        return channelPrefixes.Contains(target[0])
            ? ModeTargetType.Channel
            : ModeTargetType.User;
    }

    /// <summary>
    ///     Parses a MODE command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Source = null;
        Target = null;
        ModeChanges.Clear();

        // Check for source prefix
        if (line.StartsWith(':'))
        {
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                Source = line.Substring(1, spaceIndex - 1);
                line = line[(spaceIndex + 1)..].TrimStart();
            }
        }

        // Split remaining parts
        string[] parts = line.Split(' ');

        // First token should be "MODE"
        if (parts.Length == 0 || !parts[0].Equals("MODE", StringComparison.CurrentCultureIgnoreCase))
        {
            return;
        }

        // Ensure we have a target
        if (parts.Length < 2)
        {
            return;
        }

        // Set target and determine type
        Target = parts[1];
        TargetType = DetermineTargetType(Target);

        // If no mode changes specified, return
        if (parts.Length < 3)
        {
            return;
        }

        // Parse mode string
        var modeString = parts[2];

        // Prepare to track parameters
        var paramIndex = 3;
        var isAdding = true;

        // Parse mode string
        foreach (var c in modeString)
        {
            switch (c)
            {
                case '+':
                    isAdding = true;
                    break;
                case '-':
                    isAdding = false;
                    break;
                default:
                    var modeChange = new ModeChangeType
                    {
                        IsAdding = isAdding,
                        Mode = c
                    };

                    // Check if this mode requires a parameter
                    if (NeedsParameter(c))
                    {
                        // Ensure we have a parameter
                        if (paramIndex < parts.Length)
                        {
                            modeChange.Parameter = parts[paramIndex];
                            paramIndex++;
                        }
                    }

                    ModeChanges.Add(modeChange);
                    break;
            }
        }
    }

    /// <summary>
    ///     Determines if a mode requires a parameter
    /// </summary>
    /// <param name="mode">Mode character</param>
    /// <returns>True if the mode needs a parameter</returns>
    private bool NeedsParameter(char mode)
    {
        // This is a simplistic implementation
        // In a real-world scenario, this would depend on the specific server's mode definitions
        // For channels, modes like +b (ban), +o (op), +v (voice) need parameters
        // For users, mode changes typically don't need parameters
        return TargetType == ModeTargetType.Channel &&
               "bkloIv".IndexOf(mode) != -1;
    }

    /// <summary>
    ///     Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted MODE command string</returns>
    public override string Write()
    {
        // Prepare base command
        var commandBuilder = new StringBuilder();

        // Add source if present (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            commandBuilder.Append(':').Append(Source).Append(' ');
        }

        // Add MODE and target
        commandBuilder.Append("MODE ").Append(Target);

        // Prepare mode string and parameters
        var modeStringBuilder = new StringBuilder();
        var parameters = new List<string>();

        // Track current mode sign
        var currentSign = ' ';

        foreach (var change in ModeChanges)
        {
            // Add mode sign if changed
            if (change.IsAdding && currentSign != '+' ||
                !change.IsAdding && currentSign != '-')
            {
                modeStringBuilder.Append(change.IsAdding ? '+' : '-');
                currentSign = change.IsAdding ? '+' : '-';
            }

            // Add mode character
            modeStringBuilder.Append(change.Mode);

            // Add parameter if exists
            if (!string.IsNullOrEmpty(change.Parameter))
            {
                parameters.Add(change.Parameter);
            }
        }

        // Add mode string
        commandBuilder.Append(' ').Append(modeStringBuilder);

        // Add parameters if any
        if (parameters.Count != 0)
        {
            commandBuilder.Append(' ').Append(string.Join(" ", parameters));
        }

        return commandBuilder.ToString();
    }

    /// <summary>
    ///     Creates a MODE command to query modes
    /// </summary>
    /// <param name="target">Channel or nickname to query</param>
    public static ModeCommand Create(string target)
    {
        return new ModeCommand
        {
            Target = target,
            TargetType = target[0] == '#' || target[0] == '&'
                ? ModeTargetType.Channel
                : ModeTargetType.User
        };
    }

    /// <summary>
    ///     Creates a MODE command to set modes
    /// </summary>
    /// <param name="target">Channel or nickname to modify</param>
    /// <param name="modeChanges">Mode changes to apply</param>
    public static ModeCommand CreateWithModes(string target, params ModeChangeType[] modeChanges)
    {
        return new ModeCommand
        {
            Target = target,
            TargetType = target[0] == '#' || target[0] == '&'
                ? ModeTargetType.Channel
                : ModeTargetType.User,
            ModeChanges = modeChanges.ToList()
        };
    }
}
