using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Parser;
using Serilog;

namespace AbyssIrc.Protocol.Messages.Services;

public partial class IrcCommandParser : IIrcCommandParser
{
    private readonly Dictionary<string, IIrcCommand> _commands = new();
    private readonly ILogger _logger = Log.ForContext<IrcCommandParser>();

    /// <summary>
    /// UTF-8 encoding instance for byte-to-string conversion
    /// </summary>
    private static readonly UTF8Encoding Utf8Encoding = new(false, false);

    /// <summary>
    /// Line endings as byte sequences for efficient splitting
    /// </summary>
    private static readonly byte[] CrLf = "\r\n"u8.ToArray(); // \r\n

    private const byte Lf = 0x0A;    // \n
    private const byte Cr = 0x0D;    // \r
    private const byte Space = 0x20; // space

    /// <summary>
    /// Parse IRC commands from raw byte data
    /// </summary>
    /// <param name="data">Raw byte data containing IRC messages</param>
    /// <returns>List of parsed IRC commands</returns>
    public async Task<List<IIrcCommand>> ParseAsync(ReadOnlyMemory<byte> data)
    {
        var sw = Stopwatch.GetTimestamp();
        var commands = new List<IIrcCommand>();

        try
        {
            // Split the data into lines efficiently
            var lines = SplitIntoLines(data);

            foreach (var line in lines)
            {
                if (line.IsEmpty)
                {
                    continue;
                }

                // Convert only non-empty lines to string for parsing
                var lineString = Utf8Encoding.GetString(line.Span);

                if (string.IsNullOrWhiteSpace(lineString))
                {
                    continue;
                }

                _logger.Debug("Parsing line: {Line}", lineString);

                // Extract command efficiently without creating intermediate arrays
                var commandMemory = ExtractCommand(line);
                if (commandMemory.IsEmpty)
                {
                    continue;
                }

                // Convert command to string and lookup
                var commandString = Utf8Encoding.GetString(commandMemory.Span).ToUpperInvariant();

                if (_commands.TryGetValue(commandString, out var ircCommand))
                {
                    try
                    {
                        ircCommand.Parse(lineString);
                        _logger.Debug("Parsed command: {CommandType}", ircCommand.GetType().Name);
                        commands.Add(ircCommand);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error parsing command {Command}: {Line}", commandString, lineString);
                    }
                }
                else
                {
                    _logger.Warning("Unknown command {Command}: {Line}", commandString, lineString);
                    var notParsed = new NotParsedCommand();
                    notParsed.Parse(lineString);
                    commands.Add(notParsed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse message from byte data");
        }
        finally
        {
            _logger.Debug("Parsed {CommandCount} commands in {Elapsed}ms",
                commands.Count, Stopwatch.GetElapsedTime(sw).TotalMilliseconds);
        }

        return commands;
    }

    /// <summary>
    /// Serialize IRC command to string
    /// </summary>
    /// <param name="command">IRC command to serialize</param>
    /// <returns>Serialized command string</returns>
    public async Task<string> SerializeAsync(IIrcCommand command)
    {
        return command.Write();
    }

    /// <summary>
    /// Register an IRC command for parsing
    /// </summary>
    /// <param name="command">IRC command to register</param>
    public void RegisterCommand(IIrcCommand command)
    {
        _commands[command.Code] = command;
        _logger.Debug("Registered command {CommandName}", command.Code);
    }

    /// <summary>
    /// Split byte data into individual lines efficiently
    /// </summary>
    /// <param name="data">Input byte data</param>
    /// <returns>List of line memories</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static List<ReadOnlyMemory<byte>> SplitIntoLines(ReadOnlyMemory<byte> data)
    {
        var lines = new List<ReadOnlyMemory<byte>>();
        var span = data.Span;
        var start = 0;

        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] == Lf)
            {
                // Handle \r\n or just \n
                var end = i;
                if (end > start && span[end - 1] == Cr)
                {
                    end--; // Remove \r from \r\n
                }

                if (end > start)
                {
                    lines.Add(data.Slice(start, end - start));
                }

                start = i + 1;
            }
            else if (span[i] == Cr && (i + 1 >= span.Length || span[i + 1] != Lf))
            {
                // Handle standalone \r (Mac style)
                if (i > start)
                {
                    lines.Add(data.Slice(start, i - start));
                }
                start = i + 1;
            }
        }

        // Handle last line without line ending
        if (start < span.Length)
        {
            lines.Add(data.Slice(start));
        }

        return lines;
    }

    /// <summary>
    /// Extract the command portion from a line efficiently
    /// </summary>
    /// <param name="lineMemory">Line memory to extract command from</param>
    /// <returns>Command memory</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ReadOnlyMemory<byte> ExtractCommand(ReadOnlyMemory<byte> lineMemory)
    {
        var lineSpan = lineMemory.Span;
        var start = 0;

        // Skip source prefix if present (starts with ':')
        if (lineSpan.Length > 0 && lineSpan[0] == 0x3A) // ':'
        {
            // Find first space after source
            var spaceIndex = lineSpan.IndexOf(Space);
            if (spaceIndex == -1)
            {
                return ReadOnlyMemory<byte>.Empty;
            }

            start = spaceIndex + 1;

            // Skip any additional spaces
            while (start < lineSpan.Length && lineSpan[start] == Space)
            {
                start++;
            }
        }

        if (start >= lineSpan.Length)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        // Find the end of the command (next space or end of line)
        var commandSpan = lineSpan.Slice(start);
        var commandEnd = commandSpan.IndexOf(Space);

        var commandLength = commandEnd == -1 ? commandSpan.Length : commandEnd;
        return lineMemory.Slice(start, commandLength);
    }

    /// <summary>
    /// Legacy method for backward compatibility - converts string to bytes and calls optimized version
    /// </summary>
    /// <param name="message">String message to parse</param>
    /// <returns>List of parsed IRC commands</returns>
    public async Task<List<IIrcCommand>> ParseAsync(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return new List<IIrcCommand>();
        }

        // Convert string to UTF-8 bytes and call optimized version
        var bytes = Utf8Encoding.GetBytes(message);
        return await ParseAsync(new ReadOnlyMemory<byte>(bytes));
    }

    /// <summary>
    /// Legacy sanitize method - kept for compatibility but not used in optimized path
    /// </summary>
    /// <param name="rawMessage">Raw message string</param>
    /// <returns>List of sanitized lines</returns>
    public List<string> SanitizeMessage(string rawMessage)
    {
        if (string.IsNullOrEmpty(rawMessage))
        {
            return [];
        }

        // Use the optimized byte-based parsing and convert back to strings
        var bytes = Utf8Encoding.GetBytes(rawMessage);
        var lines = SplitIntoLines(new ReadOnlyMemory<byte>(bytes));

        return lines
            .Where(line => !line.IsEmpty)
            .Select(line => Utf8Encoding.GetString(line.Span))
            .Where(str => !string.IsNullOrWhiteSpace(str))
            .ToList();
    }
}
