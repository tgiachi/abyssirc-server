namespace AbyssIrc.Protocol.Messages.Types;

/// <summary>
///     Represents a single mode change
/// </summary>
public class ModeChangeType
{
    public ModeChangeType(bool isAdding, char mode, string parameter = null)
    {
        IsAdding = isAdding;
        Mode = mode;
        Parameter = parameter;
    }

    public ModeChangeType()
    {
    }

    /// <summary>
    ///     Whether the mode is being added (+) or removed (-)
    /// </summary>
    public bool IsAdding { get; set; }

    /// <summary>
    ///     The mode character
    /// </summary>
    public char Mode { get; set; }

    /// <summary>
    ///     Optional parameter for the mode (e.g., for ban masks, user to op)
    /// </summary>
    public string Parameter { get; set; }

    public override string ToString()
    {
        return $"{(IsAdding ? '+' : '-')}{Mode}" + (string.IsNullOrEmpty(Parameter) ? "" : $" {Parameter}");
    }
}
