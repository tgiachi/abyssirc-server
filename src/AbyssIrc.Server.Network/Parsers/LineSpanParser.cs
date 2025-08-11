
namespace AbyssIrc.Server.Network.Parsers;

/// <summary>
/// A span parser that splits incoming data by line endings (\r\n)
/// </summary>
public class LineSpanParser : ISpanParser
{
    private readonly List<byte> _buffer = [];
    private readonly byte[] _delimiter = "\r\n"u8.ToArray(); // \r\n
    private readonly int _maxLineLength;
    private readonly List<byte[]> _completedLines = [];

    /// <summary>
    /// Initializes a new instance of the LineSpanParser
    /// </summary>
    /// <param name="maxLineLength">Maximum length of a single line (default: 512 bytes for IRC compliance)</param>
    public LineSpanParser(int maxLineLength = 512)
    {
        _maxLineLength = maxLineLength;
    }

    /// <summary>
    /// Process incoming data and extract complete lines
    /// </summary>
    /// <param name="data">The incoming data to process</param>
    /// <param name="processedData">The processed lines as separate memory blocks</param>
    /// <returns>Number of bytes consumed from the input</returns>
    public int ProcessData(ReadOnlySpan<byte> data, out ReadOnlyMemory<byte> processedData)
    {
        _completedLines.Clear();
        var totalConsumed = 0;

        for (int i = 0; i < data.Length; i++)
        {
            var currentByte = data[i];
            _buffer.Add(currentByte);
            totalConsumed++;

            // Check if we have exceeded max line length
            if (_buffer.Count > _maxLineLength)
            {
                // Clear buffer and continue - this prevents memory issues with malformed data
                _buffer.Clear();
                continue;
            }

            // Check if we have a complete delimiter
            if (HasCompleteDelimiter())
            {
                // Extract the line without the delimiter
                var lineLength = _buffer.Count - _delimiter.Length;
                if (lineLength > 0)
                {
                    var lineData = _buffer.Take(lineLength).ToArray();
                    _completedLines.Add(lineData);
                }

                // Clear the buffer for the next line
                _buffer.Clear();
            }
        }

        // Combine all completed lines into a single memory block
        if (_completedLines.Count > 0)
        {
            var totalLength = _completedLines.Sum(line => line.Length);
            var combinedData = new byte[totalLength];
            var offset = 0;

            foreach (var line in _completedLines)
            {
                line.CopyTo(combinedData, offset);
                offset += line.Length;
            }

            processedData = new ReadOnlyMemory<byte>(combinedData);
        }
        else
        {
            processedData = ReadOnlyMemory<byte>.Empty;
        }

        return totalConsumed;
    }

    /// <summary>
    /// Reset the parser state and clear internal buffers
    /// </summary>
    public void Reset()
    {
        _buffer.Clear();
        _completedLines.Clear();
    }

    /// <summary>
    /// Check if the current buffer ends with the complete delimiter
    /// </summary>
    /// <returns>True if the buffer contains a complete delimiter at the end</returns>
    private bool HasCompleteDelimiter()
    {
        if (_buffer.Count < _delimiter.Length)
        {
            return false;
        }

        for (int i = 0; i < _delimiter.Length; i++)
        {
            if (_buffer[_buffer.Count - _delimiter.Length + i] != _delimiter[i])
            {
                return false;
            }
        }

        return true;
    }
}
