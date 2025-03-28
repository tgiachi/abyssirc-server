namespace AbyssIrc.Core.Data.Configs.Sections;

public class LimitConfig
{
    /// <summary>
    ///  The SILENCE parameter indicates the maximum number of entries a client can have in their silence list.
    /// </summary>
    public int MaxSilence { get; set; } = 16;


    /// <summary>
    /// The MODES parameter specifies how many ‘variable’ modes may be set on a channel by a single MODE command from a client.
    /// A ‘variable’ mode is defined as being a type A, B or C mode as defined in the CHANMODES parameter, or in the
    /// </summary>
    public int MaxModes { get; set; } = 6;

    /// <summary>
    /// Maximum length of a user's away message
    /// </summary>
    public int MaxAwayLength { get; set; } = 200;

    /// <summary>
    ///  Case mapping for nicknames and channels
    /// </summary>
    public string CaseMapping { get; set; } = "rfc1459";

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


    /// <summary>
    ///  Maximum number of channels a user can join at once
    /// </summary>
    public int MaxChanJoin { get; set; } = 25;

    /// <summary>
    ///  The STATUSMSG parameter indicates that the server supports a method for clients to send a
    ///  message via the PRIVMSG / NOTICE commands to those people on a channel with (one of) the specified channel membership prefixes.
    /// </summary>
    public string StatusMsg { get; set; } = "@+";

    /// <summary>
    ///  The ELIST parameter indicates that the server supports search extensions to the LIST command.
    /// </summary>
    public string Elist { get; set; } = "MNUCT";
}
