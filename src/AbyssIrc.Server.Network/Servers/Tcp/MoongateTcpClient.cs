using System.Buffers;
using System.Net;
using System.Net.Sockets;
using AbyssIrc.Server.Network.Buffers;
using AbyssIrc.Server.Network.Middleware;
using AbyssIrc.Server.Network.Parsers;
using NanoidDotNet;

namespace AbyssIrc.Server.Network.Servers.Tcp;

/// <summary>
/// A client for connecting to a remote host with span parser chain support
/// </summary>
public class MoongateTcpClient
{
    private readonly List<INetMiddleware> _middlewares = new();
    private readonly List<ISpanParser> _spanParsers = new();
    private int _bufferSize;

    private SocketAsyncEventArgs? _receiveArg;
    private CircularBuffer<byte>? _receiveBuffer;
    private SocketAsyncEventArgs? _sendArg;
    private int _sending;
    private Socket? _socket;
    private byte[]? _tempReceiveBuffer;
    private byte[]? _tempSendBuffer;

    /// <summary>
    /// Unique identifier for the server this client is connected to
    /// </summary>
    public string ServerId { get; set; }

    /// <summary>
    /// Unique identifier for the client
    /// </summary>
    public string Id { get; } = Nanoid.Generate();

    /// <summary>
    /// Whether the client is connected
    /// </summary>
    public bool IsConnected { get; private set; }

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
    /// Event when the client is connected
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
    /// Event when an error occurred
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
    /// Check if the client contains a specific middleware type
    /// </summary>
    /// <param name="type">The type of middleware to check for</param>
    /// <returns>True if the middleware exists</returns>
    public bool ContainsMiddleware(Type type)
    {
        return _middlewares.Any(m => m.GetType() == type);
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
    /// Remove a middleware of a specific type from the client
    /// </summary>
    /// <typeparam name="T">The type of middleware to remove</typeparam>
    public void RemoveMiddleware<T>() where T : INetMiddleware
    {
        _middlewares.RemoveAll(m => m is T);
    }

    /// <summary>
    /// Peek at data in the buffer without consuming it
    /// </summary>
    /// <param name="count">Number of bytes to peek (0 for all available)</param>
    /// <returns>Array containing the peeked data</returns>
    public byte[] PeekData(int count = 0)
    {
        if (_receiveBuffer == null || _receiveBuffer.IsEmpty)
        {
            return [];
        }

        var bytesToPeek = count <= 0 ? _receiveBuffer.Size : Math.Min(count, _receiveBuffer.Size);
        var result = new byte[bytesToPeek];

        for (var i = 0; i < bytesToPeek; i++)
        {
            result[i] = _receiveBuffer[i];
        }

        return result;
    }

    /// <summary>
    /// Consume (remove) bytes from the front of the buffer
    /// </summary>
    /// <param name="count">Number of bytes to consume</param>
    public void ConsumeBytes(int count)
    {
        if (_receiveBuffer == null)
        {
            return;
        }

        var bytesToConsume = Math.Min(count, _receiveBuffer.Size);
        for (var i = 0; i < bytesToConsume; i++)
        {
            _receiveBuffer.PopFront();
        }
    }

    /// <summary>
    /// Connect to the remote host
    /// </summary>
    /// <param name="ip">IP address to connect to</param>
    /// <param name="port">Port to connect to</param>
    /// <param name="bufferSize">Buffer size for send/receive operations</param>
    /// <exception cref="InvalidOperationException">Thrown when already connected</exception>
    public void Connect(string ip, int port, int bufferSize = 1024)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        _bufferSize = bufferSize;
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            LingerState = new LingerOption(false, 0),
            ExclusiveAddressUse = true,
            NoDelay = true,
            Blocking = false,
            SendBufferSize = bufferSize,
            ReceiveBufferSize = bufferSize
        };

        _socket.Connect(ip, port);
        IsConnected = true;
        Ip = ((IPEndPoint)_socket.RemoteEndPoint!).Address.ToString();

