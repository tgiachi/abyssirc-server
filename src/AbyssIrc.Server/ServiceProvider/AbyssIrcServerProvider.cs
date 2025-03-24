using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Server.Interfaces;
using AbyssIrc.Server.Services;
using Jab;

namespace AbyssIrc.Server.ServiceProvider;

[ServiceProvider]
[Singleton<ITcpService, TcpService>]
[Singleton(typeof(DirectoriesConfig), Instance = nameof(DirectoriesConfig))]
[Singleton(typeof(AbyssIrcConfig), Instance = nameof(AbyssIrcConfig))]
public partial class AbyssIrcServerProvider
{
    public DirectoriesConfig DirectoriesConfig { get; set; }

    public AbyssIrcConfig AbyssIrcConfig { get; set; }
}
