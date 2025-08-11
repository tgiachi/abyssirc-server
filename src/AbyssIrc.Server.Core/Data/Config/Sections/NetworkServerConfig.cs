using System.Net;

namespace AbyssIrc.Server.Core.Data.Config.Sections;

public class NetworkServerConfig
{
    public List<NetworkBindConfig> Binds { get; set; }


    public NetworkServerConfig()
    {
        Binds = new List<NetworkBindConfig>()
        {
            new()
            {
                Host = IPAddress.Any,
                Ports = [6667, 6666],
            }
        };
    }
}
