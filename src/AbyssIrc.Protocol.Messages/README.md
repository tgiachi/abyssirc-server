# AbyssIrc.Network

A comprehensive IRC protocol implementation library for .NET, providing robust parsing, serialization, and command handling for IRC communications.

## Features

- Full IRC protocol command support
- Flexible command parsing and serialization
- Comprehensive error handling
- Strongly-typed IRC commands
- Support for both client and server-side operations
- Extensive validation and parsing mechanisms

## Installation

Install the package via NuGet:

```bash
dotnet add package AbyssIrc.Protocol.Messages
```

## Quick Start

### Parsing IRC Commands

```csharp
// Parsing a PRIVMSG command
var privmsgCommand = new PrivMsgCommand();
privmsgCommand.Parse(":nick!user@host PRIVMSG #channel :Hello, world!");

Console.WriteLine(privmsgCommand.Source);       // "nick!user@host"
Console.WriteLine(privmsgCommand.Target);       // "#channel"
Console.WriteLine(privmsgCommand.Message);      // "Hello, world!"
```

### Creating Commands

```csharp
// Creating a PRIVMSG command
var newPrivmsg = PrivMsgCommand.CreateFromUser(
    "nick!user@host",
    "#channel",
    "Hello, world!"
);

// Serializing the command
string commandString = newPrivmsg.Write();
```

## Supported Commands

The library provides comprehensive support for IRC commands, including:

- Basic Commands: NICK, USER, PASS, QUIT
- Communication: PRIVMSG, NOTICE
- Channel Operations: JOIN, PART, TOPIC, MODE
- Server Queries: VERSION, ADMIN, INFO
- Authentication: AUTHENTICATE (SASL)

### Command Types

#### Base Commands
- `PassCommand`: Server password authentication
- `NickCommand`: Nickname registration
- `UserCommand`: User registration

#### Communication Commands
- `PrivMsgCommand`: Private messaging
- `NoticeCommand`: Server and user notices

#### Channel Commands
- `JoinCommand`: Joining channels
- `PartCommand`: Leaving channels
- `TopicCommand`: Channel topic management
- `ModeCommand`: Channel and user mode modifications

## Error Handling

Comprehensive error command support:

```csharp
// Creating error responses
var noSuchNickError = ErrNoSuchNick.Create(
    "server.com",
    "requester",
    "target"
);

// Parsing error commands
var errorCmd = new ErrNoSuchNick();
errorCmd.Parse(":server.com 401 nickname target :No such nick/channel");
```

## Advanced Parsing

### Flexible Command Parsing

```csharp
// Parsing multiple commands
var commandParser = new IrcCommandParser();
var commands = await commandParser.ParseAsync(rawMessage);

foreach (var cmd in commands)
{
    switch (cmd)
    {
        case PrivMsgCommand privMsg:
            // Handle private message
            break;
        case JoinCommand joinCmd:
            // Handle channel join
            break;
    }
}
```

## Configuration

### Command Parsing Options

```csharp
var parserOptions = new IrcCommandParserOptions
{
    StrictParsing = true,        // Enforce strict RFC compliance
    AllowExtendedFormat = true   // Support extended IRC command formats
};
```

## Performance Considerations

- Minimal allocation overhead
- Efficient parsing mechanisms
- Strongly-typed command representations
- Minimal runtime type checking

## Extensibility

- Easy to extend with custom commands
- Implement `IIrcCommand` interface for new command types
- Flexible parsing and serialization methods

## Dependencies

- .NET 6.0 or later
- System.Text.Json (for serialization)

## Contributing

1. Fork the repository
2. Create your feature branch
3. Implement or improve IRC command support
4. Write comprehensive tests
5. Create a pull request

## Roadmap

- [ ] Full RFC 1459 and RFC 2812 compliance
- [ ] More extensive command validation
- [ ] Additional parsing methods
- [ ] Enhanced error reporting

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Support

- Open an issue on GitHub for bug reports or feature requests
- Join our community discussions

## About AbyssIrc

AbyssIrc.Network is a core component of the AbyssIrc ecosystem, providing a robust foundation for IRC protocol implementations.

---

**Note**: This library is currently in active development. APIs may change between versions.
