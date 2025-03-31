using System.Text;
using System.Text.RegularExpressions;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
///     Represents an IRC RPL_ISUPPORT (005) response
///     Used to advertise server features and their values to clients
///     Format: ":server 005 nickname param1 param2=value ... :are supported by this server"
///     Token format follows ABNF:
///     token      =  *1"-" parameter / parameter *1( "=" value )
///     parameter  =  1*20 letter
///     value      =  * letpun
///     letter     =  ALPHA / DIGIT
///     punct      =  %d33-47 / %d58-64 / %d91-96 / %d123-126
///     letpun     =  letter / punct
/// </summary>
public class RplISupport : BaseIrcCommand
{
    /// <summary>
    ///     Maximum number of tokens that can be included in a single RPL_ISUPPORT message
    /// </summary>
    public const int MaxTokensPerMessage = 13;

    public RplISupport() : base("005")
    {
    }

    /// <summary>
    ///     The server name/source of the support information
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    ///     The target user nickname
    /// </summary>
    public string Nickname { get; set; }

    /// <summary>
    ///     Collection of ISUPPORT parameters and their values
    ///     Parameters with a null value are boolean flags (PARAMETER)
    ///     Parameters with an empty string are value parameters with empty values (PARAMETER=)
    ///     Parameters with a string value are value parameters with values (PARAMETER=VALUE)
    /// </summary>
    public Dictionary<string, string> Parameters { get; } = new();

    /// <summary>
    ///     Collection of parameters that have been negated (-PARAMETER)
    /// </summary>
    public HashSet<string> NegatedParameters { get; } = new();

    /// <summary>
    ///     The trailing message at the end of the ISUPPORT reply
    /// </summary>
    public string Message { get; set; } = "are supported by this server";

