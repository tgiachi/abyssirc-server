using AbyssIrc.Core.Extensions;
using AbyssIrc.Server.Data.Internal.ServiceCollection;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Extensions;

public static class RegisterIrcHandlerExtension
{
    public static IServiceCollection RegisterIrcHandler(this IServiceCollection services, Type handlerType)
    {
        services.AddToRegisterTypedList(new IrcHandlerDefinitionData(handlerType));

        services.AddSingleton(handlerType);

        return services;
    }

    public static IServiceCollection RegisterIrcHandler<THandler>(this IServiceCollection services)
        where THandler : class
    {
        services.AddToRegisterTypedList(new IrcHandlerDefinitionData(typeof(THandler)));

        services.AddSingleton<THandler>();

        return services;
    }
}
