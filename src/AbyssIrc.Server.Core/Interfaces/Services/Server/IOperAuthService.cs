using AbyssIrc.Server.Core.Data.Rest;

namespace AbyssIrc.Server.Core.Interfaces.Services.Server;

public interface IOperAuthService
{
    Task<bool> AuthenticateAsync(string userName, string password);


    Task<LoginResultData> AuthenticateOperAsync(string userName, string password);
}
