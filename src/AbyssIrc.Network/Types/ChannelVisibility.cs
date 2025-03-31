namespace AbyssIrc.Network.Types;

/// <summary>
/// The possible types of channel visibility for NAMES replies
/// </summary>
public enum ChannelVisibility
{
    /// <summary>
    /// Public channel (=)
    /// </summary>
    Public,

    /// <summary>
    /// Secret channel (@)
    /// </summary>
    Secret,

    /// <summary>
    /// Private channel (*)
    /// DEPRECATED: Use Secret instead
    /// </summary>
    Private
}
