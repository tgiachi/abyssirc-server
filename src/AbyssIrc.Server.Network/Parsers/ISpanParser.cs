namespace AbyssIrc.Server.Network.Parsers;

/// <summary>
/// Defines a contract for parsing incoming data spans in a middleware-like chain
/// </summary>
public interface ISpanParser
{
    /// <summary>
    /// Process incoming data and potentially modify it for the next parser in the chain
    /// </summary>
    /// <param name="data">The data to process</param>
    /// <param name="processedData">The processed data to pass to the next parser</param>
    /// <returns>Number of bytes consumed from the input data</returns>
    int ProcessData(ReadOnlySpan<byte> data, out ReadOnlyMemory<byte> processedData);

    /// <summary>
    /// Reset the parser state (clear any internal buffers)
    /// </summary>
    void Reset();
}
