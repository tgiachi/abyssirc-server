using AbyssIrc.Core.Data.Configs.Sections;

namespace AbyssIrc.Core.Data.Configs;

public class AbyssIrcConfig
{
    public NetworkConfig Network { get; set; } = new();

    public AdminConfig Admin { get; set; } = new();



}
