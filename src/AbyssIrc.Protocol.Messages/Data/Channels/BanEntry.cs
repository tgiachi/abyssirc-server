namespace AbyssIrc.Server.Data.Channels;

/// <summary>
///     Represents a ban, invite exception, or ban exception entry
/// </summary>
public class BanEntry
{
    public BanEntry(string mask, string setBy, DateTime setTime)
    {
        Mask = mask;
        SetBy = setBy;
        SetTime = setTime;
    }

    /// <summary>
    ///     The mask pattern
    /// </summary>
    public string Mask { get; }

    /// <summary>
    ///     Who set this entry
    /// </summary>
    public string SetBy { get; }

    /// <summary>
    ///     When the entry was set
    /// </summary>
    public DateTime SetTime { get; }
}
