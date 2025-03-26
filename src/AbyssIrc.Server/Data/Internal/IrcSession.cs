namespace AbyssIrc.Server.Data.Internal;

public class IrcSession
{
    public string Id { get; set; }

    public string IpAddress { get; set; }

    public string HostName { get; set; }

    public int Port { get; set; }

    public bool IsRegistered { get; set; }

    public DateTime LastPing { get; set; }

    public DateTime LastPong { get; set; }

    public string Username { get; set; }

    public string RealName { get; set; }

    public string Nickname { get; set; }
}
