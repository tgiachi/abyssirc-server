using AbyssIrc.Server.Core.Data.Configs.Sections.Plugins;

namespace AbyssIrc.Server.Core.Data.Configs.Sections;

public class PluginsConfig
{
    public List<PluginEntry> Entries { get; set; } = new();
}
