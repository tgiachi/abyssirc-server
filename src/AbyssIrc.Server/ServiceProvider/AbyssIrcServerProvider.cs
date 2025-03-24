using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Directories;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Network.Services;

using AbyssIrc.Server.Interfaces.Services;
using AbyssIrc.Server.Listeners;
using AbyssIrc.Server.Services;
using AbyssIrc.Signals.Data.Configs;
using AbyssIrc.Signals.Interfaces.Services;
using AbyssIrc.Signals.Services;
using Jab;

namespace AbyssIrc.Server.ServiceProvider;

[ServiceProvider]
[Singleton<ITcpService, TcpService>]
[Singleton<IAbyssIrcSignalEmitterService, AbyssIrcSignalEmitter>]
[Singleton<IIrcCommandParser, IrcCommandParser>]
[Singleton<IIrcManagerService, IrcManagerService>]
[Singleton(typeof(AbyssIrcSignalConfig), Instance = nameof(AbyssIrcSignalConfig))]
[Singleton(typeof(DirectoriesConfig), Instance = nameof(DirectoriesConfig))]
[Singleton(typeof(AbyssIrcConfig), Instance = nameof(AbyssIrcConfig))]
// Handlers

[Singleton<QuitMessageHandler>]
public partial class AbyssIrcServerProvider
{
    public DirectoriesConfig DirectoriesConfig { get; set; }

    public AbyssIrcConfig AbyssIrcConfig { get; set; }

    public AbyssIrcSignalConfig AbyssIrcSignalConfig { get; set; }
}
