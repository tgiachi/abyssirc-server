using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents an IRC CAP command used for capability negotiation
/// </summary>
public class CapCommand : BaseIrcCommand
{
    /// <summary>
    /// The CAP subcommand (LS, LIST, REQ, ACK, NAK, END, NEW, DEL)
    /// </summary>
    public string SubCommand { get; set; }

    /// <summary>
    /// The optional CAP version (e.g., 302)
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// List of capabilities when applicable
    /// </summary>
    public List<string> Capabilities { get; set; } = new List<string>();

    /// <summary>
    /// The client identifier (usually * for unregistered clients)
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Indicates if this is a server response
    /// </summary>
    public bool IsServerResponse { get; set; }

    /// <summary>
    /// The server prefix if this is a server response
    /// </summary>
    public string ServerPrefix { get; set; }

    public CapCommand() : base("CAP")
    {
    }

    public override void Parse(string line)
    {
        // Examples:
        // Client to server: CAP LS 302
        // Client to server: CAP REQ :sasl multi-prefix
        // Server to client: :irc.server.net CAP * LS :multi-prefix sasl account-notify extended-join tls

        // Handle server response format
        if (line.StartsWith(":"))
        {
            IsServerResponse = true;

            // Split into parts
            var parts = line.Split(' ');

            if (parts.Length < 4)
                return; // Invalid format

            ServerPrefix = parts[0].TrimStart(':');
            // parts[1] should be "CAP"
            ClientId = parts[2];
            SubCommand = parts[3].ToUpperInvariant();

            // If there are capabilities listed
            if (parts.Length > 4)
            {
                // Capabilities might be prefixed with : if at the end
                var capString = string.Join(" ", parts.Skip(4));

                if (capString.StartsWith(":"))
                    capString = capString.Substring(1);

                Capabilities = capString.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        else
        {
            // Client request format
            var parts = line.Split(' ');

            if (parts.Length < 2)
                return; // Invalid format

            // parts[0] should be "CAP"
            SubCommand = parts[1].ToUpperInvariant();

            // If version is specified (CAP LS 302)
            if (parts.Length > 2 && !parts[2].StartsWith(":"))
            {
                Version = parts[2];
            }

            // If capabilities are specified (usually in REQ subcommand)
            if (parts.Length > 2)
            {
                int startIndex = (Version != null) ? 3 : 2;

                if (parts.Length > startIndex)
                {
                    var capString = string.Join(" ", parts.Skip(startIndex));

                    if (capString.StartsWith(":"))
                        capString = capString.Substring(1);

                    Capabilities = capString.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
        }
    }

    public override string Write()
    {
        if (IsServerResponse)
        {
            // Server response format
            var caps = Capabilities.Any() ? " :" + string.Join(" ", Capabilities) : "";
            return $":{ServerPrefix} CAP {ClientId} {SubCommand}{caps}";
        }
        else
        {
            // Client request format
            var versionStr = !string.IsNullOrEmpty(Version) ? $" {Version}" : "";
            var caps = Capabilities.Any() ? " :" + string.Join(" ", Capabilities) : "";
            return $"CAP {SubCommand}{versionStr}{caps}";
        }
    }
}
