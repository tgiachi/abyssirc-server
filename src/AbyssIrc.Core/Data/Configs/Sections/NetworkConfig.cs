namespace AbyssIrc.Core.Data.Configs.Sections;

public class NetworkConfig
{
    public string Host { get; set; } = "irc.abyssirc.com";
    public string Ports { get; set; } = "6667";

    public string SslPorts { get; set; } = "6697";

    public string SslCertPath { get; set; }

    public string SslCertPassword { get; set; }
}
