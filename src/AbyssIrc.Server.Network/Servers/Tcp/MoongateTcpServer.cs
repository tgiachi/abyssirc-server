using System.Net;
using System.Net.Sockets;
using Serilog;

namespace AbyssIrc.Server.Network.Servers.Tcp;

/// <summary>
/// A server that listens for incoming connections from clients.
/// </summary>
public class MoongateTcpServer
{
    private readonly ILogger _logger = Log.ForContext<MoongateTcpServer>();

    public string Id { get; set; }

    /// <summary>
    /// Event that is raised when a new client connects to the server.
    /// </summary>
    public event Action<MoongateTcpClient> OnClientConnected;

    /// <summary>
    /// Event that is raised when a client disconnects from the server.
    /// </summary>
    public event Action<MoongateTcpClient> OnClientDisconnected;

    /// <summary>
    /// Event that is raised when a client sends data to the server.
    /// </summary>
    public event Action<MoongateTcpClient, ReadOnlyMemory<byte>> OnClientDataReceived;

    /// <summary>
    /// Event that is raised when an error occurs.
    /// </summary>
    public event Action<Exception> OnError;

    /// <summary>
    /// The size of the buffer used for sending and receiving data.
    /// </summary>
    public int BufferSize { get; set; }

    public bool IsRunning { get; private set; }

    private Socket _listenSocket;
    private SocketAsyncEventArgs? _acceptEventArgs;


    private readonly List<MoongateTcpClient> _clients = new List<MoongateTcpClient>();
    private readonly IPEndPoint _endPoint;

    public MoongateTcpServer(string id, IPEndPoint endPoint)

    {
        Id = id;
        _endPoint = endPoint;
        BufferSize = 8192;
    }


    /// <summary>
    /// Starts the server and begins listening for incoming connections.
    /// </summary>
    public void Start()
    {
        _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.Bind(_endPoint);
        _listenSocket.Listen(100);

        // Set up the SocketAsyncEventArgs for accepting connections.
        _acceptEventArgs = new SocketAsyncEventArgs();
        _acceptEventArgs.Completed += ProcessAccept;


        // Start the first accept operation.
        StartAccept();

        _logger.Information("{Id} Listening on {EndPoint}", Id, _endPoint);

        IsRunning = true;
    }

    /// <summary>
    /// Stops the server from listening for new connections.
    /// </summary>
    public void Stop()
    {
        try
        {
            _listenSocket?.Close();
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }

        lock (_clients)
        {
            // Stop all clients, make a copy of the list to avoid modifying it while iterating.
            foreach (var client in _clients.ToArray())
            {
                client.Disconnect();
            }

            _clients.Clear();
        }

        _listenSocket = null;
        _acceptEventArgs = null;
        IsRunning = false;

        _logger.Information("{Id} Server stopped", Id);
    }

    private void StartAccept()
    {
        // If the accept socket is null, then the server is stopped.
        if (_acceptEventArgs == null)
        {
            return;
        }

        // Clear any previous accepted socket.
        _acceptEventArgs.AcceptSocket = null;

        // Start an asynchronous accept operation.
        try
        {
            if (!_listenSocket.AcceptAsync(_acceptEventArgs))
            {
                // If AcceptAsync returns false, then accept completed synchronously.
                ProcessAccept(null, _acceptEventArgs);
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore ObjectDisposedException. This exception occurs when the socket is closed.
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }
    }

    private void ProcessAccept(object sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            // Retrieve the accepted socket.
            Socket? acceptedSocket = e.AcceptSocket;

            // Create a new NetClient using the accepted socket.
            var client = new MoongateTcpClient
            {
                ServerId = Id
            };

            lock (_clients)
            {
                _clients.Add(client);
            }

            _logger.Information(
                "{Id} Client connected: {RemoteEndPoint} sessionId: {SessionId}",
                Id,
                acceptedSocket.RemoteEndPoint,
                client.Id
            );
            client.OnConnected += () => OnClientConnected?.Invoke(client);
            client.OnDisconnected += () =>
            {
                OnClientDisconnected?.Invoke(client);
                _logger.Information(
                    "{Id} Client disconnected:  sessionId: {SessionId}",
                    Id,
                    client.Id
                );
                lock (_clients)
                {
                    _clients.Remove(client);
                }
            };
            client.OnDataReceived += data => OnClientDataReceived?.Invoke(client, data);
            client.OnError += OnError;
            client.Connect(acceptedSocket, BufferSize);
        }
        // If the accept operation was canceled, then the server is stopped.
        else if (e.SocketError != SocketError.Interrupted && e.SocketError != SocketError.OperationAborted)
        {
            OnError?.Invoke(new SocketException((int)e.SocketError));
        }

        // Continue accepting the next connection.
        StartAccept();
    }
}
