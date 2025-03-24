using System.Text;
using NetCoreServer;
using Serilog;

namespace AbyssIrc.Server.Servers.Session;

public class IrcTcpSession : TcpSession
{
    private readonly ILogger _logger = Log.ForContext<IrcTcpSession>();

    private string _endpoint;

    public IrcTcpSession(TcpServer server) : base(server)
    {
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
        _logger.Information("Received: {Message}", message);
        base.OnReceived(buffer, offset, size);
    }

    protected override void OnConnected()
    {
        _endpoint = Socket.RemoteEndPoint.ToString();
        _logger.Information("Connected Peer: {Peer}", Socket.RemoteEndPoint);
        base.OnConnected();
    }

    protected override void OnDisconnected()
    {
        _logger.Information("Disconnected Peer: {Peer}", _endpoint);
        base.OnDisconnected();
    }
}
