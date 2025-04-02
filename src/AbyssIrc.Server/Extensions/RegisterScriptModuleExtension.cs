using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Core.Data.Internal.Scripts;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Extensions;

public static class RegisterScriptModuleExtension
{
    public static IServiceCollection RegisterScriptModule(this IServiceCollection services, Type moduleType)
    {
        services.AddSingleton(moduleType);
        services.AddToRegisterTypedList(new ScriptModuleData(moduleType));
        return services;
    }

    public static IServiceCollection RegisterScriptModule<TModule>(this IServiceCollection services)
    {
        return services.RegisterScriptModule(typeof(TModule));
    }
}
