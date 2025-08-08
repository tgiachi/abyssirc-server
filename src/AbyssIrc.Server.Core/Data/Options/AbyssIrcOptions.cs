using AbyssIrc.Server.Core.Types.Logger;

namespace AbyssIrc.Server.Core.Data.Options;

public class AbyssIrcOptions
{
    public string Id { get; set; }

    public string RootDirectory { get; set; }

    public int[] SecurePorts { get; set; } = [];

    public int[] NonSecurePorts { get; set; } = [];

    public string CertificatePath { get; set; }

    public string CertificatePassword { get; set; }

    public string Config { get; set; }

    public bool LogToFile { get; set; }

    public bool LogToConsole { get; set; }

    public LogLevelType  LogLevel { get; set; }
}
