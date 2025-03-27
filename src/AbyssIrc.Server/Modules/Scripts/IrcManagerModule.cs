using AbyssIrc.Core.Attributes.Scripts;
using AbyssIrc.Server.Interfaces.Services.Server;
using HamLink.Core.Attributes.Scripts;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("irc_manager")]
public class IrcManagerModule
{
    private readonly IIrcManagerService _ircManagerService;

    public IrcManagerModule(IIrcManagerService ircManagerService)
    {
        _ircManagerService = ircManagerService;
    }

    [ScriptFunction]
    public void HookCommand(string commandCode, Action<string, object> callback)
    {
        _ircManagerService.RegisterListener(commandCode, (id, command) => { callback(id, command); return Task.CompletedTask; });
    }
}
