namespace AbyssIrc.Protocol.Messages.Data.Channels;

/// <summary>
///     Represents data for a channel being joined
/// </summary>
public class JoinChannelData
{
    /// <summary>
    ///     Creates a new JoinChannelData instance
    /// </summary>
    /// <param name="channelName">Name of the channel</param>
    /// <param name="key">Optional key for the channel</param>
    public JoinChannelData(string channelName, string key = null)
    {
        ChannelName = channelName.ToLower() ?? throw new ArgumentNullException(nameof(channelName));
        Key = key;
    }

    /// <summary>
    ///     Name of the channel
    /// </summary>
    public string ChannelName { get; set; }

    /// <summary>
    ///     Optional key for password-protected channels
    /// </summary>
    public string Key { get; set; }


    public bool IsValid => !string.IsNullOrEmpty(ChannelName) && ChannelName.StartsWith('#') && ChannelName.Length > 1 ||
                           ChannelName.StartsWith('&');

    /// <summary>
    ///     Returns a string representation of the join channel data
    /// </summary>
    public override string ToString()
    {
        return string.IsNullOrEmpty(Key) ? ChannelName : $"{ChannelName} (key: {Key})";
    }
}
