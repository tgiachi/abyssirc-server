using AbyssIrc.Core.Interfaces.Services;

namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface IUserManagerService : IAbyssService
{
    Task<bool> UserExists(string username);
    Task<bool> NicknameExists(string nickname);
}
