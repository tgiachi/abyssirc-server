using AbyssIrc.Server.Core.Attributes.Scripts;
using AbyssIrc.Server.Core.Data.Sessions;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using AbyssIrc.Server.Core.Interfaces.Services.System;
using Serilog;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("irc_manager")]
public class IrcManagerModule
{
    private readonly IIrcManagerService _ircManagerService;

    private readonly ISessionManagerService _sessionManagerService;

    public IrcManagerModule(IIrcManagerService ircManagerService, ISessionManagerService sessionManagerService)
    {
        _ircManagerService = ircManagerService;
        _sessionManagerService = sessionManagerService;
    }

    [ScriptFunction("Hook into a command")]
    public void HookCommand(string commandCode, Action<string, object> callback)
    {
        _ircManagerService.RegisterListener(
            commandCode,
            (id, command) =>
            {
                callback(id, command);
                return Task.CompletedTask;
            }
        );
    }


    [ScriptFunction("Get session by id")]
    public IrcSession GetSession(string id)
    {
        return _sessionManagerService.GetSession(id);
    }


    [ScriptFunction("Send NOTICE to a session or channel")]
    public void SendNotice(string nicknameOrChannel, string message)
    {
        if (nicknameOrChannel.StartsWith('@') || nicknameOrChannel.StartsWith('#'))
        {
            return;
            //nicknameOrChannel = nicknameOrChannel[1..];
        }

        var session = _sessionManagerService.GetSessions().Where(s => s.IsRegistered && s.Nickname != null)
            .FirstOrDefault(s => s.Nickname.ToLower() == nicknameOrChannel.ToLower());

        if (session == null)
        {
            Log.Logger.Error($"No such session: {nicknameOrChannel}");
            return;
        }

        _ircManagerService.SendNoticeMessageAsync(session.Id, session.Nickname, message);
    }
}
