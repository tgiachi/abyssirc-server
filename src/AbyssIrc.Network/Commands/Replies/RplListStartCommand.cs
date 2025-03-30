using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

 /// <summary>
    /// Represents RPL_LISTSTART (321) numeric reply
    /// Indicates the start of a channel list
    /// </summary>
    public class RplListStartCommand : BaseIrcCommand
    {
        /// <summary>
        /// The server name sending this reply
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// The nickname of the client receiving this reply
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Optional header message
        /// </summary>
        public string Message { get; set; } = "Channel Users Name";

        public RplListStartCommand() : base("321")
        {
        }

        /// <summary>
        /// Parses the RPL_LISTSTART numeric reply
        /// </summary>
        /// <param name="line">Raw IRC message</param>
        public override void Parse(string line)
        {
            // Example: :server.com 321 nickname :Channel Users Name

            // Reset existing data
            ServerName = null;
            Nickname = null;
            Message = "Channel Users Name";

            // Check for source prefix
            if (line.StartsWith(':'))
            {
                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex != -1)
                {
                    ServerName = line.Substring(1, spaceIndex - 1);
                    line = line.Substring(spaceIndex + 1).TrimStart();
                }
            }

            // Split remaining parts
            string[] parts = line.Split(' ');

            // Ensure we have enough parts
            if (parts.Length < 2)
                return;

            // Verify the numeric code
            if (parts[0] != "321")
                return;

            // Extract nickname
            Nickname = parts[1];

            // Extract message if present
            int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + 2);
            if (colonIndex != -1)
            {
                Message = line.Substring(colonIndex + 1);
            }
        }

        /// <summary>
        /// Converts the reply to its string representation
        /// </summary>
        /// <returns>Formatted RPL_LISTSTART message</returns>
        public override string Write()
        {
            return string.IsNullOrEmpty(ServerName)
                ? $"321 {Nickname} :{Message}"
                : $":{ServerName} 321 {Nickname} :{Message}";
        }

        /// <summary>
        /// Creates a RPL_LISTSTART reply
        /// </summary>
        /// <param name="serverName">Server sending the reply</param>
        /// <param name="nickname">Nickname of the client</param>
        /// <param name="message">Optional header message</param>
        public static RplListStartCommand Create(
            string serverName,
            string nickname,
            string message = null)
        {
            return new RplListStartCommand
            {
                ServerName = serverName,
                Nickname = nickname,
                Message = message ?? "Channel Users Name"
            };
        }
    }
