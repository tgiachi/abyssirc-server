using AbyssIrc.Server.Core.Attributes.Scripts;
using Serilog;

namespace AbyssIrc.Server.Core.Modules;

[ScriptModule("logger")]
public class LoggerModule
{

    private readonly ILogger _logger = Log.ForContext<LoggerModule>();


    [ScriptFunction("Log info")]
    public void Info(string message, params object[]? args)
    {
        _logger.Information(message, args);
    }
}
