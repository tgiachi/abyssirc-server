using AbyssIrc.Server.Core.Attributes.Scripts;
using Microsoft.Extensions.Logging;

namespace AbyssIrc.Server.Modules.Scripts;

[ScriptModule("logger")]
public class JsLoggerModule
{
    private readonly ILogger _logger;

    public JsLoggerModule(ILogger<JsLoggerModule> logger)
    {
        _logger = logger;
    }

    [ScriptFunction("Log an informational message")]
    public void Info(string message, params object[]? args)
    {
        _logger.LogInformation("[JS] " + message, args);
    }

    [ScriptFunction("Log a warning message")]
    public void Warn(string message, params object[]? args)
    {
        _logger.LogWarning("[JS] " + message, args);
    }

    [ScriptFunction("Log an error message")]
    public void Error(string message, params object[]? args)
    {
        _logger.LogError("[JS] " + message, args);
    }

    [ScriptFunction("Log a critical message")]
    public void Critical(string message, params object[]? args)
    {
        _logger.LogCritical("[JS] " + message, args);
    }

    [ScriptFunction("Log a debug message")]
    public void Debug(string message, params object[]? args)
    {
        _logger.LogDebug("[JS] " + message, args);
    }
}
