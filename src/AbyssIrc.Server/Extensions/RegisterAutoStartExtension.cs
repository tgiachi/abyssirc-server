using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Data.Internal;
using AbyssIrc.Server.Data.Internal.ServiceCollection;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Extensions;

public static class RegisterAutoStartExtension
{
    public static IServiceCollection RegisterAutoStartService(
        this IServiceCollection services, Type interfaceType, Type implementationType
    )
    {
        services.AddSingleton(interfaceType, implementationType);

        services.AddToRegisterTypedList(new AutoStartDefinitionData(interfaceType, implementationType));

        return services;
    }

    public static IServiceCollection RegisterAutoStartService<TInterface, TImplementation>(
        this IServiceCollection services
    )
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddSingleton<TInterface, TImplementation>();

        services.AddToRegisterTypedList(new AutoStartDefinitionData(typeof(TInterface), typeof(TImplementation)));

        return services;
    }

    public static IServiceCollection RegisterAutoStartService<TImplementation>(
        this IServiceCollection services
    )
        where TImplementation : class
    {
        services.AddSingleton<TImplementation>();

        services.AddToRegisterTypedList(new AutoStartDefinitionData(typeof(TImplementation), typeof(TImplementation)));

        return services;
    }
}
