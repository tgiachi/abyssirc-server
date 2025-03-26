using AbyssIrc.Server.Data.Internal;

namespace AbyssIrc.Server.Interfaces.Services.System;

public interface ISessionManagerService
{
    void AddSession(string id, string ipEndpoint, IrcSession? session = null);

    IrcSession GetSession(string id);

    List<IrcSession> GetSessions();
}
