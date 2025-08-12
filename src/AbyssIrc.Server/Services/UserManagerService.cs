using AbyssIrc.Server.Core.Interfaces.Services;
using Serilog;

namespace AbyssIrc.Server.Services;

public class UserManagerService : IUserManagerService
{
    public readonly INetworkService _networkService;

    private readonly ILogger _logger = Log.ForContext<UserManagerService>();

    public UserManagerService(INetworkService networkService)
    {
        _networkService = networkService;
        networkService.OnSessionConnected += NetworkServiceOnOnSessionConnected;
        networkService.OnSessionDisconnected += NetworkServiceOnOnSessionDisconnected;
    }

    private void NetworkServiceOnOnSessionDisconnected(string sessionId)
    {
    }

    private void NetworkServiceOnOnSessionConnected(string sessionId)
    {
    }

    public async Task<bool> UserExists(string username)
    {
        return _networkService.Sessions.Any(s => s.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task<bool> NicknameExists(string nickname)
    {
        return _networkService.Sessions.Any(s => s.Nickname.Equals(nickname, StringComparison.InvariantCultureIgnoreCase));
    }
}
