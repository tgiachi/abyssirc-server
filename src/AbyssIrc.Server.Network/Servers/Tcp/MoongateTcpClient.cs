using System.Buffers;
using System.Net.Sockets;
using AbyssIrc.Server.Network.Buffers;
using AbyssIrc.Server.Network.Middleware;
using NanoidDotNet;

namespace AbyssIrc.Server.Network.Servers.Tcp;

/// <summary>
/// A client for connecting to a remote host
/// </summary>
public class MoongateTcpClient
{
    /// <summary>
    /// Event when the client is connected
    /// </summary>
    public event Action? OnConnected;

    /// <summary>
    ///  Unique identifier for the server this client is connected to
    /// </summary>
    public string ServerId { get; set; }


    /// <summary>
    /// Event when the client is disconnected
    /// </summary>
    public event Action? OnDisconnected;

    /// <summary>
    /// Event when data is received
    /// </summary>
    public event Action<ReadOnlyMemory<byte>>? OnDataReceived;

    /// <summary>
    /// Event when an error occurred
    /// </summary>
    public event Action<Exception>? OnError;

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

    public bool HaveCompression { get; set; }

    /// <summary>
    /// Gets the number of bytes currently available in the receive buffer
    /// </summary>
    public int AvailableBytes => _receiveBuffer?.Size ?? 0;

    /// <summary>
    /// Gets whether the receive buffer is full
    /// </summary>
    public bool IsReceiveBufferFull => _receiveBuffer?.IsFull ?? false;

    private readonly List<INetMiddleware> _middlewares = new();
    private Socket? _socket;
    private int _sending;

    private SocketAsyncEventArgs? _receiveArg;
    private SocketAsyncEventArgs? _sendArg;
    private CircularBuffer<byte>? _receiveBuffer;
    private byte[]? _tempReceiveBuffer;
    private byte[]? _tempSendBuffer;
    private int _bufferSize;

    /// <summary>
    /// Add a middleware to the client
    /// </summary>
    /// <param name="middleware"></param>
    public void AddMiddleware(INetMiddleware middleware)
    {
        _middlewares.Add(middleware);
    }

    public bool ContainsMiddleware(Type type)
    {
        return _middlewares.Any(m => m.GetType() == type);
    }

    /// <summary>
    /// Remove a middleware from the client
    /// </summary>
    /// <param name="middleware"></param>
    public void RemoveMiddleware(INetMiddleware middleware)
    {
        _middlewares.Remove(middleware);
    }

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

        int bytesToPeek = count <= 0 ? _receiveBuffer.Size : Math.Min(count, _receiveBuffer.Size);
        byte[] result = new byte[bytesToPeek];

        for (int i = 0; i < bytesToPeek; i++)
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

