using System.Net;
using System.Net.Sockets;
using Serilog;

namespace AbyssIrc.Server.Network.Servers.Tcp;

/// <summary>
/// A server that listens for incoming connections from clients with span parser support
/// </summary>
public class MoongateTcpServer
{
    private readonly List<MoongateTcpClient> _clients = new();
    private readonly IPEndPoint _endPoint;
    private readonly ILogger _logger = Log.ForContext<MoongateTcpServer>();
    private SocketAsyncEventArgs? _acceptEventArgs;
    private Socket _listenSocket;

    public MoongateTcpServer(string id, IPEndPoint endPoint)
    {
        Id = id;
        _endPoint = endPoint;
        BufferSize = 8192;
    }

    public string Id { get; set; }

    /// <summary>
    /// The size of the buffer used for sending and receiving data.
    /// </summary>
    public int BufferSize { get; set; }

    public bool IsRunning { get; private set; }

    /// <summary>
    /// Event that is raised when a new client connects to the server.
    /// </summary>
    public event Action<MoongateTcpClient>? OnClientConnected;

    /// <summary>
    /// Event that is raised when a client disconnects from the server.
    /// </summary>
    public event Action<MoongateTcpClient>? OnClientDisconnected;

    /// <summary>
    /// Event that is raised when a client sends raw data to the server.
    /// </summary>
    public event Action<MoongateTcpClient, ReadOnlyMemory<byte>>? OnClientDataReceived;

    /// <summary>
    /// Event that is raised when a client sends parsed data to the server (after span parser processing).
    /// </summary>
    public event Action<MoongateTcpClient, ReadOnlyMemory<byte>>? OnClientSpanParsed;

    /// <summary>
    /// Event that is raised when an error occurs.
    /// </summary>
    public event Action<Exception>? OnError;

    /// <summary>
    /// Get all currently connected clients
    /// </summary>
    /// <returns>Read-only collection of connected clients</returns>
    public IReadOnlyList<MoongateTcpClient> GetClients()
    {
        lock (_clients)
        {
            return _clients.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Get a client by its ID
    /// </summary>
    /// <param name="clientId">The client ID to search for</param>
    /// <returns>The client if found, null otherwise</returns>
    public MoongateTcpClient? GetClient(string clientId)
    {
        lock (_clients)
        {
            return _clients.FirstOrDefault(c => c.Id == clientId);
        }
    }

    /// <summary>
    /// Broadcast data to all connected clients
    /// </summary>
    /// <param name="data">Data to broadcast</param>
    public void BroadcastToAll(ReadOnlySpan<byte> data)
    {
        lock (_clients)
        {
            foreach (var client in _clients.ToList())
            {
                try
                {
                    client.Send(data);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Failed to send data to client {ClientId}: {Error}", client.Id, ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Broadcast data to specific clients
    /// </summary>
    /// <param name="data">Data to broadcast</param>
    /// <param name="clientIds">Client IDs to send the data to</param>
    public void BroadcastToClients(ReadOnlySpan<byte> data, params string[] clientIds)
    {
        lock (_clients)
        {
            var targetClients = _clients.Where(c => clientIds.Contains(c.Id)).ToList();
            foreach (var client in targetClients)
            {
                try
                {
                    client.Send(data);
                }
                catch (Exception ex)
                {
                    _logger.Warning("Failed to send data to client {ClientId}: {Error}", client.Id, ex.Message);
                }
            }
        }
    }

    /// <summary>
    /// Disconnect a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to disconnect</param>
    /// <returns>True if the client was found and disconnected</returns>
    public bool DisconnectClient(string clientId)
    {
        var client = GetClient(clientId);
        if (client != null)
        {
            client.Disconnect();
            return true;
        }
        return false;
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
        IsRunning = false;

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
            var clientsCopy = _clients.ToList();
            foreach (var client in clientsCopy)
            {
                try
                {
                    client.Disconnect();
                }
                catch (Exception ex)
                {
                    _logger.Warning("Error disconnecting client {ClientId}: {Error}", client.Id, ex.Message);
                }
            }
            _clients.Clear();
        }

        _acceptEventArgs?.Dispose();
        _logger.Information("{Id} Server stopped", Id);
    }

    /// <summary>
    /// Start accepting a new connection.
    /// </summary>
    private void StartAccept()
    {
        if (!IsRunning) return;

        // Clear the accept socket as we're reusing the event args.
        _acceptEventArgs!.AcceptSocket = null;

        try
        {
            // Start an asynchronous accept operation.
            if (!_listenSocket.AcceptAsync(_acceptEventArgs))
            {
                // The operation completed synchronously.
                ProcessAccept(null, _acceptEventArgs);
            }
        }
        catch (ObjectDisposedException)
        {
            // This exception occurs when the socket is closed.
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }
    }

    /// <summary>
    /// Process the accept operation.
    /// </summary>
    /// <param name="sender">The sender object</param>
    /// <param name="e">The SocketAsyncEventArgs</param>
    private void ProcessAccept(object? sender, SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            // Retrieve the accepted socket.
            var acceptedSocket = e.AcceptSocket;

            // Create a new MoongateTcpClient using the accepted socket.
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
                acceptedSocket?.RemoteEndPoint,
                client.Id
            );

            // Wire up client events
            client.OnConnected += () => OnClientConnected?.Invoke(client);

            client.OnDisconnected += () =>
            {
                OnClientDisconnected?.Invoke(client);
                _logger.Information(
                    "{Id} Client disconnected: sessionId: {SessionId}",
                    Id,
                    client.Id
                );
                lock (_clients)
                {
                    _clients.Remove(client);
                }
            };

            // Wire up raw data received event
            client.OnDataReceived += data => OnClientDataReceived?.Invoke(client, data);

            // Wire up parsed data received event (NEW)
            client.OnSpanParsed += data => OnClientSpanParsed?.Invoke(client, data);

            // Wire up error event
            client.OnError += OnError;

            // Connect the client using the accepted socket
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
