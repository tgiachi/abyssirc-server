# AbyssIRC Server

![AbyssIRC server](imgs/abysslogo.png)

AbyssIRC is a modern and lightweight IRC server developed in C#, designed to provide a robust implementation of the IRC
protocol with additional features.

> **IRC is not dead!** This project aims to revitalize IRC by combining the classic protocol with modern features and
> extensibility. Looking for contributors who can help extend the server and build bridges to other chat systems through
> plugins and gateways.

## Features

- Complete implementation of the IRC protocol based on modern standards
- Native SSL/TLS support for secure connections
- Asynchronous event and signal management system
- JavaScript scripting support to extend functionality
- Efficient parsing and serialization of IRC messages
- Simple configuration via YAML files
- Modular system of IRC commands and responses
- Support for variables and templates for MOTD and messages using Scriban templating syntax
- Robust user session and connection management
- Multi-port listening (specify multiple ports with comma separation)

## Upcoming Features

- **Hyper-Scalable Architecture**: Server-to-server protocol implementation for distributed networks
- **Blazing Fast Performance**: Optimized for high-throughput messaging with minimal latency
- **AI-Enhanced Moderation**: Optional integration with AI for intelligent spam and abuse detection
- **Unified Chat Gateway**: Bridge IRC to Discord, Matrix, Slack, and other modern chat platforms
- **Rich Media Support**: Inline media sharing with thumbnails and metadata
- **WebSocket Interface**: Modern web client connectivity without traditional IRC bouncers
- **End-to-End Encryption**: Optional channel and private message encryption
- **OAuth Integration**: Modern authentication with major identity providers
- **Webhooks and Integrations API**: Easy integration with external services
- **Full-Text Chat History**: Searchable message history with customizable retention policies

## System Requirements

- .NET 9.0
- Operating System: Windows, Linux, or macOS (arm64 and x64 supported)

## Installation

### Using Docker

The easiest way to run AbyssIRC is with Docker:

```bash
# Pull the latest image
docker pull tgiachi/abyssirc-server

# Run the server (basic)
docker run -p 6667:6667 -p 6697:6697 tgiachi/abyssirc-server

# Run with a persistent configuration directory
docker run -p 6667:6667 -p 6697:6697 \
  -v $(pwd)/abyssirc-data:/app/abyss \
  -e ABYSS_ROOT_DIRECTORY=/app/abyss \
  tgiachi/abyssirc-server
```

### From Source

1. Clone the repository:
   ```
   git clone https://github.com/tgiachi/abyssirc-server.git
   cd abyssirc-server
   ```

2. Build the project:
   ```
   dotnet build --configuration Release
   ```

3. Run the server:
   ```
   dotnet run --project AbyssIrc.Server/AbyssIrc.Server.csproj
   ```

### Configuration

The main configuration file is in YAML format and is automatically created on first run. The main parameters include:

> **Note about MOTD**: The `motd` field can be either a direct string or a file reference using the `file://` prefix relative
> to the root directory. For example, `file://motd.txt` will load the MOTD from a file named `motd.txt` in the server's root
> directory.

```yaml
network:
  host: irc.abyssirc.com
  ports: "6667"        # Multiple ports can be specified with commas, e.g., "6666,6667,7000"
  ssl_ports: "6697"    # Multiple SSL ports can be specified with commas, e.g., "6697,7697"
  ssl_cert_path: ""
  ssl_cert_password: ""
  ping_timeout: 180
  ping_interval: 60

admin:
  server_password: ""
  admin_info1: "AbyssIrc"
  admin_email: "admin@abyssirc.com"
  network_name: "AbyssIRC"

motd:
  motd: "Welcome to AbyssIrc!"  # Can also use file:// prefix to load from a file

limits:
  max_silence: 16
  max_modes: 6
  max_away_length: 200
  case_mapping: "rfc1459"
  max_nick_length: 30
  max_channel_name_length: 50
  max_topic_length: 390
  max_targets: 4
  max_message_length: 512
  max_channels_per_user: 20
  max_bans_per_channel: 50
  user_modes: "iwos"
  channel_modes: "bklmntsiIpK"
  channel_modes_with_param: "bkloI"
  max_chan_join: 25
  status_msg: "@+"
  elist: "MNUCT"

opers:
  users:
    # Example of an operator configuration
    - username: "admin"
      password: "strong_admin_password"
      host: "192.168.1.*"  # IP mask allowing access

    # Another example with a more restrictive mask
    - username: "localadmin"
      password: "another_secure_password"
      host: "127.0.0.1"  # Localhost only

    # Example with domain mask
    - username: "webadmin"
      password: "web_admin_password"
      host: "*.example.com"  # All subdomains of example.com
```

