namespace AbyssIrc.Core.Utils;

using System.Buffers;
using System.Text;

/// <summary>
/// Utility methods for converting List<string> to ReadOnlyMemory<byte> with CRLF line endings
/// </summary>
public static class StringListToBytesConverter
{
    /// <summary>
    /// CRLF line ending as bytes
    /// </summary>
    private static readonly byte[] CrLf = { 0x0D, 0x0A }; // \r\n

    /// <summary>
    /// UTF-8 encoding instance (reusable)
    /// </summary>
    private static readonly UTF8Encoding Utf8Encoding = new(false, false);

    /// <summary>
    /// Convert List<string> to ReadOnlyMemory<byte> with CRLF line endings - Most efficient version
    /// Uses ArrayPool for temporary buffer management
    /// </summary>
    /// <param name="lines">List of strings to convert</param>
    /// <returns>ReadOnlyMemory<byte> containing all lines with CRLF endings</returns>
    public static ReadOnlyMemory<byte> ConvertWithArrayPool(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        // Calculate total byte count needed
        var totalBytes = 0;
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                totalBytes += Utf8Encoding.GetByteCount(line);
            }

            totalBytes += 2; // CRLF
        }

        // Rent buffer from ArrayPool
        var buffer = ArrayPool<byte>.Shared.Rent(totalBytes);
        var position = 0;

        try
        {
            foreach (var line in lines)
            {
                // Convert string to bytes
                if (!string.IsNullOrEmpty(line))
                {
                    var bytesWritten = Utf8Encoding.GetBytes(line, buffer.AsSpan(position));
                    position += bytesWritten;
                }

                // Add CRLF
                buffer[position++] = CrLf[0];
                buffer[position++] = CrLf[1];
            }

            // Create a copy since we need to return the ArrayPool buffer
            var result = new byte[position];
            Array.Copy(buffer, result, position);
            return new ReadOnlyMemory<byte>(result);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Convert List<string> to ReadOnlyMemory<byte> with CRLF line endings - Simple version
    /// Uses StringBuilder approach (less memory efficient but simpler)
    /// </summary>
    /// <param name="lines">List of strings to convert</param>
    /// <returns>ReadOnlyMemory<byte> containing all lines with CRLF endings</returns>
    public static ReadOnlyMemory<byte> ConvertWithStringBuilder(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.AppendLine(line); // AppendLine adds Environment.NewLine, but we'll handle CRLF manually
        }

        // Convert to string then to bytes
        var combinedString = string.Join("\r\n", lines) + "\r\n";
        var bytes = Utf8Encoding.GetBytes(combinedString);
        return new ReadOnlyMemory<byte>(bytes);
    }

    /// <summary>
    /// Convert List<string> to ReadOnlyMemory<byte> with CRLF line endings - Memory efficient version
    /// Uses MemoryStream for dynamic sizing
    /// </summary>
    /// <param name="lines">List of strings to convert</param>
    /// <returns>ReadOnlyMemory<byte> containing all lines with CRLF endings</returns>
    public static ReadOnlyMemory<byte> ConvertWithMemoryStream(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        using var memoryStream = new MemoryStream();

        foreach (var line in lines)
        {
            // Write line bytes
            if (!string.IsNullOrEmpty(line))
            {
                var lineBytes = Utf8Encoding.GetBytes(line);
                memoryStream.Write(lineBytes, 0, lineBytes.Length);
            }

            // Write CRLF
            memoryStream.Write(CrLf, 0, CrLf.Length);
        }

        return new ReadOnlyMemory<byte>(memoryStream.ToArray());
    }

    /// <summary>
    /// Convert List<string> to ReadOnlyMemory<byte> with CRLF line endings - Span-based version
    /// Most performant for known small to medium sized lists
    /// </summary>
    /// <param name="lines">List of strings to convert</param>
    /// <returns>ReadOnlyMemory<byte> containing all lines with CRLF endings</returns>
    public static ReadOnlyMemory<byte> ConvertWithSpan(List<string> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        // Pre-calculate total size
        var totalSize = 0;
        foreach (var line in lines)
        {
            totalSize += Utf8Encoding.GetByteCount(line ?? string.Empty) + 2; // +2 for CRLF
        }

        // Allocate exact size
        var result = new byte[totalSize];
        var span = result.AsSpan();
        var position = 0;

        foreach (var line in lines)
        {
            // Write line
            if (!string.IsNullOrEmpty(line))
            {
                var bytesWritten = Utf8Encoding.GetBytes(line, span.Slice(position));
                position += bytesWritten;
            }

            // Write CRLF
            span[position++] = CrLf[0];
            span[position++] = CrLf[1];
        }

        return new ReadOnlyMemory<byte>(result);
    }

    /// <summary>
    /// Convert List<string> to ReadOnlyMemory<byte> with custom line ending
    /// </summary>
    /// <param name="lines">List of strings to convert</param>
    /// <param name="lineEnding">Custom line ending bytes (default: CRLF)</param>
    /// <returns>ReadOnlyMemory<byte> containing all lines with specified endings</returns>
    public static ReadOnlyMemory<byte> ConvertWithCustomLineEnding(
        List<string> lines, ReadOnlySpan<byte> lineEnding = default
    )
    {
        if (lines == null || lines.Count == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        // Use CRLF as default
        if (lineEnding.IsEmpty)
        {
            lineEnding = CrLf;
        }

        // Pre-calculate total size
        var totalSize = 0;
        foreach (var line in lines)
        {
            totalSize += Utf8Encoding.GetByteCount(line ?? string.Empty) + lineEnding.Length;
        }

        // Allocate exact size
        var result = new byte[totalSize];
        var span = result.AsSpan();
        var position = 0;

        foreach (var line in lines)
        {
            // Write line
            if (!string.IsNullOrEmpty(line))
            {
                var bytesWritten = Utf8Encoding.GetBytes(line, span.Slice(position));
                position += bytesWritten;
            }

            // Write line ending
            lineEnding.CopyTo(span.Slice(position));
            position += lineEnding.Length;
        }

        return new ReadOnlyMemory<byte>(result);
    }
}
