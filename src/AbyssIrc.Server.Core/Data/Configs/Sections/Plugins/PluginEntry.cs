namespace AbyssIrc.Server.Core.Data.Configs.Sections.Plugins;

public class PluginEntry
{
    public string PluginId { get; set; }

    public bool IsEnabled { get; set; } = true;


    public override string ToString()
    {
        return PluginId + (IsEnabled ? " (enabled)" : " (disabled)");
    }
}
