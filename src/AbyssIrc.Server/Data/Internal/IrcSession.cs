using System.Collections.Concurrent;

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

    public bool IsAway { get; set; }

    public string AwayMessage { get; set; }

    public ConcurrentBag<string> Channels { get; set; } = new();


    public void AddChannel(string channel)
    {
        Channels.Add(channel);
    }

    public void RemoveChannel(string channel)
    {
        Channels.TryTake(out channel);
    }

    public bool IsInChannel(string channel)
    {
        return Channels.Contains(channel);
    }
}
