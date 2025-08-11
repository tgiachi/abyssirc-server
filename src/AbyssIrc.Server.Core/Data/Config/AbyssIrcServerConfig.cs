using AbyssIrc.Server.Core.Data.Config.Sections;

namespace AbyssIrc.Server.Core.Data.Config;

public class AbyssIrcServerConfig
{
    public HostServerConfig Server { get; set; } = new();
    public NetworkServerConfig Network { get; set; } = new();
    public SslServerConfig Ssl { get; set; } = new();
}
