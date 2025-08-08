using NanoidDotNet;

namespace AbyssIrc.Server.Core.Data.Config.Sections;

public class HostServerConfig
{
    public string Id { get; set; } = Nanoid.Generate();
    public string Hostname { get; set; } = "irc.abyss.io";
    public string Network { get; set; }  = "AbyssIrc";
    public string Description { get; set; } = "AbyssIRC server";

}
