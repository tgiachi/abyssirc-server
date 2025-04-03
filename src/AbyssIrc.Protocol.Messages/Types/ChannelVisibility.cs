namespace AbyssIrc.Protocol.Messages.Types;

/// <summary>
/// The possible types of channel visibility for NAMES replies
/// </summary>
/// <summary>
/// Represents the channel visibility status
/// </summary>
public enum ChannelVisibility
{
    /// <summary>
    /// Public channel (=)
    /// </summary>
    Public = '=',

    /// <summary>
    /// Secret channel (@)
    /// </summary>
    Secret = '@',

    /// <summary>
    /// Private channel (*) - Deprecated
    /// </summary>
    Private = '*'
}
