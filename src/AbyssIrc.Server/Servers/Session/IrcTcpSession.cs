using System.Text;
using NetCoreServer;
using Serilog;

namespace AbyssIrc.Server.Servers.Session;

public class IrcTcpSession : TcpSession
{
    private readonly ILogger _logger = Log.ForContext<IrcTcpSession>();

    private readonly IrcTcpServer _server;

    private string _endpoint;

    public IrcTcpSession(IrcTcpServer server) : base(server)
    {
        _server = server;
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        var message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);

        _logger.Debug("Received: {Message}", CleanMessage(message));
        _server.DispatchMessageAsync(Id.ToString(), message);

        base.OnReceived(buffer, offset, size);
    }

    private static string CleanMessage(string message)
    {
        return ">> " + message.Replace("\r", "-").Replace("\n", "-") + "<<";
    }

    protected override void OnConnected()
    {
        _endpoint = Socket.RemoteEndPoint.ToString();
        _logger.Debug("Connected Peer: {Peer}", Socket.RemoteEndPoint);
        _server.ClientConnected(Id.ToString(), _endpoint);
        base.OnConnected();
    }

    protected override void OnDisconnected()
    {
        _logger.Debug("Disconnected Peer: {Peer}", _endpoint);
        _server.ClientDisconnected(Id.ToString(), _endpoint);
        base.OnDisconnected();
    }

    public override long Send(string text)
    {
        _logger.Debug(
            "Sending to {Id}: {Text}",
            Id,
            text
        );
        return base.Send(text);
    }
}
