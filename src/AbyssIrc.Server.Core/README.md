# AbyssIrc.Server.Core

[![NuGet](https://img.shields.io/nuget/v/AbyssIrc.Server.Core.svg)](https://www.nuget.org/packages/AbyssIrc.Server.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Core server components and framework for the AbyssIrc server ecosystem.

## Overview

AbyssIrc.Server.Core provides the fundamental server infrastructure for building a modular, extensible IRC server. This library sits between the low-level protocol implementation (AbyssIrc.Network) and the full server application, offering a robust framework for IRC server functionality while maintaining a clean separation of concerns.

## Features

- **Service Layer**: Core services for IRC server functionality
- **Session Management**: User session creation, tracking, and lifecycle management
- **Command Handling**: Framework for processing IRC commands and generating responses
- **Event System Integration**: Server-specific events and handlers
- **Server Configuration**: Server-specific configuration management
- **Template Processing**: Server message template processing
- **Extension Framework**: Pluggable architecture for extending server functionality

## Installation

### Via NuGet

```bash
# Install via .NET CLI
dotnet add package AbyssIrc.Server.Core

# Install via Package Manager
Install-Package AbyssIrc.Server.Core
```

### Via PackageReference in .csproj

```xml
<ItemGroup>
    <PackageReference Include="AbyssIrc.Server.Core" Version="0.1.0" />
</ItemGroup>
```

## Usage Examples

### Service Registration

```csharp
// Register core IRC server services
services.AddSingleton<ISessionManagerService, SessionManagerService>();
services.AddSingleton<IIrcManagerService, IrcManagerService>();
services.AddSingleton<ITextTemplateService, TextTemplateService>();
services.AddSingleton<IStringMessageService, StringMessageService>();
services.AddSingleton<ISchedulerSystemService, SchedulerSystemService>();
```

### Command Handlers

```csharp
// Create a command handler
public class PingPongHandler : BaseHandler, IIrcMessageListener
{
    public PingPongHandler(ILogger<PingPongHandler> logger, IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    public Task OnMessageReceivedAsync(string id, IIrcCommand command)
    {
        return command switch
        {
            PingCommand pingCommand => HandlePingCommand(id, pingCommand),
            PongCommand pongCommand => HandlePongCommand(id, pongCommand),
            _ => Task.CompletedTask
        };
    }

    private async Task HandlePingCommand(string id, PingCommand pingCommand)
    {
        var session = GetSession(id);
        await SendIrcMessageAsync(id, new PongCommand(ServerData.Hostname, pingCommand.Token));
    }

    // Additional handler methods...
}
```

### Session Management

```csharp
// Using the session manager
public async Task ProcessNewConnection(string connectionId, string ipEndpoint)
{
    // Add a new session
    _sessionManagerService.AddSession(connectionId, ipEndpoint);

    // Get an existing session
    var session = _sessionManagerService.GetSession(connectionId);

    // Query sessions
    var operatorSessions = _sessionManagerService.GetSessions()
        .Where(s => s.IsOperator)
        .ToList();
}
```

### Sending IRC Messages

```csharp
// Send messages to clients
await SendIrcMessageAsync(
    sessionId,
    new RplWelcomeCommand(ServerData.Hostname, session.Nickname, "Welcome to the IRC network!")
);

await SendIrcMessageAsync(
    sessionId,
    new RplMotdStart.Create(ServerData.Hostname, session.Nickname)
);

foreach (var line in _motdLines)
{
    await SendIrcMessageAsync(
        sessionId,
        new RplMotd(ServerData.Hostname, session.Nickname, _textTemplateService.TranslateText(line))
    );
}
```

## Core Components

### Base Handlers

The library provides base handler classes for implementing IRC command functionality:

```csharp
public abstract class BaseHandler
{
    protected ILogger Logger { get; }
    protected AbyssServerData ServerData { get; }
    protected AbyssIrcConfig ServerConfig { get; }

    // Helper methods for command processing
    protected Task SendIrcMessageAsync(string id, IIrcCommand message);
    protected IrcSession? GetSession(string id);
    protected List<IrcSession> GetSessions();
    protected IEnumerable<IrcSession> QuerySessions(Func<IrcSession, bool> query);

    // Event handling
    protected void SubscribeSignal<TEvent>(IAbyssSignalListener<TEvent> listener) where TEvent : class;
    protected Task SendSignalAsync<T>(T signal) where T : class;

    // Additional utility methods...
}
```

### Service Interfaces

Key service interfaces that form the server's architecture:

- `ISessionManagerService`: Manages user connections and sessions
- `IIrcManagerService`: Core IRC command processing and dispatch
- `ITextTemplateService`: Template processing for server messages
- `ISchedulerSystemService`: Task scheduling for maintenance operations
- `IEventDispatcherService`: Event routing and handling

### Events System

Server-specific events for different aspects of IRC server operation:

- Connection events: `ClientConnectedEvent`, `ClientDisconnectedEvent`, `ClientReadyEvent`
- Message events: `IrcMessageReceivedEvent`, `SendIrcMessageEvent`
- Session events: `SessionAddedEvent`, `SessionRemovedEvent`

## Dependency on Other AbyssIrc Packages

AbyssIrc.Server.Core has the following dependencies:

- **AbyssIrc.Core**: Fundamental types, utilities, and infrastructure
- **AbyssIrc.Network**: IRC protocol implementation and command definitions

## Requirements

- .NET 9.0 or later
- Compatible with Windows, Linux, and macOS

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! If you're interested in improving AbyssIrc.Server.Core:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

For more information on AbyssIrc and related projects, visit the [AbyssIrc GitHub repository](https://github.com/tgiachi/abyssirc-server).
