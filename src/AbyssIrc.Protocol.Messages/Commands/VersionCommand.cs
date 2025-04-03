using AbyssIrc.Protocol.Messages.Commands.Base;

namespace AbyssIrc.Protocol.Messages.Commands;

  /// <summary>
    /// Represents the IRC VERSION command used to query server version information
    /// </summary>
    public class VersionCommand : BaseIrcCommand
    {
        /// <summary>
        /// Source of the VERSION command (optional, used when relayed by server)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Optional target server to query version from
        /// </summary>
        public string Target { get; set; }

        public VersionCommand() : base("VERSION")
        {
        }

        /// <summary>
        /// Parses a VERSION command from a raw IRC message
        /// </summary>
        /// <param name="line">Raw IRC message</param>
        public override void Parse(string line)
        {
            // Reset existing data
            Source = null;
            Target = null;

            // Check for source prefix
            if (line.StartsWith(':'))
            {
                int spaceIndex = line.IndexOf(' ');
                if (spaceIndex != -1)
                {
                    Source = line.Substring(1, spaceIndex - 1);
                    line = line.Substring(spaceIndex + 1).TrimStart();
                }
            }

            // Split remaining parts
            string[] parts = line.Split(' ');

            // First token should be "VERSION"
            if (parts.Length == 0 || parts[0].ToUpper() != "VERSION")
                return;

            // Check for optional target server
            if (parts.Length > 1)
            {
                Target = parts[1];
            }
        }

        /// <summary>
        /// Converts the command to its string representation
        /// </summary>
        /// <returns>Formatted VERSION command string</returns>
        public override string Write()
        {
            // Prepare base command
            var commandBuilder = new System.Text.StringBuilder();

            // Add source if present (server-side)
            if (!string.IsNullOrEmpty(Source))
            {
                commandBuilder.Append(':').Append(Source).Append(' ');
            }

            // Add VERSION command
            commandBuilder.Append("VERSION");

            // Add target if present
            if (!string.IsNullOrEmpty(Target))
            {
                commandBuilder.Append(' ').Append(Target);
            }

            return commandBuilder.ToString();
        }

        /// <summary>
        /// Creates a VERSION command to query server version
        /// </summary>
        public static VersionCommand Create()
        {
            return new VersionCommand();
        }

        /// <summary>
        /// Creates a VERSION command for a specific target server
        /// </summary>
        /// <param name="target">Target server to query</param>
        public static VersionCommand CreateForTarget(string target)
        {
            return new VersionCommand
            {
                Target = target
            };
        }
    }
