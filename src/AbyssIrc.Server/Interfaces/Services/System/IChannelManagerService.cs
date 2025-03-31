using System.Collections.Concurrent;
using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Network.Data.Channels;

namespace AbyssIrc.Server.Interfaces.Services.System;

/// <summary>
/// Interface for the channel management service that handles IRC channels operations
/// </summary>
public interface IChannelManagerService : IAbyssStarStopService
{
    /// <summary>
    /// Dictionary of all active channels in the IRC server
    /// Key: Channel name (case-insensitive)
    /// Value: Channel data containing members, modes, and other properties
    /// </summary>
    ConcurrentDictionary<string, ChannelData> Channels { get; }

    /// <summary>
    /// Checks if a channel is already registered in the server
    /// </summary>
    /// <param name="channelName">The name of the channel to check</param>
    /// <returns>True if the channel exists, false otherwise</returns>
    bool IsChannelRegistered(string channelName);

    /// <summary>
    /// Registers a new channel in the server
    /// </summary>
    /// <param name="channelName">The name of the channel to register</param>
    void RegisterChannel(string channelName);

    /// <summary>
    /// Adds a user to a channel
    /// </summary>
    /// <param name="channelName">The name of the channel</param>
    /// <param name="nickname">The nickname of the user to add</param>
    void AddNicknameToChannel(string channelName, string nickname);

    /// <summary>
    /// Removes a user from a channel
    /// </summary>
    /// <param name="channelName">The name of the channel</param>
    /// <param name="nickname">The nickname of the user to remove</param>
    void RemoveNicknameFromChannel(string channelName, string nickname);

    /// <summary>
    ///  Gets a list of all registered channel names
    /// </summary>
    /// <returns></returns>
    List<string> GetChannelNames();

    /// <summary>
    ///  Gets a list of all registered channels and their topics
    /// </summary>
    /// <returns></returns>
    List<(string channelName, string topic, int memberCount)> GetChannelTopics();

    /// <summary>
    ///  Gets a list of all nicknames in a specific channel
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    List<string> GetNicknamesInChannel(string channelName);
}
