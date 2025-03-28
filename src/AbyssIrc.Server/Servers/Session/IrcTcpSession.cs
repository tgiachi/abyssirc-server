using System.Net.Sockets;
using System.Text;
using AbyssIrc.Server.Servers.Utils;
using NetCoreServer;
using Serilog;

namespace AbyssIrc.Server.Servers.Session;

public class IrcTcpSession : TcpSession
{
    private readonly ILogger _logger = Log.ForContext<IrcTcpSession>();

    private readonly IrcTcpServer _server;

    private readonly IrcMessageFramer _messageFramer = new();

    private string _endpoint;

    public IrcTcpSession(IrcTcpServer server) : base(server)
    {
        _server = server;
    }

    protected override async void OnReceived(byte[] buffer, long offset, long size)
    {
        try
        {
            // Add received data to the framer without creating strings prematurely
            _messageFramer.Append(new ReadOnlySpan<byte>(buffer, (int)offset, (int)size));

            // Process all complete messages
            foreach (var message in _messageFramer.GetCompletedMessages())
            {
                // Now we only create strings for complete messages
                await _server.DispatchMessageAsync(Id.ToString(), message);
            }

            base.OnReceived(buffer, offset, size);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing incoming IRC data for session {SessionId}", Id);
        }
    }


    protected override void OnConnecting()
    {
        _endpoint = Socket.RemoteEndPoint.ToString();
        _logger.Debug("Connected Peer: {Peer}", Socket.RemoteEndPoint);
        _server.ClientConnected(Id.ToString(), _endpoint);
        base.OnConnecting();
    }

    protected override void OnDisconnected()
    {
        _logger.Debug("Disconnected Peer: {Peer}", _endpoint);
        _server.ClientDisconnected(Id.ToString(), _endpoint);
        base.OnDisconnected();
    }

    protected override void OnError(SocketError error)
    {
        _logger.Error("Session {Id} caught an error: {Error}", Id, error);
        base.OnError(error);
    }
}
