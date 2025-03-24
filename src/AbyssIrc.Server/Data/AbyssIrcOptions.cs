using CommandLine;

namespace AbyssIrc.Server.Data;

public class AbyssIrcOptions
{
    [Option('r', "root", Required = false, HelpText = "Root directory for the server.")]
    public string RootDirectory { get; set; } = "";

    [Option('c', "config", Required = false, HelpText = "Configuration file for the server.")]
    public string ConfigFile { get; set; } = "config.json";



}
