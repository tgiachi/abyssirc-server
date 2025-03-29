namespace AbyssIrc.Network.Types;

/// <summary>
/// Represents a single mode change
/// </summary>
public class ModeChangeType
{
    /// <summary>
    /// Whether the mode is being added (+) or removed (-)
    /// </summary>
    public bool IsAdding { get; set; }

    /// <summary>
    /// The mode character
    /// </summary>
    public char Mode { get; set; }

    /// <summary>
    /// Optional parameter for the mode (e.g., for ban masks, user to op)
    /// </summary>
    public string Parameter { get; set; }
}
