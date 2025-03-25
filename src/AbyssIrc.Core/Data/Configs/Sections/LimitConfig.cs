namespace AbyssIrc.Core.Data.Configs.Sections;

public class LimitConfig
{
    /// <summary>
    /// Maximum length of a nickname
    /// </summary>
    public int MaxNickLength { get; set; } = 30;

    /// <summary>
    /// Maximum length of a channel name
    /// </summary>
    public int MaxChannelNameLength { get; set; } = 50;

    /// <summary>
    /// Maximum length of a channel topic
    /// </summary>
    public int MaxTopicLength { get; set; } = 390;

    /// <summary>
    /// Maximum number of targets for a single message (to prevent spam)
    /// </summary>
    public int MaxTargets { get; set; } = 4;

    /// <summary>
    /// Maximum length of a message
    /// </summary>
    public int MaxMessageLength { get; set; } = 512;

    /// <summary>
    /// Maximum number of channels a user can join
    /// </summary>
    public int MaxChannelsPerUser { get; set; } = 20;

    /// <summary>
    /// Maximum number of bans per channel
    /// </summary>
    public int MaxBansPerChannel { get; set; } = 50;

    /// <summary>
    /// User modes supported by this server
    /// </summary>
    public string UserModes { get; set; } = "iwos";

    /// <summary>
    /// Channel modes supported by this server
    /// </summary>
    public string ChannelModes { get; set; } = "bklmntsiIpK";

    /// <summary>
    /// Channel modes that require a parameter
    /// </summary>
    public string ChannelModesWithParam { get; set; } = "bkloI";
}