        int bytesToConsume = Math.Min(count, _receiveBuffer.Size);
        for (int i = 0; i < bytesToConsume; i++)
        {
            _receiveBuffer.PopFront();
        }
    }

    /// <summary>
    /// Connect to the remote host
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="bufferSize"></param>
    /// <exception cref="InvalidOperationException"></exception>
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
        Ip = ((System.Net.IPEndPoint)_socket.RemoteEndPoint!).Address.ToString();

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
    /// <param name="socket"></param>
    /// <param name="bufferSize"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal void Connect(Socket? socket, int bufferSize = 1024)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("Already connected");
        }

        _bufferSize = bufferSize;
        _socket = socket;
        IsConnected = true;
        Ip = ((System.Net.IPEndPoint)socket.RemoteEndPoint!).Address.ToString();

        InitializeBuffers();
        SetupSocketArgs();

        OnConnected?.Invoke();

        if (!_socket.ReceiveAsync(_receiveArg))
        {
            Receive(_receiveArg);
        }
    }

    private void InitializeBuffers()
    {
        int circularBufferSize = Math.Max(_bufferSize * 4, 4096);
        _receiveBuffer = new CircularBuffer<byte>(circularBufferSize);

        _tempReceiveBuffer = new byte[_bufferSize];
    }

    private void SetupSocketArgs()
    {
        _receiveArg = new SocketAsyncEventArgs();
        _receiveArg.SetBuffer(_tempReceiveBuffer, 0, _tempReceiveBuffer.Length);
        _receiveArg.UserToken = this;
        _receiveArg.Completed += HandleReadWrite;

        _sendArg = new SocketAsyncEventArgs();
        _sendArg.UserToken = this;
        _sendArg.Completed += HandleReadWrite;
    }

    /// <summary>
    /// Stop the client
    /// </summary>
    public void Disconnect()
    {
        if (!IsConnected)
        {
            return;
        }

        IsConnected = false;
        _socket?.Shutdown(SocketShutdown.Both);
        _socket?.Close();
        _socket?.Dispose();
        _socket = null;

        // Cleanup buffers
        if (_tempSendBuffer != null)
        {
            ArrayPool<byte>.Shared.Return(_tempSendBuffer);
            _tempSendBuffer = null;
        }

        _receiveBuffer?.Clear();
        _tempReceiveBuffer = null;

        try
        {
            OnDisconnected?.Invoke();
        }
        catch (Exception e)
        {
            OnError?.Invoke(e);
        }
    }

    /// <summary>
    /// Send data to the remote host
    /// </summary>
    /// <param name="data"></param>
    /// <returns>Whether the data is sent successfully</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public bool Send(ReadOnlyMemory<byte> data)
    {
        if (!IsConnected)
        {
            return false;
        }

        // ensure only one sending operation at a time, no concurrent sending, by using Interlocked
        if (Interlocked.CompareExchange(ref _sending, 1, 0) == 1)
        {
            return false;
        }

        // process through middlewares
        foreach (var middleware in _middlewares)
        {
            try
            {
                middleware.ProcessSend(ref data, out data);
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                // reset sending flag
                Interlocked.Exchange(ref _sending, 0);
                return false;
            }
        }

        _tempSendBuffer = ArrayPool<byte>.Shared.Rent(data.Length);
        data.Span.CopyTo(_tempSendBuffer);
        _sendArg.SetBuffer(_tempSendBuffer, 0, data.Length);

        if (!_socket.SendAsync(_sendArg))
        {
            HandleReadWrite(null, _sendArg);
        }

        return true;
    }

    private static void HandleReadWrite(object sender, SocketAsyncEventArgs? args)
    {
        switch (args.LastOperation)
        {
            case SocketAsyncOperation.Send:
                MoongateTcpClient client = (MoongateTcpClient)args.UserToken!;
                // return buffer
                ArrayPool<byte>.Shared.Return(client._tempSendBuffer!);
                client._tempSendBuffer = null;
                args.SetBuffer(null, 0, 0);
                // reset sending flag
                Interlocked.Exchange(ref client._sending, 0);

                //check connection
                if (args.SocketError != SocketError.Success)
                {
                    client.OnError?.Invoke(new SocketException((int)args.SocketError));
                    Stop(args);
                }

                break;
            case SocketAsyncOperation.Receive:
                //continue receive
                Receive(args);
                break;
            default:
                throw new InvalidOperationException($"Unknown operation: {args.LastOperation}");
        }
    }

    private static void Stop(SocketAsyncEventArgs? args)
    {
        MoongateTcpClient client = (MoongateTcpClient)args.UserToken!;
        client.Disconnect();
    }

    private static void Receive(SocketAsyncEventArgs? args)
    {
        MoongateTcpClient client = (MoongateTcpClient)args.UserToken!;

        // check if the remote host closed the connection
        if (args is { BytesTransferred: > 0, SocketError: SocketError.Success })
        {
            try
            {
                ReadOnlySpan<byte> receivedData = new(args.Buffer, 0, args.BytesTransferred);


                int bytesToAdd = receivedData.Length;
                while (client._receiveBuffer.Size + bytesToAdd > client._receiveBuffer.Capacity)
                {
                    int bytesToRemove = Math.Min(1024, client._receiveBuffer.Size);
                    for (int i = 0; i < bytesToRemove; i++)
                    {
                        client._receiveBuffer.PopFront();
                    }


                    client.OnError?.Invoke(
                        new InvalidOperationException(
                            $"Receive buffer overflow, removed {bytesToRemove} bytes"
                        )
                    );
                }


                for (int i = 0; i < receivedData.Length; i++)
                {
                    client._receiveBuffer.PushBack(receivedData[i]);
                }


                ProcessReceivedData(client);
            }
            catch (Exception e)
            {
                client.OnError?.Invoke(e);
                goto cont_receive;
            }

            if (!client.IsConnected || client._socket == null)
            {
                return;
            }

            cont_receive:
            if (client._socket != null)
            {
                if (!client._socket.ReceiveAsync(args))
                    Receive(args);
            }
            else
            {
                Stop(args);
            }
        }
        else
        {
            Stop(args);
        }
    }

    private static void ProcessReceivedData(MoongateTcpClient client)
    {
        while (!client._receiveBuffer.IsEmpty)
        {
            byte[] currentData = client._receiveBuffer.ToArray();
            ReadOnlyMemory<byte> processedData = currentData;


            // TODO: Implementare la logica completa dei middleware
            int consumedBytes = 0;
            bool shouldHalt = false;


            try
            {
                for (int i = client._middlewares.Count - 1; i >= 0; i--)
                {
                    var middleware = client._middlewares[i];
                    // Nota: questa è una semplificazione - potresti dover adattare l'interfaccia
                    // var (halt, consumed) = middleware.ProcessReceive(ref processedData, out processedData);
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


                if (consumedBytes == 0)
                {
                    consumedBytes = processedData.Length;
                }


                if (!processedData.IsEmpty)
                {
                    client.OnDataReceived?.Invoke(processedData);
                }


                for (int i = 0; i < consumedBytes && !client._receiveBuffer.IsEmpty; i++)
                {
                    client._receiveBuffer.PopFront();
                }


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
