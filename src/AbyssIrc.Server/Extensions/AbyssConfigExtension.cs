using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Server.Data.Internal.Handlers;

namespace AbyssIrc.Server.Extensions;

public static class AbyssConfigExtension
{
    public static AbyssServerData ToServerData(this AbyssIrcConfig ircConfig)
    {
        return new AbyssServerData()
        {
            Hostname = ircConfig.Network.Host
        };
    }

}
