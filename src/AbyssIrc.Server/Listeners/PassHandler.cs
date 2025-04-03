using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Commands.Errors;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Server.Core.Interfaces.Listener;
using AbyssIrc.Server.Listeners.Base;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Listeners;

public class PassHandler : BaseHandler, IIrcMessageListener
{
    public PassHandler(ILogger<PassHandler> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
    }

    public async Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        var session = GetSession(id);
        if (string.IsNullOrEmpty(ServerConfig.Admin.ServerPassword))
        {
            Logger.LogInformation("No server password set, skipping pass command");
            await SendIrcMessageAsync(id, ErrAlreadyRegistered.Create(Hostname, session.Nickname));
            return;
        }

        if (command is PassCommand passCommand)
        {
            if (passCommand.Password == ServerConfig.Admin.ServerPassword)
            {
                session.IsPasswordSent = true;
                await SendIrcMessageAsync(id, passCommand);
                return;
            }

            if (session.IsPasswordSent)
            {
                await SendIrcMessageAsync(id, ErrAlreadyRegistered.Create(Hostname, session.Nickname));
            }

            if (string.IsNullOrEmpty(passCommand.Password))
            {
                await SendIrcMessageAsync(id, ErrNeedMoreParams.Create(Hostname, session.Nickname, passCommand.Code));
                return;
            }
        }
    }
}
