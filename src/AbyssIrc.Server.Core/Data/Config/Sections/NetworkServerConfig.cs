namespace AbyssIrc.Server.Core.Data.Config.Sections;

public class NetworkServerConfig
{
    public List<NetworkBindConfig> Binds { get; set; } = new();
}