    /// <summary>
    ///     Maximum away message length
    /// </summary>
    public int? AwayLen
    {
        get => Parameters.TryGetValue("AWAYLEN", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("AWAYLEN", value?.ToString());
    }

    /// <summary>
    ///     Case mapping method used by the server
    /// </summary>
    public string CaseMapping
    {
        get => Parameters.GetValueOrDefault("CASEMAPPING", "rfc1459");
        set => SetParameter("CASEMAPPING", value);
    }

    /// <summary>
    ///     Channel join limits by prefix
    /// </summary>
    public Dictionary<string, int?> ChanLimit
    {
        get
        {
            var result = new Dictionary<string, int?>();
            if (Parameters.TryGetValue("CHANLIMIT", out var val))
            {
                foreach (var pair in val.Split(','))
                {
                    var parts = pair.Split(':');
                    if (parts.Length >= 1)
                    {
                        var prefixes = parts[0];
                        int? limit = null;

                        if (parts.Length >= 2 && int.TryParse(parts[1], out var limitVal))
                        {
                            limit = limitVal;
                        }

                        foreach (var prefix in prefixes)
                        {
                            result[prefix.ToString()] = limit;
                        }
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    ///     Channel mode types
    /// </summary>
    public (string TypeA, string TypeB, string TypeC, string TypeD)? ChanModes
    {
        get
        {
            if (Parameters.TryGetValue("CHANMODES", out var val))
            {
                var parts = val.Split(',');
                if (parts.Length >= 4)
                {
                    return (parts[0], parts[1], parts[2], parts[3]);
                }
            }

            return null;
        }
        set
        {
            if (value.HasValue)
            {
                SetParameter(
                    "CHANMODES",
                    $"{value.Value.TypeA},{value.Value.TypeB},{value.Value.TypeC},{value.Value.TypeD}"
                );
            }
            else
            {
                Parameters.Remove("CHANMODES");
            }
        }
    }

    /// <summary>
    ///     Maximum channel name length
    /// </summary>
    public int? ChannelLen
    {
        get => Parameters.TryGetValue("CHANNELLEN", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("CHANNELLEN", value?.ToString());
    }

    /// <summary>
    ///     Available channel prefix characters
    /// </summary>
    public string ChanTypes
    {
        get => Parameters.TryGetValue("CHANTYPES", out var val) ? val : "#&";
        set => SetParameter("CHANTYPES", value);
    }

    /// <summary>
    ///     LIST command search extensions
    /// </summary>
    public string EList
    {
        get => Parameters.TryGetValue("ELIST", out var val) ? val : null;
        set => SetParameter("ELIST", value);
    }

    /// <summary>
    ///     Ban exception mode character
    /// </summary>
    public char? Excepts
    {
        get
        {
            if (Parameters.TryGetValue("EXCEPTS", out var val))
            {
                return string.IsNullOrEmpty(val) ? 'e' : val[0];
            }

            return null;
        }
        set
        {
            if (value.HasValue)
            {
                SetParameter("EXCEPTS", value.ToString());
            }
            else
            {
                Parameters.Remove("EXCEPTS");
            }
        }
    }

    /// <summary>
    ///     Extended ban information
    /// </summary>
    public (string Prefix, string Types)? ExtBan
    {
        get
        {
            if (Parameters.TryGetValue("EXTBAN", out var val))
            {
                var parts = val.Split(',');
                if (parts.Length == 2)
                {
                    return (parts[0], parts[1]);
                }

                if (parts.Length == 1)
                {
                    return (string.Empty, parts[0]);
                }
            }

            return null;
        }
        set
        {
            if (value.HasValue)
            {
                if (string.IsNullOrEmpty(value.Value.Prefix))
                {
                    SetParameter("EXTBAN", $",{value.Value.Types}");
                }
                else
                {
                    SetParameter("EXTBAN", $"{value.Value.Prefix},{value.Value.Types}");
                }
            }
            else
            {
                Parameters.Remove("EXTBAN");
            }
        }
    }

    /// <summary>
    ///     Maximum hostname length
    /// </summary>
    public int? HostLen
    {
        get => Parameters.TryGetValue("HOSTLEN", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("HOSTLEN", value?.ToString());
    }

    /// <summary>
    ///     Invite exception mode character
    /// </summary>
    public char? Invex
    {
        get
        {
            if (Parameters.TryGetValue("INVEX", out var val))
            {
                return string.IsNullOrEmpty(val) ? 'I' : val[0];
            }

            return null;
        }
        set
        {
            if (value.HasValue)
            {
                SetParameter("INVEX", value.ToString());
            }
            else
            {
                Parameters.Remove("INVEX");
            }
        }
    }

    /// <summary>
    ///     Maximum kick reason length
    /// </summary>
    public int? KickLen
    {
        get => Parameters.TryGetValue("KICKLEN", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("KICKLEN", value?.ToString());
    }

    /// <summary>
    ///     Maximum list mode limits
    /// </summary>
    public Dictionary<string, int> MaxList
    {
        get
        {
            var result = new Dictionary<string, int>();
            if (Parameters.TryGetValue("MAXLIST", out var val))
            {
                foreach (var pair in val.Split(','))
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out var limit))
                    {
                        foreach (var mode in parts[0])
                        {
                            result[mode.ToString()] = limit;
                        }
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    ///     Maximum targets for PRIVMSG/NOTICE
    /// </summary>
    public int? MaxTargets
    {
        get => Parameters.TryGetValue("MAXTARGETS", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("MAXTARGETS", value?.ToString());
    }

    /// <summary>
    ///     Maximum variable modes per MODE command
    /// </summary>
    public int? Modes
    {
        get => Parameters.TryGetValue("MODES", out var val) && int.TryParse(val, out var result) ? result : 3;
        set => SetParameter("MODES", value?.ToString());
    }

    /// <summary>
    ///     IRC network name
    /// </summary>
    public string Network
    {
        get => Parameters.GetValueOrDefault("NETWORK");
        set => SetParameter("NETWORK", value);
    }

    /// <summary>
    ///     Maximum nickname length
    /// </summary>
    public int? NickLen
    {
        get => Parameters.TryGetValue("NICKLEN", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("NICKLEN", value?.ToString());
    }

    /// <summary>
    ///     Channel membership prefixes
    /// </summary>
    public (string Modes, string Prefixes)? Prefix
    {
        get
        {
            if (Parameters.TryGetValue("PREFIX", out var val))
            {
                if (string.IsNullOrEmpty(val))
                {
                    return (string.Empty, string.Empty);
                }

                if (val.StartsWith('(') && val.Contains(')'))
                {
                    var modesEnd = val.IndexOf(')');
                    var modes = val.Substring(1, modesEnd - 1);
                    var prefixes = val.Substring(modesEnd + 1);
                    return (modes, prefixes);
                }
            }

            return ("ov", "@+");
        }
        set
        {
            if (value.HasValue)
            {
                if (string.IsNullOrEmpty(value.Value.Modes) && string.IsNullOrEmpty(value.Value.Prefixes))
                {
                    SetParameter("PREFIX", string.Empty);
                }
                else
                {
                    SetParameter("PREFIX", $"({value.Value.Modes}){value.Value.Prefixes}");
                }
            }
            else
            {
                Parameters.Remove("PREFIX");
            }
        }
    }

    /// <summary>
    ///     Whether the server supports safe LIST usage
    /// </summary>
    public bool SafeList
    {
        get => Parameters.ContainsKey("SAFELIST");
        set
        {
            if (value)
            {
                SetParameter("SAFELIST", null);
            }
            else
            {
                Parameters.Remove("SAFELIST");
            }
        }
    }

    /// <summary>
    ///     Maximum silence list entries
    /// </summary>
    public int? Silence
    {
        get
        {
            if (Parameters.TryGetValue("SILENCE", out var val))
            {
                if (string.IsNullOrEmpty(val))
                {
                    return 0; // Indicates SILENCE is supported but no limit specified
                }

                if (int.TryParse(val, out var result))
                {
                    return result;
                }
            }

            return null;
        }
        set
        {
            if (value.HasValue)
            {
                SetParameter("SILENCE", value.Value > 0 ? value.ToString() : string.Empty);
            }
            else
            {
                Parameters.Remove("SILENCE");
            }
        }
    }

    /// <summary>
    ///     Status message prefixes
    /// </summary>
    public string StatusMsg
    {
        get => Parameters.TryGetValue("STATUSMSG", out var val) ? val : null;
        set => SetParameter("STATUSMSG", value);
    }

    /// <summary>
    ///     Command target limits
    /// </summary>
    public Dictionary<string, int?> TargMax
    {
        get
        {
            var result = new Dictionary<string, int?>();
            if (Parameters.TryGetValue("TARGMAX", out var val))
            {
                foreach (var pair in val.Split(','))
                {
                    var parts = pair.Split(':');
                    if (parts.Length >= 1)
                    {
                        var command = parts[0];
                        int? limit = null;

                        if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[1]) && int.TryParse(parts[1], out var limitVal))
                        {
                            limit = limitVal;
                        }

                        result[command.ToUpper()] = limit;
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    ///     Maximum topic length
    /// </summary>
    public int? TopicLen
    {
        get => Parameters.TryGetValue("TOPICLEN", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("TOPICLEN", value?.ToString());
    }

    /// <summary>
    ///     Maximum username length
    /// </summary>
    public int? UserLen
    {
        get => Parameters.TryGetValue("USERLEN", out var val) && int.TryParse(val, out var result) ? result : null;
        set => SetParameter("USERLEN", value?.ToString());
    }

    /// <summary>
    ///     Helper method to set a parameter value or remove it if the value is null
    /// </summary>
    private void SetParameter(string parameter, string value)
    {
        // Parameters must be uppercase
        parameter = parameter.ToUpperInvariant();

        // Remove from negated parameters if present
        NegatedParameters.Remove(parameter);

        if (value == null)
        {
            Parameters.Remove(parameter);
        }
        else
        {
            Parameters[parameter] = value;
        }
    }

    /// <summary>
    ///     Negates a parameter, removing its previous value if any
    /// </summary>
    public void NegateParameter(string parameter)
    {
        parameter = parameter.ToUpperInvariant();
        Parameters.Remove(parameter);
        NegatedParameters.Add(parameter);
    }

    /// <summary>
    ///     Validates that a parameter name follows the ISUPPORT ABNF rules
    /// </summary>
    private bool IsValidParameterName(string parameter)
    {
        if (string.IsNullOrEmpty(parameter) || parameter.Length > 20)
        {
            return false;
        }

        // parameter = 1*20 letter
        // letter = ALPHA / DIGIT
        return Regex.IsMatch(parameter, "^[A-Z0-9]{1,20}$");
    }

    /// <summary>
    ///     Encodes a value for the ISUPPORT token, escaping characters as needed
    /// </summary>
    private string EncodeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value
            .Replace("\\", "\\x5C")
            .Replace(" ", "\\x20")
            .Replace("=", "\\x3D");
    }

    /// <summary>
    ///     Decodes a value from an ISUPPORT token, unescaping characters
    /// </summary>
    private string DecodeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return Regex.Replace(
            value,
            @"\\x([0-9A-Fa-f]{2})",
            match =>
            {
                var charCode = Convert.ToInt32(match.Groups[1].Value, 16);
                return ((char)charCode).ToString();
            }
        );
    }

    public override void Parse(string line)
    {
        // RPL_ISUPPORT format: ":server 005 nickname param1 param2=value param3 :are supported by this server"

        if (!line.StartsWith(':'))
        {
            return; // Invalid format
        }

        var parts = line.Split(' ');
        if (parts.Length < 4)
        {
            return; // Invalid format
        }

        ServerName = parts[0].TrimStart(':');

        // Check if this is RPL_ISUPPORT (005) or RPL_REMOTEISUPPORT (105)
        var numeric = parts[1];
        if (numeric != "005" && numeric != "105")
        {
            return; // Not an ISUPPORT message
        }

        Nickname = parts[2];

        // Find the index of the trailing message
        var trailingIndex = -1;
        for (var i = 3; i < parts.Length; i++)
        {
            if (parts[i].StartsWith(':'))
            {
                trailingIndex = i;
                break;
            }
        }

        // Parse tokens
        for (var i = 3; i < (trailingIndex != -1 ? trailingIndex : parts.Length); i++)
        {
            var token = parts[i];

            // Handle negated parameters (token starts with -)
            if (token.StartsWith('-'))
            {
                var name = token.Substring(1).ToUpperInvariant();
                if (IsValidParameterName(name))
                {
                    Parameters.Remove(name);
                    NegatedParameters.Add(name);
                }

                continue;
            }

            // Handle parameters with values
            var equalPos = token.IndexOf('=');
            if (equalPos != -1)
            {
                var name = token.Substring(0, equalPos).ToUpperInvariant();

                if (!IsValidParameterName(name))
                {
                    continue; // Invalid parameter name
                }

                var value = token.Substring(equalPos + 1);
                Parameters[name] = DecodeValue(value);
                NegatedParameters.Remove(name);
            }
            else
            {
                // Parameter without a value (boolean parameter)
                var name = token.ToUpperInvariant();

                if (!IsValidParameterName(name))
                {
                    continue; // Invalid parameter name
                }

                Parameters[name] = null; // null indicates a boolean parameter
                NegatedParameters.Remove(name);
            }
        }

        // Parse trailing message
        if (trailingIndex != -1)
        {
            var messageBuilder = new StringBuilder();
            for (var i = trailingIndex; i < parts.Length; i++)
            {
                if (i == trailingIndex)
                {
                    messageBuilder.Append(parts[i].Substring(1)); // Remove the leading :
                }
                else
                {
                    messageBuilder.Append(' ').Append(parts[i]);
                }
            }

            Message = messageBuilder.ToString();
        }
    }

    // public override string Write()
    // {
    //     // Format: ":server 005 nickname param1 param2=value ... :are supported by this server"
    //     // A single message can have at most 13 tokens
    //
    //     // We'll need to break this into multiple messages if there are too many tokens
    //     var allTokens = new List<string>();
    //
    //     // Add regular parameters
    //     foreach (var param in Parameters)
    //     {
    //         if (param.Value == null)
    //         {
    //             // Boolean parameter
    //             allTokens.Add(param.Key);
    //         }
    //         else
    //         {
    //             // Parameter with value
    //             allTokens.Add($"{param.Key}={EncodeValue(param.Value)}");
    //         }
    //     }
    //
    //     // Add negated parameters
    //     foreach (var param in NegatedParameters)
    //     {
    //         allTokens.Add($"-{param}");
    //     }
    //
    //     // If there are no tokens, just return a basic message
    //     if (allTokens.Count == 0)
    //     {
    //         return $":{ServerName} 005 {Nickname} :{Message}";
    //     }
    //
    //     // If we have 13 or fewer tokens, we can fit them all in one message
    //     if (allTokens.Count <= MaxTokensPerMessage)
    //     {
    //         return $":{ServerName} 005 {Nickname} {string.Join(" ", allTokens)} :{Message}";
    //     }
    //
    //     // Otherwise, we need to generate multiple messages
    //     // This implementation returns just the first message, as the full implementation
    //     // would require a different architecture to support multiple messages
    //     var firstMessageTokens = allTokens.Take(MaxTokensPerMessage);
    //     return $":{ServerName} 005 {Nickname} {string.Join(" ", firstMessageTokens)} :{Message}";
    // }

    public override string Write()
    {
        var messages = WriteAllMessages();
        if (messages.Length == 2)
        {
            return messages[0] + "\r\n" + messages[1];
        }

        return messages[0];
    }

    /// <summary>
    ///     Generates multiple RPL_ISUPPORT messages if necessary due to token limits
    /// </summary>
    /// <returns>An array of message strings</returns>
    public string[] WriteAllMessages()
    {
        var allTokens = new List<string>();

        // Add regular parameters
        foreach (var param in Parameters)
        {
            if (param.Value == null || string.IsNullOrEmpty(param.Value))
            {
                // Boolean parameter
                allTokens.Add(param.Key);
            }
            else
            {
                // Parameter with value
                allTokens.Add($"{param.Key}={EncodeValue(param.Value)}");
            }
        }

        // Add negated parameters
        foreach (var param in NegatedParameters)
        {
            allTokens.Add($"-{param}");
        }

        // If there are no tokens, just return a basic message
        if (allTokens.Count == 0)
        {
            return new[] { $":{ServerName} 005 {Nickname} :{Message}" };
        }

        // Calculate how many messages we need
        var messageCount = (allTokens.Count + MaxTokensPerMessage - 1) / MaxTokensPerMessage;
        var messages = new string[messageCount];

        for (var i = 0; i < messageCount; i++)
        {
            var tokensForMessage = allTokens
                .Skip(i * MaxTokensPerMessage)
                .Take(MaxTokensPerMessage)
                .ToArray();

            messages[i] = $":{ServerName} 005 {Nickname} {string.Join(" ", tokensForMessage)} :{Message}";
        }

        return messages;
    }

    /// <summary>
    ///     Adds a target limit for a specific command
    /// </summary>
    public void AddTargMax(string command, int? limit)
    {
        if (string.IsNullOrEmpty(command))
        {
            return;
        }

        if (!Parameters.TryGetValue("TARGMAX", out var val))
        {
            val = string.Empty;
        }

        var pairs = new List<string>(val.Split(',', StringSplitOptions.RemoveEmptyEntries));
        string newPair;

        if (limit.HasValue)
        {
            newPair = $"{command.ToUpper()}:{limit.Value}";
        }
        else
        {
            newPair = $"{command.ToUpper()}:";
        }

        // Remove existing entry for this command if it exists
        pairs.RemoveAll(p => p.StartsWith(command.ToUpper() + ":"));

        // Add the new pair
        pairs.Add(newPair);

        Parameters["TARGMAX"] = string.Join(',', pairs);
    }

    /// <summary>
    ///     Adds a channel limit for a specific channel type
    /// </summary>
    public void AddChanLimit(string prefixes, int? limit)
    {
        if (string.IsNullOrEmpty(prefixes))
        {
            return;
        }

        if (!Parameters.TryGetValue("CHANLIMIT", out var val))
        {
            val = string.Empty;
        }

        var pairs = new List<string>(val.Split(',', StringSplitOptions.RemoveEmptyEntries));
        string newPair;

        if (limit.HasValue)
        {
            newPair = $"{prefixes}:{limit.Value}";
        }
        else
        {
            newPair = $"{prefixes}:";
        }

        // Remove existing entries for these prefixes
        pairs.RemoveAll(
            p =>
            {
                var parts = p.Split(':');
                return parts.Length > 0 && prefixes.Any(c => parts[0].Contains(c));
            }
        );

        // Add the new pair
        pairs.Add(newPair);

        Parameters["CHANLIMIT"] = string.Join(',', pairs);
    }

    /// <summary>
    ///     Adds a max list limit for specific modes
    /// </summary>
    public void AddMaxList(string modes, int limit)
    {
        if (string.IsNullOrEmpty(modes) || limit <= 0)
        {
            return;
        }

        if (!Parameters.TryGetValue("MAXLIST", out var val))
        {
            val = string.Empty;
        }

        var pairs = new List<string>(val.Split(',', StringSplitOptions.RemoveEmptyEntries));
        var newPair = $"{modes}:{limit}";

        // Remove existing entries for these modes
        pairs.RemoveAll(
            p =>
            {
                var parts = p.Split(':');
                return parts.Length > 0 && modes.Any(c => parts[0].Contains(c));
            }
        );

        // Add the new pair
        pairs.Add(newPair);

        Parameters["MAXLIST"] = string.Join(',', pairs);
    }
}
