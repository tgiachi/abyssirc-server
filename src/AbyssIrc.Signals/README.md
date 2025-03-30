# AbyssIrc.Signals

A lightweight, high-performance event and signal management library for .NET applications.

## Features

- Asynchronous event dispatching
- Publish/Subscribe pattern implementation
- High-performance event processing
- Flexible signal listener registration
- Supports multiple event handlers
- Configurable parallel event processing

## Installation

Install the package via NuGet:

```bash
dotnet add package AbyssIrc.Signals
```

## Quick Start

### Basic Usage

```csharp
// Create a signal service
var signalConfig = new AbyssIrcSignalConfig
{
    DispatchTasks = Environment.ProcessorCount
};
var signalService = new AbyssSignalService(signalConfig);

// Define an event
public record UserConnectedEvent(string Username);

// Create a listener
public class UserConnectionListener : IAbyssSignalListener<UserConnectedEvent>
{
    public async Task OnEventAsync(UserConnectedEvent signalEvent)
    {
        Console.WriteLine($"User connected: {signalEvent.Username}");
    }
}

// Subscribe to an event
signalService.Subscribe<UserConnectedEvent>(new UserConnectionListener());

// Or use a lambda
signalService.Subscribe<UserConnectedEvent>(async evt =>
{
    Console.WriteLine($"User connected: {evt.Username}");
});

// Publish an event
await signalService.PublishAsync(new UserConnectedEvent("JohnDoe"));
```

## Core Concepts

### AbyssSignalService

The central component for event management. Key methods include:

- `Subscribe<TEvent>()`: Register event listeners
- `PublishAsync<TEvent>()`: Dispatch events to all registered listeners
- `AllEventsObservable`: Observable stream of all events

### AbyssIrcSignalConfig

Configure the signal service:

```csharp
var config = new AbyssIrcSignalConfig
{
    // Number of parallel dispatch tasks
    DispatchTasks = 5
};
```

## Advanced Usage

### Multiple Listeners

```csharp
// Multiple listeners can handle the same event
signalService.Subscribe<UserConnectedEvent>(listener1);
signalService.Subscribe<UserConnectedEvent>(listener2);
```

### Error Handling

The signal service handles listener exceptions, logging them without stopping event dispatching.

## Performance Considerations

- Uses `System.Reactive` for efficient event handling
- Configurable parallel event processing
- Low-overhead event dispatching

## Extensibility

- Implement `IAbyssSignalListener<TEvent>` for custom listeners
- Use lambda expressions for quick event handling
- Easily integrate with dependency injection

## Dependencies

- .NET 6.0 or later
- System.Reactive

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Support

- Open an issue on GitHub for bug reports or feature requests
- Join our community discussions

## About AbyssIrc

AbyssIrc.Signals is part of the AbyssIrc ecosystem, a modern, extensible IRC server and networking library.

---

**Note**: This library is currently in active development. APIs may change between versions.
