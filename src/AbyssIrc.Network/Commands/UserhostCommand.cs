using System;
using System.Collections.Generic;
using System.Linq;
using AbyssIrc.Network.Commands.Base;
using AbyssIrc.Network.Commands.Replies;

namespace AbyssIrc.Network.Commands;

/// <summary>
/// Represents the IRC USERHOST command used to query information about users
/// </summary>
public class UserhostCommand : BaseIrcCommand
{
    /// <summary>
    /// The source of the command (optional, used when relayed by server)
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// The list of nicknames to query
    /// </summary>
    public List<string> Nicknames { get; set; } = new List<string>();

    public UserhostCommand() : base("USERHOST")
    {
    }

    /// <summary>
    /// Parses a USERHOST command from a raw IRC message
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Reset existing data
        Nicknames.Clear();
        Source = null;

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

        // First token should be "USERHOST"
        if (parts.Length == 0 || parts[0].ToUpper() != "USERHOST")
            return;

        // Collect nicknames (starting from index 1)
        for (int i = 1; i < parts.Length; i++)
        {
            string nickname = parts[i].Trim();
            if (!string.IsNullOrWhiteSpace(nickname))
            {
                Nicknames.Add(nickname);
            }
        }
    }

    /// <summary>
    /// Converts the command to its string representation
    /// </summary>
    /// <returns>Formatted USERHOST command string</returns>
    public override string Write()
    {
        // With source (server-side)
        if (!string.IsNullOrEmpty(Source))
        {
            return $":{Source} USERHOST {string.Join(" ", Nicknames)}";
        }

        // Client-side
        return $"USERHOST {string.Join(" ", Nicknames)}";
    }

    /// <summary>
    /// Represents a single USERHOST reply for a user
    /// </summary>
    public class UserhostReply
    {
        /// <summary>
        /// Nickname of the queried user
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// User's hostname
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Indicates if the user is logged in (away)
        /// </summary>
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// Indicates if the user is an IRC operator
        /// </summary>
        public bool IsOperator { get; set; }

        /// <summary>
        /// Converts the reply to a user-friendly string
        /// </summary>
        public override string ToString()
        {
            return $"{Nickname}={(IsLoggedIn ? "+" : "-")}({(IsOperator ? "@" : "")}{Hostname})";
        }
    }

    /// <summary>
    /// Represents the RPL_USERHOST (302) numeric reply
    /// </summary>
    public class RplUserhostCommand : BaseIrcCommand
    {
        /// <summary>
        /// The server name sending the reply
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// The nickname of the client receiving the reply
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// List of userhost replies
        /// </summary>
        public List<UserhostReply> Replies { get; set; } = new List<UserhostReply>();

        public RplUserhostCommand() : base("302")
        {
        }

        /// <summary>
        /// Parses the RPL_USERHOST reply
        /// </summary>
        /// <param name="line">Raw IRC message</param>
        public override void Parse(string line)
        {
            // Reset existing replies
            Replies.Clear();

            // Example: :server.com 302 nickname :nickname=+user@host
            var parts = line.Split(' ', 4);

            if (parts.Length < 4)
                return;

            ServerName = parts[0].TrimStart(':');
            // parts[1] should be "302"
            Nickname = parts[2];

            // Parse the reply part
            string replyPart = parts[3].TrimStart(':');
            var userReplies = replyPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var userReply in userReplies)
            {
                // Parse each user reply
                var match = System.Text.RegularExpressions.Regex.Match(
                    userReply,
                    @"(\w+)([+-])(\*)?(@)?(.+)"
                );

                if (match.Success)
                {
                    Replies.Add(
                        new UserhostReply
                        {
                            Nickname = match.Groups[1].Value,
                            IsLoggedIn = match.Groups[2].Value == "+",
                            IsOperator = match.Groups[4].Success
                        }
                    );
                }
            }
        }

        /// <summary>
        /// Converts the reply to its string representation
        /// </summary>
        public override string Write()
        {
            var replyStrings = Replies.Select(r => r.ToString());
            return $":{ServerName} 302 {Nickname} :{string.Join(" ", replyStrings)}";
        }

        /// <summary>
        /// Creates a RPL_USERHOST reply
        /// </summary>
        public static RplUserhostCommand Create(
            string serverName,
            string nickname,
            IEnumerable<UserhostReply> replies
        )
        {
            return new RplUserhostCommand
            {
                ServerName = serverName,
                Nickname = nickname,
                Replies = replies.ToList()
            };
        }
    }

    /// <summary>
    /// Creates a USERHOST command with specified nicknames
    /// </summary>
    /// <param name="nicknames">Nicknames to query</param>
    public static UserhostCommand Create(params string[] nicknames)
    {
        return new UserhostCommand
        {
            Nicknames = nicknames.ToList()
        };
    }
}
