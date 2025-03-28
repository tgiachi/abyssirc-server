using System.Text;

namespace AbyssIrc.Server.Servers.Utils;

public class IrcMessageFramer
{
    // Circular buffer with initial capacity to avoid frequent reallocations
    private byte[] _buffer;
    private int _start = 0;
    private int _end = 0;
    private readonly List<(int offset, int length)> _messageRanges = [];

    // Typical value for IRC messages (512 is common size)
    public IrcMessageFramer(int initialCapacity = 2048)
    {
        _buffer = new byte[initialCapacity];
    }

    public void Append(ReadOnlySpan<byte> data)
    {
        // Ensure we have enough space in the buffer
        EnsureCapacity(data.Length);

        // Copy new data into the buffer
        if (_end + data.Length <= _buffer.Length)
        {
            // Direct copy if space is contiguous
            data.CopyTo(new Span<byte>(_buffer, _end, data.Length));
        }
        else
        {
            // Handle circular buffer wrapping
            int firstPart = _buffer.Length - _end;
            data[..firstPart].CopyTo(new Span<byte>(_buffer, _end, firstPart));
            data[firstPart..].CopyTo(new Span<byte>(_buffer, 0, data.Length - firstPart));
        }

        // Update end position
        _end = (_end + data.Length) % _buffer.Length;

        // Find all complete messages
        FindCompleteMessages();
    }

    private void EnsureCapacity(int additionalLength)
    {
        int currentCapacity = _buffer.Length;
        int usedSpace = UsedSpace();
        int requiredCapacity = usedSpace + additionalLength;

        if (requiredCapacity <= currentCapacity)
        {
            return;
        }

        // Double capacity until it meets the requirement
        int newCapacity = currentCapacity;
        while (newCapacity < requiredCapacity)
        {
            newCapacity *= 2;
        }

        // Allocate new buffer and copy linearized data
        byte[] newBuffer = new byte[newCapacity];
        CopyToLinearBuffer(newBuffer);

        // Update start/end in the new linear buffer
        _start = 0;
        _end = usedSpace;
        _buffer = newBuffer;
    }

    private void CopyToLinearBuffer(byte[] newBuffer)
    {
        if (_start <= _end)
        {
            // Data is already contiguous
            Buffer.BlockCopy(_buffer, _start, newBuffer, 0, _end - _start);
        }
        else
        {
            // Data is wrapped, copy in two parts
            int part1Length = _buffer.Length - _start;
            Buffer.BlockCopy(_buffer, _start, newBuffer, 0, part1Length);
            Buffer.BlockCopy(_buffer, 0, newBuffer, part1Length, _end);
        }
    }

    private int UsedSpace()
    {
        return _start <= _end
            ? _end - _start
            : _buffer.Length - _start + _end;
    }

    private void FindCompleteMessages()
    {
        int scan = _start;
        int messageStart = _start;

        while (scan != _end)
        {
            // Check for line endings (\r\n or \n)
            if (_buffer[scan] == '\r')
            {
                // Check if next byte is \n
                int nextPos = (scan + 1) % _buffer.Length;
                if (nextPos != _end && _buffer[nextPos] == '\n')
                {
                    // Calculate message offset and length (excluding \r\n)
                    int messageLength = GetDistance(messageStart, scan);
                    _messageRanges.Add((messageStart, messageLength));

                    // Advance past \r\n
                    messageStart = (nextPos + 1) % _buffer.Length;
                    scan = messageStart;
                    continue;
                }
            }
            else if (_buffer[scan] == '\n')
            {
                // \n line ending
                int messageLength = GetDistance(messageStart, scan);
                _messageRanges.Add((messageStart, messageLength));

                // Advance past \n
                messageStart = (scan + 1) % _buffer.Length;
                scan = messageStart;
                continue;
            }

            // Advance to next element
            scan = (scan + 1) % _buffer.Length;
        }

        // Update _start to free consumed space
        _start = messageStart;
    }

    private int GetDistance(int from, int to)
    {
        return from <= to
            ? to - from
            : _buffer.Length - from + to;
    }

    public IEnumerable<ReadOnlyMemory<byte>> GetCompletedMessageBuffers()
    {
        if (_messageRanges.Count == 0)
            yield break;

        foreach (var (offset, length) in _messageRanges)
        {
            if (offset + length <= _buffer.Length)
            {
                // Contiguous message
                yield return new ReadOnlyMemory<byte>(_buffer, offset, length);
            }
            else
            {
                // Wrapped message
                // Note: this case would require an allocation or a more complex way to represent non-contiguous buffers
                byte[] temp = new byte[length];
                int firstPart = _buffer.Length - offset;
                Buffer.BlockCopy(_buffer, offset, temp, 0, firstPart);
                Buffer.BlockCopy(_buffer, 0, temp, firstPart, length - firstPart);
                yield return temp;
            }
        }

        _messageRanges.Clear();
    }

    public IEnumerable<string> GetCompletedMessages()
    {
        // Convert buffers to strings only when actually needed
        return GetCompletedMessageBuffers().Select(buffer => Encoding.UTF8.GetString(buffer.Span));
    }
}
