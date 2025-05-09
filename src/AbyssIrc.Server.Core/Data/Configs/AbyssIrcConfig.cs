using AbyssIrc.Server.Core.Data.Configs.Sections;

namespace AbyssIrc.Server.Core.Data.Configs;

public class AbyssIrcConfig
{
    public NetworkConfig Network { get; set; } = new();

    public AdminConfig Admin { get; set; } = new();

    public MotdConfig Motd { get; set; } = new();

    public LimitConfig Limits { get; set; } = new();

    public OperConfig Opers { get; set; } = new();

    public ProcessQueueConfig ProcessQueue { get; set; } = new();

    public WebServerConfig WebServer { get; set; } = new();

    public PluginsConfig Plugins { get; set; } = new();
}
