# AbyssIRC


![AbyssIRC server](imgs/abysslogo.png)

A modern, lightning-fast IRC server written in C#. Built with .NET, this open-source project delivers minimal latency with a
clean, modular architecture.

## Features

- Modern implementation of the IRC protocol
- Multi-threaded message handling
- High performance with minimal latency
- Template-based message system
- Extensible via JavaScript scripting engine
- Support for all standard IRC commands and features
- Modular architecture with dependency injection
- Docker support

## Installation

### Docker

The easiest way to run AbyssIRC is using Docker:

```bash
docker run -d -p 6667:6667 -p 6697:6697 --name abyssirc tgiachi/abyssirc
```

### From Source

1. Clone the repository:

```bash
git clone https://github.com/tgiachi/AbyssIrc.git
```

2. Build the project:

```bash
cd AbyssIrc
dotnet build
```

3. Run the server:

```bash
dotnet run --project src/AbyssIrc.Server/AbyssIrc.Server.csproj
```

## Configuration

AbyssIRC can be configured using command-line arguments or a configuration file:

```bash
AbyssIrc.Server --config /path/to/config.json --root /path/to/data
```

Configuration options:

| Parameter  | Description                   | Default           |
|------------|-------------------------------|-------------------|
| `--root`   | Root directory for the server | Current directory |
| `--config` | Configuration file path       | config.json       |

## JavaScript Extension API

AbyssIRC includes a JavaScript scripting engine that allows extending the server's functionality. Scripts are loaded from the
`scripts` directory under the root path.

Example script:

```javascript
// hello.js
logger.Info("Hello from JavaScript extension!", []);

events.OnStarted(() => {
    logger.Info("Server has started!", []);
});
```

### API Reference

```typescript
// Constants
const VERSION: string;

// Logger
const logger: {
    Info(message: string, args: any[]): void;
};

// Events
const events: {
    OnStarted(action: any): void;
};
```

## IRC Commands Support

AbyssIRC supports the standard IRC protocol commands:

- Connection Registration (NICK, USER, PASS)
- Channel Operations (JOIN, PART, MODE, TOPIC)
- Messaging (PRIVMSG, NOTICE)
- Server-to-Client (PING/PONG, MOTD)
- And many more!

## Building Docker Image

If you want to build the Docker image yourself:

```bash
docker build -t abyssirc .
```

## Development

AbyssIRC is built with .NET and uses a modular architecture. Key components:

- Message Bus (AbyssSignal) for event handling
- Template-based message system
- Session management
- Channel management
- Command handlers

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Thanks to all contributors and the IRC community
- Special thanks to all open-source projects that made this possible
