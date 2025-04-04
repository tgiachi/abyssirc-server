namespace AbyssIrc.Server.Core.Data.Plugins;

public record AbyssPluginInfo(string Id, string Name, string Version, string Description, string Authors, string[] Dependencies);
