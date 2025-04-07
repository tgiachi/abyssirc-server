using AbyssIrc.Protocol.Messages.Interfaces.Parser;
using AbyssIrc.Protocol.Messages.Services;
using AbyssIrc.Server.Core.Interfaces.Modules;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Services;
using AbyssIrc.Signals.Interfaces.Services;
using AbyssIrc.Signals.Services;

namespace AbyssIrc.Server.Plugins.Core.Modules;

public class CoreServiceContainerModule : IAbyssContainerModule
{
    public IServiceCollection InitializeModule(IServiceCollection services)
    {
        return services
                .RegisterAutoStartService<IAbyssSignalService, AbyssSignalService>()
                .RegisterAutoStartService<IOperAuthService, OperAuthService>()
                .RegisterAutoStartService<IIrcCommandParser, IrcCommandParser>()
                .RegisterAutoStartService<IIrcManagerService, IrcManagerService>()
                .RegisterAutoStartService<ISessionManagerService, SessionManagerService>()
                .RegisterAutoStartService<ITextTemplateService, TextTemplateService>()
                .RegisterAutoStartService<IStringMessageService, StringMessageService>()
                .RegisterAutoStartService<ISchedulerSystemService, SchedulerSystemService>()
                .RegisterAutoStartService<IScriptEngineService, ScriptEngineService>()
                .RegisterAutoStartService<IEventDispatcherService, EventDispatcherService>()
                .RegisterAutoStartService<IProcessQueueService, ProcessQueueService>()
                .RegisterAutoStartService<IChannelManagerService, ChannelManagerService>()
                .RegisterAutoStartService<IDiagnosticService, DiagnosticService>()
                .RegisterAutoStartService<ITcpService, TcpService>()
            ;
    }
}
