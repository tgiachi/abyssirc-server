namespace AbyssIrc.Server.Data.Internal;

public class IrcSession
{
    public string Id { get; set; }

    public string IpAddress { get; set; }

    public string HostName { get; set; }

    public int Port { get; set; }

    public DateTime LastPing { get; set; }
}