        InitializeBuffers();
        SetupSocketArgs();

        OnConnected?.Invoke();

        if (!_socket.ReceiveAsync(_receiveArg))
        {
            Receive(_receiveArg);
        }
    }

    /// <summary>
    /// When a server accepts a connection, use this method to connect the client
    /// </summary>
    /// <param name="socket">The accepted socket</param>
    /// <param name="bufferSize">Buffer size for operations</param>
    /// <exception cref="InvalidOperationException">Thrown when already connected</exception>
    internal void Connect(Socket? socket, int bufferSize = 1024)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        _bufferSize = bufferSize;
        _socket = socket;
        IsConnected = true;
        Ip = ((IPEndPoint)socket.RemoteEndPoint!).Address.ToString();

        InitializeBuffers();
        SetupSocketArgs();

        OnConnected?.Invoke();

        if (!_socket.ReceiveAsync(_receiveArg))
        {
            Receive(_receiveArg);
        }
    }

    /// <summary>
    /// Disconnect from the remote host
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected) return;

        IsConnected = false;

        // Reset all span parsers
        foreach (var parser in _spanParsers)
        {
            parser.Reset();
        }

        try
        {
            _socket?.Shutdown(SocketShutdown.Both);
            _socket?.Close();
        }
        catch
        {
            // Ignore disconnect errors
        }
        finally
        {
            _socket = null;
            OnDisconnected?.Invoke();
        }

        // Clean up resources
        _receiveArg?.Dispose();
        _sendArg?.Dispose();
        _receiveBuffer?.Clear();

        if (_tempReceiveBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_tempReceiveBuffer);
            _tempReceiveBuffer = null;
        }

        if (_tempSendBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_tempSendBuffer);
            _tempSendBuffer = null;
        }
    }

    /// <summary>
    /// Send data to the remote host
    /// </summary>
    /// <param name="data">Data to send</param>
    public void Send(ReadOnlySpan<byte> data)
    {
        if (!IsConnected || _socket == null)
        {
            throw new InvalidOperationException("Not connected");
        }

        // Use thread-safe sending mechanism
        if (Interlocked.CompareExchange(ref _sending, 1, 0) == 0)
        {
            try
            {
                _tempSendBuffer = ArrayPool<byte>.Shared.Rent(data.Length);
                data.CopyTo(_tempSendBuffer);

                _sendArg!.SetBuffer(_tempSendBuffer, 0, data.Length);

                if (!_socket.SendAsync(_sendArg))
                {
                    ProcessSend(_sendArg);
                }
            }
            catch (Exception ex)
            {
                Interlocked.Exchange(ref _sending, 0);
                if (_tempSendBuffer != null)
                {
                    ArrayPool<byte>.Shared.Return(_tempSendBuffer);
                    _tempSendBuffer = null;
                }

                OnError?.Invoke(ex);
            }
        }
        else
        {
            OnError?.Invoke(new InvalidOperationException("Send operation already in progress"));
        }
    }

    // Private methods implementation (simplified for this example)
    private void InitializeBuffers()
    {
        _receiveBuffer = new CircularBuffer<byte>(_bufferSize * 4);
        _tempReceiveBuffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
    }

    private void SetupSocketArgs()
    {
        _receiveArg = new SocketAsyncEventArgs();
        _receiveArg.SetBuffer(_tempReceiveBuffer, 0, _bufferSize);
        _receiveArg.UserToken = this;
        _receiveArg.Completed += ProcessSocketOperation;

        _sendArg = new SocketAsyncEventArgs();
        _sendArg.UserToken = this;
        _sendArg.Completed += ProcessSocketOperation;
    }

    private static void ProcessSocketOperation(object? sender, SocketAsyncEventArgs args)
    {
        switch (args.LastOperation)
        {
            case SocketAsyncOperation.Send:
                ProcessSend(args);
                break;
            case SocketAsyncOperation.Receive:
                Receive(args);
                break;
        }
    }

    private static void ProcessSend(SocketAsyncEventArgs args)
    {
        var client = (MoongateTcpClient)args.UserToken!;

        // Return buffer
        if (client._tempSendBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(client._tempSendBuffer);
            client._tempSendBuffer = null;
        }

        args.SetBuffer(null, 0, 0);
        Interlocked.Exchange(ref client._sending, 0);

        if (args.SocketError != SocketError.Success)
        {
            client.OnError?.Invoke(new SocketException((int)args.SocketError));
        }
    }

    private static void Receive(SocketAsyncEventArgs args)
    {
        var client = (MoongateTcpClient)args.UserToken!;

        if (args is { BytesTransferred: > 0, SocketError: SocketError.Success })
        {
            try
            {
                ReadOnlySpan<byte> receivedData = new(args.Buffer, 0, args.BytesTransferred);

                // Add data to circular buffer with overflow protection
                var bytesToAdd = receivedData.Length;
                while (client._receiveBuffer!.Size + bytesToAdd > client._receiveBuffer.Capacity)
                {
                    var bytesToRemove = Math.Min(1024, client._receiveBuffer.Size);
                    for (var i = 0; i < bytesToRemove; i++)
                    {
                        client._receiveBuffer.PopFront();
                    }

                    client.OnError?.Invoke(
                        new InvalidOperationException($"Receive buffer overflow, removed {bytesToRemove} bytes")
                    );
                }

                // Add received data to buffer
                for (var i = 0; i < receivedData.Length; i++)
                {
                    client._receiveBuffer.PushBack(receivedData[i]);
                }

                // Process the received data
                ProcessReceivedData(client);
            }
            catch (Exception e)
            {
                client.OnError?.Invoke(e);
            }

            // Continue receiving
            if (client.IsConnected && client._socket != null)
            {
                if (!client._socket.ReceiveAsync(args))
                {
                    Receive(args);
                }
            }
        }
        else
        {
            // Connection closed or error
            client.Disconnect();
        }
    }

    /// <summary>
    /// Process received data through middleware and span parser chains
    /// </summary>
    /// <param name="client">The client instance</param>
    private static void ProcessReceivedData(MoongateTcpClient client)
    {
        while (!client._receiveBuffer!.IsEmpty)
        {
            var currentData = client._receiveBuffer.ToArray();
            ReadOnlyMemory<byte> processedData = currentData;
            var consumedBytes = 0;
            var shouldHalt = false;

            try
            {
                // Process through middlewares first
                for (var i = client._middlewares.Count - 1; i >= 0; i--)
                {
                    var middleware = client._middlewares[i];
                    // TODO: Implement middleware processing logic based on your INetMiddleware interface
                    // var (halt, consumed) = middleware.ProcessReceive(ref processedData);
                    // if (halt)
                    // {
                    //     shouldHalt = true;
                    //     break;
                    // }
                    // consumedBytes += consumed;
                }

                if (shouldHalt)
                {
                    break;
                }

                // Process through span parser chain
                if (client._spanParsers.Count > 0 && !processedData.IsEmpty)
                {
                    var currentSpan = processedData.Span;
                    var totalBytesConsumed = 0;

                    foreach (var parser in client._spanParsers)
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
                        client.OnSpanParsed?.Invoke(processedData);
                    }
                }
                else
                {
                    // No span parsers - trigger raw data event
                    if (!processedData.IsEmpty)
                    {
                        client.OnDataReceived?.Invoke(processedData);
                    }

                    consumedBytes = Math.Max(consumedBytes, processedData.Length);
                }

                // Remove consumed bytes from the buffer
                for (var i = 0; i < consumedBytes && !client._receiveBuffer.IsEmpty; i++)
                {
                    client._receiveBuffer.PopFront();
                }

                // If no bytes were consumed, break to prevent infinite loop
                if (consumedBytes == 0)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                client.OnError?.Invoke(e);
                client._receiveBuffer.Clear();
                break;
            }
        }
    }
}
