using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Server.Interfaces;
using Serilog;

namespace AbyssIrc.Server.Services;

public class TcpService : ITcpService
{
    private readonly AbyssIrcConfig _abyssIrcConfig;

    private readonly ILogger _logger = Log.ForContext<TcpService>();

    public TcpService(AbyssIrcConfig abyssIrcConfig)
    {
        _abyssIrcConfig = abyssIrcConfig;
    }
}
