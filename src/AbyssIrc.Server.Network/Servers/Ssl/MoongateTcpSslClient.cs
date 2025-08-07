using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using AbyssIrc.Server.Network.Buffers;
using AbyssIrc.Server.Network.Middleware;
using AbyssIrc.Server.Network.Parsers;
using NanoidDotNet;
using Serilog;

namespace AbyssIrc.Server.Network.Servers.Ssl;

/// <summary>
/// A SSL/TLS-enabled client for secure connections to remote hosts with span parser chain support
/// </summary>
public class MoongateTcpSslClient
{
    private readonly List<INetMiddleware> _middlewares = new();
    private readonly List<ISpanParser> _spanParsers = new();
    private readonly ILogger _logger = Log.ForContext<MoongateTcpSslClient>();

    private int _bufferSize;
    private Socket? _socket;
    private SslStream? _sslStream;
    private NetworkStream? _networkStream;
    private CircularBuffer<byte>? _receiveBuffer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _receiveTask;
    private readonly SemaphoreSlim _sendSemaphore = new(1, 1);

    /// <summary>
    /// Unique identifier for the server this client is connected to
    /// </summary>
    public string ServerId { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for the client
    /// </summary>
    public string Id { get; } = Nanoid.Generate();

    /// <summary>
    /// Whether the client is connected and SSL handshake is complete
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Whether the SSL handshake is complete
    /// </summary>
    public bool IsSslAuthenticated => _sslStream?.IsAuthenticated ?? false;

    /// <summary>
    /// SSL protocol used for the connection
    /// </summary>
    public SslProtocols SslProtocol => _sslStream?.SslProtocol ?? SslProtocols.None;

    /// <summary>
    /// Remote certificate information
    /// </summary>
    public X509Certificate? RemoteCertificate => _sslStream?.RemoteCertificate;

    /// <summary>
    /// Local certificate information
    /// </summary>
    public X509Certificate? LocalCertificate => _sslStream?.LocalCertificate;

    /// <summary>
    /// Remote host IP address
    /// </summary>
    public string? Ip { get; private set; }

    /// <summary>
    /// Gets the number of bytes currently available in the receive buffer
    /// </summary>
    public int AvailableBytes => _receiveBuffer?.Size ?? 0;

    /// <summary>
    /// Gets whether the receive buffer is full
    /// </summary>
    public bool IsReceiveBufferFull => _receiveBuffer?.IsFull ?? false;

    /// <summary>
    /// Event when the client is connected and SSL handshake is complete
    /// </summary>
    public event Action? OnConnected;

    /// <summary>
    /// Event when the client is disconnected
    /// </summary>
    public event Action? OnDisconnected;

    /// <summary>
    /// Event when raw data is received (before parsing)
    /// </summary>
    public event Action<ReadOnlyMemory<byte>>? OnDataReceived;

    /// <summary>
    /// Event when parsed data is received (after span parser chain processing)
    /// </summary>
    public event Action<ReadOnlyMemory<byte>>? OnSpanParsed;

    /// <summary>
    /// Event when an SSL-related error occurred
    /// </summary>
    public event Action<Exception>? OnSslError;

    /// <summary>
    /// Event when a general error occurred
    /// </summary>
    public event Action<Exception>? OnError;

    /// <summary>
    /// Add a span parser to the processing chain
    /// </summary>
    /// <param name="parser">The parser to add to the chain</param>
    public void AddSpanParser(ISpanParser parser)
    {
        _spanParsers.Add(parser);
    }

    /// <summary>
    /// Remove a span parser from the processing chain
    /// </summary>
    /// <param name="parser">The parser to remove</param>
    public void RemoveSpanParser(ISpanParser parser)
    {
        _spanParsers.Remove(parser);
    }

    /// <summary>
    /// Remove all span parsers of a specific type
    /// </summary>
    /// <typeparam name="T">The type of parser to remove</typeparam>
    public void RemoveSpanParser<T>() where T : ISpanParser
    {
        _spanParsers.RemoveAll(p => p is T);
    }

    /// <summary>
    /// Check if the client contains a specific span parser type
    /// </summary>
    /// <param name="type">The type of parser to check for</param>
    /// <returns>True if the parser exists in the chain</returns>
    public bool ContainsSpanParser(Type type)
    {
        return _spanParsers.Any(p => p.GetType() == type);
    }

    /// <summary>
    /// Get all span parsers in the processing chain
    /// </summary>
    /// <returns>Read-only collection of span parsers</returns>
    public IReadOnlyList<ISpanParser> GetSpanParsers() => _spanParsers.AsReadOnly();

    /// <summary>
    /// Clear all span parsers from the chain
    /// </summary>
    public void ClearSpanParsers()
    {
        foreach (var parser in _spanParsers)
        {
            parser.Reset();
        }
        _spanParsers.Clear();
    }

    /// <summary>
    /// Add a middleware to the client
    /// </summary>
    /// <param name="middleware">The middleware to add</param>
    public void AddMiddleware(INetMiddleware middleware)
    {
        _middlewares.Add(middleware);
    }

    /// <summary>
    /// Remove a middleware from the client
    /// </summary>
    /// <param name="middleware">The middleware to remove</param>
    public void RemoveMiddleware(INetMiddleware middleware)
    {
        _middlewares.Remove(middleware);
    }

    /// <summary>
    /// Connect to a remote SSL/TLS host as a client
    /// </summary>
    /// <param name="hostname">Hostname to connect to</param>
    /// <param name="port">Port to connect to</param>
    /// <param name="bufferSize">Buffer size for operations</param>
    /// <param name="clientCertificate">Optional client certificate for mutual authentication</param>
    /// <param name="serverCertificateValidationCallback">Optional custom certificate validation</param>
    /// <param name="enabledSslProtocols">SSL protocols to enable</param>
    public async Task ConnectAsync(
        string hostname,
        int port,
        int bufferSize = 8192,
        X509Certificate? clientCertificate = null,
        RemoteCertificateValidationCallback? serverCertificateValidationCallback = null,
        SslProtocols enabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        try
        {
            _bufferSize = bufferSize;

            // Create TCP socket
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            // Connect to remote host
            await _socket.ConnectAsync(hostname, port);
            Ip = ((IPEndPoint)_socket.RemoteEndPoint!).Address.ToString();

            // Create network stream
            _networkStream = new NetworkStream(_socket, true);

            // Create SSL stream
            _sslStream = new SslStream(
                _networkStream,
                false,
                serverCertificateValidationCallback ?? DefaultCertificateValidation
            );

            // Perform SSL handshake
            var clientCertificates = clientCertificate != null
                ? new X509CertificateCollection { clientCertificate }
                : new X509CertificateCollection();

            await _sslStream.AuthenticateAsClientAsync(hostname, clientCertificates, enabledSslProtocols, false);

            // Initialize receive buffer
            _receiveBuffer = new CircularBuffer<byte>(bufferSize * 4);

            IsConnected = true;
            _logger.Information("SSL client {Id} connected to {Hostname}:{Port} using {Protocol}",
                Id, hostname, port, _sslStream.SslProtocol);

            // Start receiving data
            _receiveTask = ReceiveDataAsync(_cancellationTokenSource.Token);

            OnConnected?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to connect SSL client {Id} to {Hostname}:{Port}", Id, hostname, port);
            await DisconnectAsync();
            OnSslError?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Connect using an existing socket (for server-side SSL connections)
    /// </summary>
    /// <param name="socket">The accepted socket</param>
    /// <param name="serverCertificate">Server certificate for SSL</param>
    /// <param name="bufferSize">Buffer size for operations</param>
    /// <param name="clientCertificateRequired">Whether to require client certificates</param>
    /// <param name="enabledSslProtocols">SSL protocols to enable</param>
    internal async Task ConnectAsync(
        Socket socket,
        X509Certificate serverCertificate,
        int bufferSize = 8192,
        bool clientCertificateRequired = false,
        SslProtocols enabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        try
        {
            _bufferSize = bufferSize;
            _socket = socket;
            Ip = ((IPEndPoint)socket.RemoteEndPoint!).Address.ToString();

            // Create network stream
            _networkStream = new NetworkStream(_socket, true);

            // Create SSL stream
            _sslStream = new SslStream(_networkStream, false);

            // Perform SSL handshake as server
            await _sslStream.AuthenticateAsServerAsync(
                serverCertificate,
                clientCertificateRequired,
                enabledSslProtocols,
                false);

            // Initialize receive buffer
            _receiveBuffer = new CircularBuffer<byte>(bufferSize * 4);

            IsConnected = true;
            _logger.Information("SSL server connection established for client {Id} from {Ip} using {Protocol}",
                Id, Ip, _sslStream.SslProtocol);

            // Start receiving data
            _receiveTask = ReceiveDataAsync(_cancellationTokenSource.Token);

            OnConnected?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to establish SSL server connection for client {Id}", Id);
            await DisconnectAsync();
            OnSslError?.Invoke(ex);
            throw;
        }
    }

    /// <summary>
    /// Send data securely over the SSL connection
    /// </summary>
    /// <param name="data">Data to send</param>
    public async Task SendAsync(ReadOnlyMemory<byte> data)
    {
        if (!IsConnected || _sslStream == null)
        {
            throw new InvalidOperationException("Not connected");
        }

        await _sendSemaphore.WaitAsync();
        try
        {
            await _sslStream.WriteAsync(data, _cancellationTokenSource.Token);
            await _sslStream.FlushAsync(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send data for SSL client {Id}", Id);
            OnError?.Invoke(ex);
            throw;
        }
        finally
        {
            _sendSemaphore.Release();
        }
    }

    /// <summary>
    /// Disconnect from the remote host
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        IsConnected = false;

        try
        {
            // Cancel receive operations
            await _cancellationTokenSource.CancelAsync();

            // Wait for receive task to complete
            if (_receiveTask != null)
            {
                try
                {
                    await _receiveTask;
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancelling
                }
            }

            // Reset all span parsers
            foreach (var parser in _spanParsers)
            {
                parser.Reset();
            }

            // Close SSL stream
            _sslStream?.Close();
            _networkStream?.Close();
            _socket?.Close();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Error during SSL client {Id} disconnect", Id);
        }
        finally
        {
            _sslStream?.Dispose();
            _networkStream?.Dispose();
            _socket?.Dispose();
            _receiveBuffer?.Clear();

            _sslStream = null;
            _networkStream = null;
            _socket = null;

            OnDisconnected?.Invoke();
            _logger.Information("SSL client {Id} disconnected", Id);
        }
    }

    /// <summary>
    /// Continuously receive data from the SSL stream
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    private async Task ReceiveDataAsync(CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected && _sslStream != null)
            {
                var bytesReceived = await _sslStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                if (bytesReceived == 0)
                {
                    // Connection closed by remote host
                    break;
                }

                // Add data to circular buffer with overflow protection
                var receivedSpan = new ReadOnlySpan<byte>(buffer, 0, bytesReceived);
                ProcessIncomingData(receivedSpan);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error receiving data for SSL client {Id}", Id);
            OnError?.Invoke(ex);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);

            if (IsConnected)
            {
                _ = Task.Run(async () => await DisconnectAsync());
            }
        }
    }

    /// <summary>
    /// Process incoming data through middleware and parser chains
    /// </summary>
    /// <param name="data">Received data</param>
    private void ProcessIncomingData(ReadOnlySpan<byte> data)
    {
        try
        {
            // Add data to circular buffer with overflow protection
            var bytesToAdd = data.Length;
            while (_receiveBuffer!.Size + bytesToAdd > _receiveBuffer.Capacity)
            {
                var bytesToRemove = Math.Min(1024, _receiveBuffer.Size);
                for (var i = 0; i < bytesToRemove; i++)
                {
                    _receiveBuffer.PopFront();
                }

                OnError?.Invoke(new InvalidOperationException($"Receive buffer overflow, removed {bytesToRemove} bytes"));
            }

            // Add received data to buffer
            for (var i = 0; i < data.Length; i++)
            {
                _receiveBuffer.PushBack(data[i]);
            }

            // Process the buffered data
            ProcessBufferedData();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error processing incoming data for SSL client {Id}", Id);
            OnError?.Invoke(ex);
        }
    }

    /// <summary>
    /// Process buffered data through middleware and span parser chains
    /// </summary>
    private async Task ProcessBufferedData()
    {
        while (!_receiveBuffer!.IsEmpty)
        {
            var currentData = _receiveBuffer.ToArray();
            ReadOnlyMemory<byte> processedData = currentData;
            var consumedBytes = 0;
            var shouldHalt = false;

            try
            {
                // Process through middlewares first
                foreach (var middleware in _middlewares.Reverse<INetMiddleware>())
                {
                    // TODO: Implement middleware processing based on your INetMiddleware interface
                    // This would depend on how your middleware interface is designed
                }

                if (shouldHalt)
                {
                    break;
                }

                // Process through span parser chain
                if (_spanParsers.Count > 0 && !processedData.IsEmpty)
                {
                    var currentSpan = processedData.Span;
                    var totalBytesConsumed = 0;

                    foreach (var parser in _spanParsers)
                    {
                        var bytesConsumed = parser.ProcessData(currentSpan, out var parserOutput);
                        totalBytesConsumed = Math.Max(totalBytesConsumed, bytesConsumed);

                        if (!parserOutput.IsEmpty)
                        {
                            processedData = parserOutput;
                        }
                    }

                    consumedBytes = Math.Max(consumedBytes, totalBytesConsumed);

                    // Trigger the parsed data event if we have processed data
                    if (!processedData.IsEmpty)
                    {
                        OnSpanParsed?.Invoke(processedData);
                    }
                }
                else
                {
                    // No span parsers - trigger raw data event
                    if (!processedData.IsEmpty)
                    {
                        OnDataReceived?.Invoke(processedData);
                    }
                    consumedBytes = Math.Max(consumedBytes, processedData.Length);
                }

                // Remove consumed bytes from the buffer
                for (var i = 0; i < consumedBytes && !_receiveBuffer.IsEmpty; i++)
                {
                    _receiveBuffer.PopFront();
                }

                // If no bytes were consumed, break to prevent infinite loop
                if (consumedBytes == 0)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing buffered data for SSL client {Id}", Id);
                OnError?.Invoke(ex);
                _receiveBuffer.Clear();
                break;
            }
        }
    }

    /// <summary>
    /// Default certificate validation callback
    /// </summary>
    /// <param name="sender">The object that initiated the validation</param>
    /// <param name="certificate">The certificate to validate</param>
    /// <param name="chain">The certificate chain</param>
    /// <param name="sslPolicyErrors">SSL policy errors</param>
    /// <returns>True if the certificate is valid</returns>
    private bool DefaultCertificateValidation(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        _logger.Warning("SSL certificate validation failed for client {Id}: {Errors}", Id, sslPolicyErrors);

        // In production, you should implement proper certificate validation
        // For development/testing, you might want to accept self-signed certificates
        return sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch ||
               sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors;
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    public void Dispose()
    {
        _ = Task.Run(async () => await DisconnectAsync());
        _cancellationTokenSource.Dispose();
        _sendSemaphore.Dispose();
    }
}
