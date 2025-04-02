namespace AbyssIrc.Server.Core.Data.Configs.Sections;

public class NetworkConfig
{
    public string Host { get; set; } = "irc.abyssirc.com";
    public string Ports { get; set; } = "6667";

    public string SslPorts { get; set; } = "6697";

    public string SslCertPath { get; set; }

    public string SslCertPassword { get; set; }

    /// <summary>
    /// The ping timeout in seconds
    /// </summary>
    public int PingTimeout { get; set; } = 180;

    /// <summary>
    /// The ping interval in seconds
    /// </summary>
    public int PingInterval { get; set; } = 60;
}
