using System.Diagnostics;
using System.Text.RegularExpressions;
using AbyssIrc.Protocol.Messages.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Commands;
using AbyssIrc.Protocol.Messages.Interfaces.Parser;
using Serilog;

namespace AbyssIrc.Protocol.Messages.Services;

public class IrcCommandParser : IIrcCommandParser
{
    private readonly Dictionary<string, IIrcCommand> _commands = new();
    private readonly ILogger _logger = Log.ForContext<IrcCommandParser>();


    public async Task<List<IIrcCommand>> ParseAsync(string message)
    {
        var sw = Stopwatch.StartNew();
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
                var parts = line.Split(' ');
                var command = parts[0].ToUpperInvariant();

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
            sw.Stop();
            _logger.Debug("Parsed {CommandCount} commands in {Elapsed}ms", commands.Count, sw.ElapsedMilliseconds);
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
        var lines = new Regex(@"\r\n|\n").Split(rawMessage);

        return lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
    }
}
