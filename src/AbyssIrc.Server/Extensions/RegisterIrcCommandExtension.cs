using AbyssIrc.Core.Extensions;
using AbyssIrc.Protocol.Messages.Data.Internal;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace AbyssIrc.Server.Extensions;

public static class RegisterIrcCommandExtension
{
    public static IServiceCollection RegisterIrcCommand(this IServiceCollection services, IIrcCommand commandType)
    {
        services.AddToRegisterTypedList(new IrcCommandDefinitionData(commandType));

        return services;
    }
}
