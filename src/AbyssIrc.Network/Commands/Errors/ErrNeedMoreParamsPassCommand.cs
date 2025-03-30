using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Errors;

/// <summary>
    /// Represents ERR_NEEDMOREPARAMS (461) error for PASS command
    /// Sent when the PASS command is missing required parameters
    /// </summary>
    public class ErrNeedMoreParamsPassCommand : BaseIrcCommand
    {
        /// <summary>
        /// The server name sending this error
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// The nickname of the client receiving this error
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// The error message explaining the lack of parameters
        /// </summary>
        public string ErrorMessage { get; set; } = "Not enough parameters";

        public ErrNeedMoreParamsPassCommand() : base("461")
        {
        }

        /// <summary>
        /// Parses the ERR_NEEDMOREPARAMS error message
        /// </summary>
        /// <param name="line">Raw IRC error message</param>
        public override void Parse(string line)
        {
            // Example: :server.com 461 nickname PASS :Not enough parameters

            // Reset existing data
            ServerName = null;
            Nickname = null;

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
            if (parts.Length < 3)
                return;

            // Verify the numeric code
            if (parts[0] != "461")
                return;

            // Extract nickname
            Nickname = parts[1];

            // Extract error message if present
            int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + parts[2].Length + 3);
            if (colonIndex != -1)
            {
                ErrorMessage = line.Substring(colonIndex + 1);
            }
        }

        /// <summary>
        /// Converts the error to its string representation
        /// </summary>
        /// <returns>Formatted error message</returns>
        public override string Write()
        {
            return string.IsNullOrEmpty(ServerName)
                ? $"461 {Nickname} PASS :{ErrorMessage}"
                : $":{ServerName} 461 {Nickname} PASS :{ErrorMessage}";
        }

        /// <summary>
        /// Creates an ERR_NEEDMOREPARAMS error for PASS command
        /// </summary>
        /// <param name="serverName">Server sending the error</param>
        /// <param name="nickname">Nickname of the client</param>
        /// <param name="errorMessage">Optional custom error message</param>
        public static ErrNeedMoreParamsPassCommand Create(
            string serverName,
            string nickname,
            string errorMessage = null)
        {
            return new ErrNeedMoreParamsPassCommand
            {
                ServerName = serverName,
                Nickname = nickname,
                ErrorMessage = errorMessage ?? "Not enough parameters"
            };
        }
    }

