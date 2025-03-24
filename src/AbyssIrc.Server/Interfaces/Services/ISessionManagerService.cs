using AbyssIrc.Server.Data.Internal;

namespace AbyssIrc.Server.Interfaces.Services;

public interface ISessionManagerService
{

    IrcSession GetSession(string id);
}
