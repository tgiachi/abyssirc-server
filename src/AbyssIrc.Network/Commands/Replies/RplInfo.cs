using System.Reflection;
using AbyssIrc.Network.Commands.Base;

namespace AbyssIrc.Network.Commands.Replies;

/// <summary>
/// Represents RPL_INFO (371) numeric reply
/// Provides server information in multiple lines
/// </summary>
public class RplInfo : BaseIrcCommand
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
    /// Information text for this line
    /// </summary>
    public string InfoText { get; set; }

    public RplInfo() : base("371")
    {
    }

    /// <summary>
    /// Parses the RPL_INFO numeric reply
    /// </summary>
    /// <param name="line">Raw IRC message</param>
    public override void Parse(string line)
    {
        // Example: :server.com 371 nickname :Server information text

        // Reset existing data
        ServerName = null;
        Nickname = null;
        InfoText = null;

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
        if (parts[0] != "371")
            return;

        // Extract nickname
        Nickname = parts[1];

        // Extract info text
        int colonIndex = line.IndexOf(':', parts[0].Length + parts[1].Length + 2);
        if (colonIndex != -1)
        {
            InfoText = line.Substring(colonIndex + 1);
        }
    }

    /// <summary>
    /// Converts the reply to its string representation
    /// </summary>
    /// <returns>Formatted RPL_INFO message</returns>
    public override string Write()
    {
        return string.IsNullOrEmpty(ServerName)
            ? $"371 {Nickname} :{InfoText}"
            : $":{ServerName} 371 {Nickname} :{InfoText}";
    }

    /// <summary>
    /// Creates a RPL_INFO reply
    /// </summary>
    /// <param name="serverName">Server sending the reply</param>
    /// <param name="nickname">Nickname of the client</param>
    /// <param name="infoText">Server information text</param>
    public static RplInfo Create(
        string serverName,
        string nickname,
        string infoText
    )
    {
        return new RplInfo
        {
            ServerName = serverName,
            Nickname = nickname,
            InfoText = infoText
        };
    }

    /// <summary>
    /// Generates a list of default server information lines
    /// </summary>
    /// <returns>Collection of server information lines</returns>
    public static List<RplInfo> GenerateDefaultServerInfo(
        string serverName,
        string nickname
    )
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        return
        [
            Create(serverName, nickname, $"AbyssIRC Server Version {version}"),
            Create(serverName, nickname, "Copyright (c) 2024 AbyssIRC Development Team"),
            Create(serverName, nickname, ""),
            Create(serverName, nickname, "This program is free software: you can redistribute it and/or modify"),
            Create(serverName, nickname, "it under the terms of the GNU General Public License as published by"),
            Create(serverName, nickname, "the Free Software Foundation, either version 3 of the License, or"),
            Create(serverName, nickname, "(at your option) any later version."),
            Create(serverName, nickname, ""),
            Create(serverName, nickname, "Developed with passion for the IRC community."),
            Create(serverName, nickname, "https://github.com/tgiachi/abyssirc-server")
        ];
    }
}
