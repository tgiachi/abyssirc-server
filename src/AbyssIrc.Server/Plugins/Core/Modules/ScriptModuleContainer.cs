using AbyssIrc.Server.Core.Interfaces.Modules;
using AbyssIrc.Server.Extensions;
using AbyssIrc.Server.Modules.Scripts;

namespace AbyssIrc.Server.Plugins.Core.Modules;

public class ScriptModuleContainer : IAbyssContainerModule
{
    public IServiceCollection InitializeModule(IServiceCollection services)
    {
        return services
                .RegisterScriptModule<JsLoggerModule>()
                .RegisterScriptModule<EventsModule>()
                .RegisterScriptModule<SchedulerModule>()
                .RegisterScriptModule<IrcManagerModule>()
                .RegisterScriptModule<VariableModule>()
                .RegisterScriptModule<ChannelsModule>()
            ;
    }
}
