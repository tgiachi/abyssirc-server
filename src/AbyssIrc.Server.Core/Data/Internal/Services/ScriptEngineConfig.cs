using AbyssIrc.Server.Core.Types.Scripts;

namespace AbyssIrc.Server.Core.Data.Internal.Services;

public class ScriptEngineConfig
{
    public string DefinitionPath { get; set; }

    public ScriptNameConversion ScriptNameConversion { get; set; } = ScriptNameConversion.CamelCase;

    public List<string> InitScriptsFileNames { get; set; } = ["bootstrap.js", "main.js", "init.js"];
}
