using System.Text;
using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

/// <summary>
///     Represents an IRC CAP command for capability negotiation
///     Allows IRC clients and servers to negotiate new features in a backwards-compatible way
/// </summary>
public class CapCommand : BaseIrcCommand
{
    /// <summary>
    ///     Defines the subcommands that can be used with CAP
    /// </summary>
    public enum CapSubcommand
    {
        /// <summary>
        ///     Unknown or invalid subcommand
        /// </summary>
        Unknown,

        /// <summary>
        ///     List available capabilities
        /// </summary>
        LS,

        /// <summary>
        ///     List currently enabled capabilities
        /// </summary>
        LIST,

        /// <summary>
        ///     Request capability changes
        /// </summary>
        REQ,

        /// <summary>
        ///     Acknowledge capability changes
        /// </summary>
        ACK,

        /// <summary>
        ///     Reject capability changes
        /// </summary>
        NAK,

        /// <summary>
        ///     End capability negotiation
        /// </summary>
        END,

        /// <summary>
        ///     Server advertising new capabilities
        /// </summary>
        NEW,

        /// <summary>
        ///     Server removing previously advertised capabilities
        /// </summary>
        DEL
    }

    public CapCommand() : base("CAP")
    {
    }

    /// <summary>
    ///     The source/prefix of the command (when sent by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The nickname or * for the target of the CAP message (when sent by server)
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    ///     The CAP subcommand being used
    /// </summary>
    public CapSubcommand Subcommand { get; set; }

    /// <summary>
    ///     Capabilities list for the subcommand
    /// </summary>
    public List<CapabilityToken> Capabilities { get; } = new();

    /// <summary>
    ///     For LS subcommand: the version number requested by the client
    /// </summary>
    public int? LsVersion { get; set; }

    /// <summary>
    ///     For LS and LIST: flag indicating this is a multi-line response and more lines will follow
    /// </summary>
    public bool HasMoreLines { get; set; }

    public override void Parse(string line)
    {
        // Three main formats:
        // From client: CAP <subcommand> [<parameters>...]
        // From server: CAP <nick> <subcommand> [<parameters>...]
        // With multiline: CAP <nick> <subcommand> * [<parameters>...]

        if (line.StartsWith(':'))
        {
            // Server-sent format with source
            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex == -1)
            {
                return; // Invalid format
            }

            Source = line.Substring(1, spaceIndex - 1);
            line = line.Substring(spaceIndex + 1).TrimStart();
        }

        // Split into tokens
        string[] tokens = line.Split(' ');

        if (tokens.Length < 1)
        {
            return; // Invalid format
        }

        // First token should be "CAP"
        if (tokens[0].ToUpper() != "CAP")
        {
            return; // Not a CAP command
        }

        var tokenIndex = 1;

        // Determine if this is client or server format
        if (tokens.Length > tokenIndex && !IsSubcommand(tokens[tokenIndex]))
        {
            // Server format with target/nick
            Target = tokens[tokenIndex++];
        }

