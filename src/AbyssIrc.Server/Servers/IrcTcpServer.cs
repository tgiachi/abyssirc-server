using System.Net;
using AbyssIrc.Server.Servers.Session;
using NetCoreServer;

namespace AbyssIrc.Server.Servers;

public class IrcTcpServer : TcpServer
{
    public IrcTcpServer(IPAddress address, int port) : base(address, port)
    {
    }

    protected override TcpSession CreateSession()
    {
        return new IrcTcpSession(this);
    }
}
