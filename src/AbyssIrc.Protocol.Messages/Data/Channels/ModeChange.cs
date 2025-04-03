namespace AbyssIrc.Protocol.Messages.Data.Channels;

/// <summary>
/// Represents a mode change event
/// </summary>
public class ModeChange
{
    /// <summary>
    /// Whether the mode is being added (true) or removed (false)
    /// </summary>
    public bool IsAdded { get; }

    /// <summary>
    /// The mode character
    /// </summary>
    public char Mode { get; }

    /// <summary>
    /// Optional parameter for the mode
    /// </summary>
    public string Parameter { get; }

    public ModeChange(bool isAdded, char mode, string parameter = null)
    {
        IsAdded = isAdded;
        Mode = mode;
        Parameter = parameter;
    }

    /// <summary>
    /// Returns a string representation of this mode change (e.g., "+k password" or "-i")
    /// </summary>
    public override string ToString()
    {
        var prefix = IsAdded ? "+" : "-";
        return string.IsNullOrEmpty(Parameter)
            ? $"{prefix}{Mode}"
            : $"{prefix}{Mode} {Parameter}";
    }
}
