using System.Diagnostics;
using System.Text.RegularExpressions;
using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Interfaces.Commands;
using AbyssIrc.Network.Interfaces.Parser;
using Serilog;

namespace AbyssIrc.Network.Services;

public partial class IrcCommandParser : IIrcCommandParser
{
    private readonly ILogger _logger = Log.ForContext<IrcCommandParser>();

    private readonly Dictionary<string, IIrcCommand> _commands = new();

    public async Task<List<IIrcCommand>> ParseAsync(string message)
    {
        var sw = Stopwatch.GetTimestamp();
        var commands = new List<IIrcCommand>();
        try
        {
            foreach (var line in SanitizeMessage(message))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                _logger.Debug("Parsing line: {Line}", line);

                // Split the command and parameters
                string[] parts = line.Split(' ');
                string command = parts[0].ToUpperInvariant();

                // Create the appropriate command object based on the command string
                _commands.TryGetValue(command, out var ircCommand);

                if (ircCommand != null)
                {
                    try
                    {
                        ircCommand.Parse(line);
                        _logger.Debug("Parsed command: {CommandType}", ircCommand.GetType().Name);
                        commands.Add(ircCommand);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error parsing command {Command}: {Line}", command, line);
                    }
                }
                else
                {
                    _logger.Warning("Unknown command {Command}: {Line}", command, line);
                    var notParsed = new NotParsedCommand();
                    notParsed.Parse(line);
                    commands.Add(notParsed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse message {Message}", message);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(sw);
            _logger.Debug("Parsed {CommandCount} commands in {Elapsed}ms", commands.Count, elapsed);
        }

        return commands;
    }

    public async Task<string> SerializeAsync(IIrcCommand command)
    {
        return command.Write();
    }

    public void RegisterCommand(IIrcCommand command)
    {
        _commands[command.Code] = command;

        _logger.Debug("Registered command {CommandName}", command.Code);
    }

    public List<string> SanitizeMessage(string rawMessage)
    {
        var lines = IrcCommandRegex().Split(rawMessage);

        return lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
    }

    [GeneratedRegex(@"\r\n|\n")]
    private static partial Regex IrcCommandRegex();
}
