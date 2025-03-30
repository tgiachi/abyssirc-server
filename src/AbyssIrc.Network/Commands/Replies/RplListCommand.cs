using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

 /// <summary>
    /// Represents RPL_LIST (322) numeric reply
    /// Provides details about a single channel
    /// </summary>
    public class RplListCommand : BaseIrcCommand
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
        /// The channel name
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Number of visible users in the channel
        /// </summary>
        public int VisibleUserCount { get; set; }

        /// <summary>
        /// Channel topic
        /// </summary>
        public string Topic { get; set; }

        public RplListCommand() : base("322")
        {
        }

        /// <summary>
        /// Parses the RPL_LIST numeric reply
        /// </summary>
        /// <param name="line">Raw IRC message</param>
        public override void Parse(string line)
        {
            // Example: :server.com 322 nickname #channel 42 :Channel topic goes here

            // Reset existing data
            ServerName = null;
            Nickname = null;
            ChannelName = null;
            VisibleUserCount = 0;
            Topic = null;

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
            if (parts.Length < 4)
                return;

            // Verify the numeric code
            if (parts[0] != "322")
                return;

            // Extract nickname
            Nickname = parts[1];

            // Extract channel name
            ChannelName = parts[2];

            // Extract visible user count
            if (int.TryParse(parts[3], out int userCount))
            {
                VisibleUserCount = userCount;
            }

            // Extract topic
            int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + parts[2].Length + parts[3].Length + 4);
            if (colonIndex != -1)
            {
                Topic = line.Substring(colonIndex + 1);
            }
        }

        /// <summary>
        /// Converts the reply to its string representation
        /// </summary>
        /// <returns>Formatted RPL_LIST message</returns>
        public override string Write()
        {
            return string.IsNullOrEmpty(ServerName)
                ? (string.IsNullOrEmpty(Topic)
                    ? $"322 {Nickname} {ChannelName} {VisibleUserCount}"
                    : $"322 {Nickname} {ChannelName} {VisibleUserCount} :{Topic}")
                : (string.IsNullOrEmpty(Topic)
                    ? $":{ServerName} 322 {Nickname} {ChannelName} {VisibleUserCount}"
                    : $":{ServerName} 322 {Nickname} {ChannelName} {VisibleUserCount} :{Topic}");
        }

        /// <summary>
        /// Creates a RPL_LIST reply
        /// </summary>
        /// <param name="serverName">Server sending the reply</param>
        /// <param name="nickname">Nickname of the client</param>
        /// <param name="channelName">Name of the channel</param>
        /// <param name="visibleUserCount">Number of visible users</param>
        /// <param name="topic">Optional channel topic</param>
        public static RplListCommand Create(
            string serverName,
            string nickname,
            string channelName,
            int visibleUserCount,
            string topic = null)
        {
            return new RplListCommand
            {
                ServerName = serverName,
                Nickname = nickname,
                ChannelName = channelName,
                VisibleUserCount = visibleUserCount,
                Topic = topic
            };
        }
    }
