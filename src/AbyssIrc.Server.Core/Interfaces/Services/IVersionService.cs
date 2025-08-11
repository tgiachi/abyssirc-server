using AbyssIrc.Core.Interfaces.Services;
using AbyssIrc.Server.Core.Data.Version;

namespace AbyssIrc.Server.Core.Interfaces.Services;

public interface IVersionService : IAbyssService
{
    VersionInfoData GetVersionInfo();
}
