namespace AbyssIrc.Network.Types;

/// <summary>
/// Defines possible filter types for LIST command
/// </summary>
public enum ListFilterType
{
    /// <summary>
    /// Filter by number of users
    /// </summary>
    Users,

    /// <summary>
    /// Filter by channel creation time
    /// </summary>
    Created,

    /// <summary>
    /// Filter by topic last changed time
    /// </summary>
    TopicChanged
}
