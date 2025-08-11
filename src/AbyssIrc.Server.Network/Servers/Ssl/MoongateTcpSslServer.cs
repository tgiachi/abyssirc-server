using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using AbyssIrc.Server.Network.Servers.Tcp;
using Serilog;

namespace AbyssIrc.Server.Network.Servers.Ssl;

/// <summary>
/// A SSL/TLS-enabled server that listens for incoming secure connections from clients
/// </summary>
public class MoongateTcpSslServer
{
    private readonly List<MoongateTcpSslClient> _clients = new();
    private readonly IPEndPoint _endPoint;
    private readonly ILogger _logger = Log.ForContext<MoongateTcpSslServer>();
    private readonly X509Certificate _serverCertificate;
    private readonly SslServerOptions _sslOptions;

    private Socket? _listenSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _acceptTask;

    public MoongateTcpSslServer(string id, IPEndPoint endPoint, X509Certificate serverCertificate, SslServerOptions? sslOptions = null)
    {
        Id = id;
        _endPoint = endPoint;
        _serverCertificate = serverCertificate ?? throw new ArgumentNullException(nameof(serverCertificate));
        _sslOptions = sslOptions ?? new SslServerOptions();
        BufferSize = 8192;
    }

    public string Id { get; set; }

    /// <summary>
    /// The size of the buffer used for sending and receiving data.
    /// </summary>
    public int BufferSize { get; set; }

    public bool IsRunning { get; private set; }

    /// <summary>
    /// Event that is raised when a new client connects to the server and SSL handshake is complete.
    /// </summary>
    public event Action<MoongateTcpSslClient>? OnClientConnected;

    /// <summary>
    /// Event that is raised when a client disconnects from the server.
    /// </summary>
    public event Action<MoongateTcpSslClient>? OnClientDisconnected;

    /// <summary>
    /// Event that is raised when a client sends raw data to the server.
    /// </summary>
    public event Action<MoongateTcpSslClient, ReadOnlyMemory<byte>>? OnClientDataReceived;

    /// <summary>
    /// Event that is raised when a client sends parsed data to the server (after span parser processing).
    /// </summary>
    public event Action<MoongateTcpSslClient, ReadOnlyMemory<byte>>? OnClientSpanParsed;

    /// <summary>
    /// Event that is raised when an SSL-related error occurs.
    /// </summary>
    public event Action<MoongateTcpSslClient, Exception>? OnClientSslError;

    /// <summary>
    /// Event that is raised when a general error occurs.
    /// </summary>
    public event Action<Exception>? OnError;

