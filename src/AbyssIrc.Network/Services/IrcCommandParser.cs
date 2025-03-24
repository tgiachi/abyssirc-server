using System.Text.RegularExpressions;
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
        var commands = new List<IIrcCommand>();
        try
        {
            var lines = IrcCommandRegex().Split(message);

            foreach (var line in lines)
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
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse message {Message}", message);
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

    [GeneratedRegex(@"\r\n|\n")]
    private static partial Regex IrcCommandRegex();
}
