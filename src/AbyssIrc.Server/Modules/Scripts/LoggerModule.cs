using AbyssIrc.Core.Attributes.Scripts;
using HamLink.Core.Attributes.Scripts;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("logger")]
public class LoggerModule
{
    private readonly ILogger _logger;

    public LoggerModule(ILogger<LoggerModule> logger)
    {
        _logger = logger;
    }

    [ScriptFunction("Log an informational message")]
    public void Info(string message, params object[]? args)
    {
        _logger.LogInformation(message, args);
    }
}
