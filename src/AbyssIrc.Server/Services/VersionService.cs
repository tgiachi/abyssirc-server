using System.Reflection;
using AbyssIrc.Server.Core.Data.Version;
using AbyssIrc.Server.Core.Interfaces.Services;

namespace AbyssIrc.Server.Services;

public class VersionService : IVersionService
{
    public VersionService()
    {
        var versionInfo = GetVersionInfo();
    }

    public VersionInfoData GetVersionInfo()
    {
        var version = typeof(VersionService).Assembly.GetName().Version;

        var codename = Assembly.GetExecutingAssembly()
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attr => attr.Key == "Codename")
            ?.Value;

        return new VersionInfoData(
            "AbyssIrc",
            codename,
            version.ToString(),
            ThisAssembly.Git.Commit,
            ThisAssembly.Git.Branch,
            ThisAssembly.Git.CommitDate
        );
    }
}
