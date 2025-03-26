using AbyssIrc.Core.Data.Internal;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Server.Interfaces.Listener;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Extensions;

public static class RegisterIrcHandlerExtension
{
    public static IServiceCollection RegisterIrcHandler(this IServiceCollection services, Type handlerType, Type messageType)
    {
        services.AddSingleton(handlerType);

        services.AddToRegisterTypedList(new IrcHandlerDefinitionData(handlerType, messageType));

        return services;
    }

    public static IServiceCollection RegisterIrcHandler<THandler, TMessageType>(this IServiceCollection services)
        where THandler : IIrcMessageListener where TMessageType : IIrcCommand
    {
        return RegisterIrcHandler(services, typeof(THandler), typeof(TMessageType));
    }
}