    /// <summary>
    /// Get all currently connected clients
    /// </summary>
    /// <returns>Read-only collection of connected clients</returns>
    public IReadOnlyList<MoongateTcpSslClient> GetClients()
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
    public MoongateTcpSslClient? GetClient(string clientId)
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
    public async Task BroadcastToAllAsync(ReadOnlyMemory<byte> data)
    {
        List<MoongateTcpSslClient> clientsCopy;
        lock (_clients)
        {
            clientsCopy = _clients.ToList();
        }

        var tasks = clientsCopy.Select(async client =>
        {
            try
            {
                await client.SendAsync(data);
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to send data to SSL client {ClientId}: {Error}", client.Id, ex.Message);
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Broadcast data to specific clients
    /// </summary>
    /// <param name="data">Data to broadcast</param>
    /// <param name="clientIds">Client IDs to send the data to</param>
    public async Task BroadcastToClientsAsync(ReadOnlyMemory<byte> data, params string[] clientIds)
    {
        List<MoongateTcpSslClient> targetClients;
        lock (_clients)
        {
            targetClients = _clients.Where(c => clientIds.Contains(c.Id)).ToList();
        }

        var tasks = targetClients.Select(async client =>
        {
            try
            {
                await client.SendAsync(data);
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to send data to SSL client {ClientId}: {Error}", client.Id, ex.Message);
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Disconnect a specific client
    /// </summary>
    /// <param name="clientId">The ID of the client to disconnect</param>
    /// <returns>True if the client was found and disconnected</returns>
    public async Task<bool> DisconnectClientAsync(string clientId)
    {
        var client = GetClient(clientId);
        if (client != null)
        {
            await client.DisconnectAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Starts the SSL server and begins listening for incoming connections.
    /// </summary>
    public void Start()
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Server is already running");
        }

        try
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_endPoint);
            _listenSocket.Listen(100);

            _cancellationTokenSource = new CancellationTokenSource();
            _acceptTask = AcceptClientsAsync(_cancellationTokenSource.Token);

            IsRunning = true;
            _logger.Information("{Id} SSL Server listening on {EndPoint} with certificate {Subject}",
                Id, _endPoint, _serverCertificate.Subject);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start SSL server {Id}", Id);
            Stop();
            throw;
        }
    }

    /// <summary>
    /// Stops the SSL server from listening for new connections.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning) return;

        IsRunning = false;

        try
        {
            // Cancel accepting new connections
            _cancellationTokenSource?.Cancel();

            // Close the listen socket
            _listenSocket?.Close();

            // Wait for accept task to complete
            if (_acceptTask != null)
            {
                try
                {
                    _acceptTask.Wait(TimeSpan.FromSeconds(5));
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Error waiting for accept task to complete");
                }
            }

            // Disconnect all clients
            List<MoongateTcpSslClient> clientsCopy;
            lock (_clients)
            {
                clientsCopy = _clients.ToList();
                _clients.Clear();
            }

            var disconnectTasks = clientsCopy.Select(async client =>
            {
                try
                {
                    await client.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.Warning("Error disconnecting SSL client {ClientId}: {Error}", client.Id, ex.Message);
                }
            });

            Task.WaitAll(disconnectTasks.ToArray(), TimeSpan.FromSeconds(10));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error stopping SSL server {Id}", Id);
            OnError?.Invoke(ex);
        }
        finally
        {
            _listenSocket?.Dispose();
            _cancellationTokenSource?.Dispose();
            _logger.Information("{Id} SSL Server stopped", Id);
        }
    }

    /// <summary>
    /// Continuously accept new client connections
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task AcceptClientsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsRunning)
        {
            try
            {
                var tcpClient = await _listenSocket!.AcceptAsync();

                // Handle the new connection asynchronously
                _ = Task.Run(async () => await HandleNewConnectionAsync(tcpClient), cancellationToken);
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
                // Expected when stopping the server
                break;
            }
            catch (ObjectDisposedException)
            {
                // Expected when stopping the server
                break;
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    _logger.Error(ex, "Error accepting client connection on SSL server {Id}", Id);
                    OnError?.Invoke(ex);
                }
            }
        }
    }

    /// <summary>
    /// Handle a new client connection and perform SSL handshake
    /// </summary>
    /// <param name="socket">The accepted TCP socket</param>
    private async Task HandleNewConnectionAsync(Socket socket)
    {
        MoongateTcpSslClient? client = null;

        try
        {
            // Create a new SSL client
            client = new MoongateTcpSslClient
            {
                ServerId = Id
            };

            _logger.Information("{Id} New SSL connection from {RemoteEndPoint}, assigning session ID {SessionId}",
                Id, socket.RemoteEndPoint, client.Id);

            // Wire up client events before connecting
            client.OnConnected += () =>
            {
                lock (_clients)
                {
                    _clients.Add(client);
                }

                _logger.Information("{Id} SSL handshake completed for client {ClientId}", Id, client.Id);
                OnClientConnected?.Invoke(client);
            };

            client.OnDisconnected += () =>
            {
                lock (_clients)
                {
                    _clients.Remove(client);
                }

                _logger.Information("{Id} SSL client {ClientId} disconnected", Id, client.Id);
                OnClientDisconnected?.Invoke(client);
            };

            // Wire up data events
            client.OnDataReceived += data => OnClientDataReceived?.Invoke(client, data);
            client.OnSpanParsed += data => OnClientSpanParsed?.Invoke(client, data);

            // Wire up error events
            client.OnSslError += ex =>
            {
                _logger.Warning(ex, "SSL error for client {ClientId}", client.Id);
                OnClientSslError?.Invoke(client, ex);
            };

            client.OnError += ex =>
            {
                _logger.Warning(ex, "General error for SSL client {ClientId}", client.Id);
                OnError?.Invoke(ex);
            };

            // Perform SSL handshake with timeout
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_sslOptions.HandshakeTimeoutSeconds));

            await client.ConnectAsync(
                socket,
                _serverCertificate,
                BufferSize,
                _sslOptions.RequireClientCertificate,
                _sslOptions.EnabledSslProtocols
            );

            _logger.Information("{Id} SSL client {ClientId} connected successfully using {Protocol}",
                Id, client.Id, client.SslProtocol);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{Id} Failed to establish SSL connection for client {ClientId}",
                Id, client?.Id ?? "unknown");

            // Clean up on failure
            if (client != null)
            {
                try
                {
                    await client.DisconnectAsync();
                }
                catch (Exception cleanupEx)
                {
                    _logger.Warning(cleanupEx, "Error cleaning up failed SSL connection");
                }
            }
            else
            {
                // Close the raw socket if client creation failed
                try
                {
                    socket.Close();
                }
                catch (Exception socketEx)
                {
                    _logger.Warning(socketEx, "Error closing socket after failed SSL setup");
                }
            }
        }
    }
}

/// <summary>
/// Options for configuring SSL server behavior
/// </summary>
public class SslServerOptions
{
    /// <summary>
    /// Whether to require client certificates for mutual authentication
    /// </summary>
    public bool RequireClientCertificate { get; set; } = false;

    /// <summary>
    /// SSL/TLS protocols to enable
    /// </summary>
    public SslProtocols EnabledSslProtocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls13;

    /// <summary>
    /// Timeout for SSL handshake in seconds
    /// </summary>
    public int HandshakeTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to check certificate revocation
    /// </summary>
    public bool CheckCertificateRevocation { get; set; } = false;

    /// <summary>
    /// Allowed cipher suites (null for default)
    /// </summary>
    public CipherSuitesPolicy? CipherSuitesPolicy { get; set; } = null;
}
