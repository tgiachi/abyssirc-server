using AbyssIrc.Network.Commands;
using AbyssIrc.Network.Commands.Replies;
using AbyssIrc.Network.Interfaces.Parser;
using AbyssIrc.Network.Services;

namespace AbyssIrc.Tests;

public class Tests
{
    private IIrcCommandParser _commandParser;

    [SetUp]
    public void Setup()
    {
        _commandParser = new IrcCommandParser();
        _commandParser.RegisterCommand(new RplCreatedCommand());
        _commandParser.RegisterCommand(new RplMyInfoCommand());
        _commandParser.RegisterCommand(new RplWelcomeCommand());
        _commandParser.RegisterCommand(new RplYourHostCommand());

        _commandParser.RegisterCommand(new CapCommand());
        _commandParser.RegisterCommand(new NickCommand());
        _commandParser.RegisterCommand(new UserCommand());
    }

    [Test]
    public async Task TestParseMessage()
    {
        // Command:

        var cmd = "CAP LS 302\r\nNICK Guest82\r\nUSER textual 0 * :Textual User\r\n";

        var messages = await _commandParser.ParseAsync(cmd);

        Assert.That(messages.Count, Is.EqualTo(3));
    }
}