        // Parse the subcommand
        if (tokens.Length > tokenIndex)
        {
            Subcommand = ParseSubcommand(tokens[tokenIndex++]);

            // Handle LS version
            if (Subcommand == CapSubcommand.LS && tokens.Length > tokenIndex &&
                int.TryParse(tokens[tokenIndex], out var version))
            {
                LsVersion = version;
                tokenIndex++;
            }

            // Handle multi-line marker
            if ((Subcommand == CapSubcommand.LS || Subcommand == CapSubcommand.LIST) &&
                tokens.Length > tokenIndex && tokens[tokenIndex] == "*")
            {
                HasMoreLines = true;
                tokenIndex++;
            }

            // Parse capability list
            if (tokens.Length > tokenIndex)
            {
                var capabilitiesList = tokens[tokenIndex];

                // Check if the list is prefixed with ':'
                if (capabilitiesList.StartsWith(':'))
                {
                    capabilitiesList = capabilitiesList.Substring(1);

                    // Gather the remaining tokens if any (for parameters with spaces)
                    for (var i = tokenIndex + 1; i < tokens.Length; i++)
                    {
                        capabilitiesList += " " + tokens[i];
                    }
                }

                // Split and parse the capability tokens
                foreach (var capToken in capabilitiesList.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var capability = CapabilityToken.Parse(capToken);
                    if (capability != null)
                    {
                        Capabilities.Add(capability);
                    }
                }
            }
        }
    }

    public override string Write()
    {
        var sb = new StringBuilder();

        // Add source if provided
        if (!string.IsNullOrEmpty(Source))
        {
            sb.Append(':').Append(Source).Append(' ');
        }

        // Add the command
        sb.Append("CAP");

        // Add target if provided (for server responses)
        if (!string.IsNullOrEmpty(Target))
        {
            sb.Append(' ').Append(Target);
        }

        // Add subcommand
        sb.Append(' ').Append(Subcommand.ToString().ToUpper());

        // Add LS version if applicable
        if (Subcommand == CapSubcommand.LS && LsVersion.HasValue)
        {
            sb.Append(' ').Append(LsVersion.Value);
        }

        // Add multi-line marker if applicable
        if ((Subcommand == CapSubcommand.LS || Subcommand == CapSubcommand.LIST) && HasMoreLines)
        {
            sb.Append(" *");
        }

        // Add capability list if any
        if (Capabilities.Count > 0)
        {
            sb.Append(" :");
            sb.Append(string.Join(" ", Capabilities.Select(c => c.ToString())));
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Converts a string to the corresponding subcommand enum
    /// </summary>
    private CapSubcommand ParseSubcommand(string subcommand)
    {
        if (string.IsNullOrEmpty(subcommand))
        {
            return CapSubcommand.Unknown;
        }

        if (Enum.TryParse<CapSubcommand>(subcommand, true, out var result))
        {
            return result;
        }

        return CapSubcommand.Unknown;
    }

    /// <summary>
    ///     Checks if a string is a valid subcommand
    /// </summary>
    private bool IsSubcommand(string text)
    {
        return Enum.TryParse<CapSubcommand>(text, true, out _);
    }

    /// <summary>
    ///     Factory method to create a client CAP LS request
    /// </summary>
    /// <param name="version">Optional capability version</param>
    public static CapCommand CreateClientLs(int? version = null)
    {
        var cmd = new CapCommand
        {
            Subcommand = CapSubcommand.LS,
            LsVersion = version
        };

        return cmd;
    }

    /// <summary>
    ///     Factory method to create a server CAP LS response
    /// </summary>
    /// <param name="target">The client's nickname or *</param>
    /// <param name="capabilities">List of supported capabilities</param>
    /// <param name="hasMoreLines">Whether more capabilities will follow in subsequent messages</param>
    public static CapCommand CreateServerLs(
        string target, IEnumerable<CapabilityToken> capabilities, bool hasMoreLines = false
    )
    {
        var cmd = new CapCommand
        {
            Target = target,
            Subcommand = CapSubcommand.LS,
            HasMoreLines = hasMoreLines
        };

        cmd.Capabilities.AddRange(capabilities);

        return cmd;
    }

    /// <summary>
    ///     Factory method to create a client CAP LIST request
    /// </summary>
    public static CapCommand CreateClientList()
    {
        return new CapCommand { Subcommand = CapSubcommand.LIST };
    }

    /// <summary>
    ///     Factory method to create a server CAP LIST response
    /// </summary>
    /// <param name="target">The client's nickname</param>
    /// <param name="capabilities">List of enabled capabilities</param>
    /// <param name="hasMoreLines">Whether more capabilities will follow in subsequent messages</param>
    public static CapCommand CreateServerList(
        string target, IEnumerable<CapabilityToken> capabilities, bool hasMoreLines = false
    )
    {
        var cmd = new CapCommand
        {
            Target = target,
            Subcommand = CapSubcommand.LIST,
            HasMoreLines = hasMoreLines
        };

        cmd.Capabilities.AddRange(capabilities);

        return cmd;
    }

    /// <summary>
    ///     Factory method to create a client CAP REQ message
    /// </summary>
    /// <param name="capabilities">List of capabilities to request</param>
    public static CapCommand CreateClientReq(IEnumerable<CapabilityToken> capabilities)
    {
        var cmd = new CapCommand { Subcommand = CapSubcommand.REQ };
        cmd.Capabilities.AddRange(capabilities);

        return cmd;
    }

    /// <summary>
    ///     Factory method to create a server CAP ACK message
    /// </summary>
    /// <param name="target">The client's nickname</param>
    /// <param name="capabilities">List of acknowledged capabilities</param>
    public static CapCommand CreateServerAck(string target, IEnumerable<CapabilityToken> capabilities)
    {
        var cmd = new CapCommand
        {
            Target = target,
            Subcommand = CapSubcommand.ACK
        };

        cmd.Capabilities.AddRange(capabilities);

        return cmd;
    }

    /// <summary>
    ///     Factory method to create a server CAP NAK message
    /// </summary>
    /// <param name="target">The client's nickname</param>
    /// <param name="capabilities">List of rejected capabilities</param>
    public static CapCommand CreateServerNak(string target, IEnumerable<CapabilityToken> capabilities)
    {
        var cmd = new CapCommand
        {
            Target = target,
            Subcommand = CapSubcommand.NAK
        };

        cmd.Capabilities.AddRange(capabilities);

        return cmd;
    }

    /// <summary>
    ///     Factory method to create a client CAP END message
    /// </summary>
    public static CapCommand CreateClientEnd()
    {
        return new CapCommand { Subcommand = CapSubcommand.END };
    }

    /// <summary>
    ///     Factory method to create a server CAP NEW message
    /// </summary>
    /// <param name="target">The client's nickname</param>
    /// <param name="capabilities">List of new capabilities</param>
    public static CapCommand CreateServerNew(string target, IEnumerable<CapabilityToken> capabilities)
    {
        var cmd = new CapCommand
        {
            Target = target,
            Subcommand = CapSubcommand.NEW
        };

        cmd.Capabilities.AddRange(capabilities);

        return cmd;
    }

    /// <summary>
    ///     Factory method to create a server CAP DEL message
    /// </summary>
    /// <param name="target">The client's nickname</param>
    /// <param name="capabilities">List of removed capabilities</param>
    public static CapCommand CreateServerDel(string target, IEnumerable<CapabilityToken> capabilities)
    {
        var cmd = new CapCommand
        {
            Target = target,
            Subcommand = CapSubcommand.DEL
        };

        cmd.Capabilities.AddRange(capabilities);

        return cmd;
    }

    /// <summary>
    ///     Represents a capability token that can be enabled, disabled or advertised
    /// </summary>
    public class CapabilityToken
    {
        /// <summary>
        ///     Creates a new capability token
        /// </summary>
        /// <param name="name">The capability name</param>
        /// <param name="disable">Whether it should be disabled</param>
        /// <param name="value">Optional capability value</param>
        public CapabilityToken(string name, bool disable = false, string value = null)
        {
            Name = name;
            Disable = disable;
            Value = value;
        }

        /// <summary>
        ///     The name of the capability
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Whether the capability is being disabled (prefixed with -)
        /// </summary>
        public bool Disable { get; set; }

        /// <summary>
        ///     The value of the capability, if any (used in LS and NEW)
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Creates a new capability token from a token string
        /// </summary>
        /// <param name="token">The token string (e.g. "multi-prefix" or "-sasl" or "sasl=PLAIN,EXTERNAL")</param>
        /// <returns>A parsed capability token</returns>
        public static CapabilityToken Parse(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var disable = false;
            var name = token;
            string value = null;

            // Check if it's being disabled
            if (token.StartsWith('-'))
            {
                disable = true;
                name = token.Substring(1);
            }

            // Check if it has a value
            var valueIndex = name.IndexOf('=');
            if (valueIndex != -1)
            {
                value = name.Substring(valueIndex + 1);
                name = name.Substring(0, valueIndex);
            }

            return new CapabilityToken(name, disable, value);
        }

        /// <summary>
        ///     Converts the capability token to its string representation
        /// </summary>
        public override string ToString()
        {
            var result = Disable ? "-" : "";
            result += Name;

            if (!string.IsNullOrEmpty(Value))
            {
                result += "=" + Value;
            }

            return result;
        }
    }
}