## Startup Options

The server supports several startup options:

```
-r, --root         Root directory for the server
-c, --config       Configuration file (default: config.yml)
-d, --debug        Enable debug logging
-h, --hostname     Hostname for the server
-s, --show-header  Show header (default: true)
```

## Architecture

AbyssIRC is organized into several modules:

- **AbyssIrc.Core**: Contains core functionality and interfaces
- **AbyssIrc.Network**: Implements the IRC protocol and command handling
- **AbyssIrc.Server**: Implements server logic and connection management
- **AbyssIrc.Signals**: Event system and internal messaging

### Event System

The heart of the server is a publish/subscribe event system that allows communication between various components in a
decoupled and asynchronous manner.

### Connection Management

AbyssIRC uses an efficient connection management system with support for standard and SSL/TLS connections. Each connection is
handled asynchronously, allowing the server to manage a large number of clients simultaneously.

### Template Variables and Text Substitution

AbyssIRC supports dynamic text substitution in MOTD and server messages using the Scriban templating engine. You can use
variables such as:

- `{{hostname}}` - Server hostname
- `{{version}}` - Server version
- `{{created}}` - When the server was created
- `{{uptime}}` - Server uptime
- `{{admin_email}}` - Admin email address
- `{{network_name}}` - IRC network name
- `{{os_name}}` - Operating system name
- `{{cpu_count}}` - Number of CPU cores

Custom variables can be defined programmatically. These variables are processed in real-time, so values like `{{uptime}}`
will always show the current uptime.

### Extensibility via JavaScript

A powerful feature of AbyssIRC is the ability to extend server functionality through JavaScript scripts. Scripts can:

- Register new commands
- React to server events
- Schedule tasks
- Modify server behavior
- Add custom template variables
- Implement custom functionality

#### JavaScript Example

```javascript
// Example script that adds a custom greeting and schedules a task
// Save this as scripts/example.js

// Register event handler for when a client connects
events.HookEvent("client_connected_event", (eventData) => {
    logger.Info("New client connected: " + eventData.Endpoint);
});

// Schedule a task to run every 60 seconds
scheduler.ScheduleTask("announce_uptime", 60, () => {
    // Get all current sessions
    let sessions = irc_manager.GetSessions();
    sessions.forEach(session => {
        irc_manager.SendNotice(session.Id, "Server has been up for " + uptime);
    });
});

// Register a custom command handler
irc_manager.HookCommand("MYCOMMAND", (sessionId, command) => {
    // Send response to the client
    irc_manager.SendPrivMsg(sessionId, "You used my custom command!");

    // Log the event
    logger.Info("Custom command used by: " + sessionId);
});

// Add a custom template variable
template.AddVariableBuilder("random_quote", () => {
    const quotes = [
        "The best way to predict the future is to invent it.",
        "Programming today is a race between software engineers striving to build bigger and better idiot-proof programs, and the Universe trying to produce bigger and better idiots. So far, the Universe is winning.",
        "The most disastrous thing that you can ever learn is your first programming language."
    ];
    return quotes[Math.floor(Math.random() * quotes.length)];
});

// Log when the server starts
events.OnStarted(() => {
    logger.Info("Server has started. Custom script loaded successfully!");
});
```

You can use the custom variable in your MOTD like this: `Quote of the day: {{random_quote}}`

## Development

To contribute to the development:

1. Fork the repository
2. Create a branch for your feature (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### IRC Command Structure

IRC commands are implemented following a common pattern:

```csharp
public class MyCommand : BaseIrcCommand
{
    public MyCommand() : base("COMMANDCODE") { }

    // Command-specific properties

    public override void Parse(string line)
    {
        // Parse the message
    }

    public override string Write()
    {
        // Serialize the command
    }

    // Factory methods
}
```

## License

This project is licensed under [insert license] - see the LICENSE file for details.

## Contributing

I'm actively looking for contributors who can help:

- Develop plugins for AbyssIRC
- Create gateways to bridge IRC with other chat systems (Discord, Matrix, Slack, etc.)
- Improve the core server functionality
- Write documentation and examples
- Test the server in different environments

If you're interested in bringing IRC back to life with modern technology, please consider contributing!

## Contact

- GitHub: https://github.com/tgiachi/abyssirc-server
- Open an issue or pull request for questions and contributions

---

squid -
