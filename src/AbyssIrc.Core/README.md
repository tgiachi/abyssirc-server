# AbyssIrc.Core

[![NuGet](https://img.shields.io/nuget/v/AbyssIrc.Core.svg)](https://www.nuget.org/packages/AbyssIrc.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Core library for AbyssIrc server, providing the foundation for building modular, extensible IRC server components.

## Overview

AbyssIrc.Core is the fundamental building block of the AbyssIrc server ecosystem. It provides essential utilities, interfaces, extensions, and data structures that other AbyssIrc modules rely on. This library is designed to be lightweight yet powerful, offering a consistent set of tools for IRC server development.

## Features

- **Configuration System**: YAML-based configuration with strong typing and validation
- **Directory Management**: Structured directory system for server resources
- **Template Engine**: Dynamic text templating using Scriban for MOTD and messages
- **Event System**: Base interfaces for the event/signal system
- **Common Utilities**: String processing, JSON serialization, YAML parsing
- **Extension Methods**: Comprehensive set of extension methods for common operations
- **Service Interface Definitions**: Core service interfaces for IRC server operations

## Installation

### Via NuGet

```bash
# Install via .NET CLI
dotnet add package AbyssIrc.Core

# Install via Package Manager
Install-Package AbyssIrc.Core
```

### Via PackageReference in .csproj

```xml
<ItemGroup>
    <PackageReference Include="AbyssIrc.Core" Version="0.1.0" />
</ItemGroup>
```

## Usage Examples

### Configuration

```csharp
// Load a YAML configuration
var configYaml = File.ReadAllText("config.yml");
var config = configYaml.FromYaml<AbyssIrcConfig>();

// Save configuration
var yaml = config.ToYaml();
File.WriteAllText("config.yml", yaml);
```

### Directory Management

```csharp
// Create a structured directory system
var dirConfig = new DirectoriesConfig("/path/to/root");

// Access specific directories
string databaseDir = dirConfig[DirectoryType.Database];
string scriptsDir = dirConfig[DirectoryType.Scripts];
string logsDir = dirConfig[DirectoryType.Logs];
```

### Template Variables

```csharp
// Create a text template service
var templateService = new TextTemplateService(logger, signalService);

// Add static variables
templateService.AddVariable("hostname", "irc.example.com");
templateService.AddVariable("version", "1.0.0");

// Add dynamic variables with builders
templateService.AddVariableBuilder("uptime", () =>
    (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString());

// Process a template
string motd = "Welcome to {{hostname}} running version {{version}}!\nServer uptime: {{uptime}}";
string processed = templateService.TranslateText(motd);
```

### Utility Methods

```csharp
// String utilities
string snakeCase = "MyClassName".ToSnakeCase(); // "my_class_name"
string camelCase = "SERVER_NAME".ToCamelCase(); // "serverName"

// Hash utilities
string md5Hash = "test string".GetMd5Checksum();

// Base64 utilities
bool isBase64 = "SGVsbG8gV29ybGQ=".IsBase64String(); // true
string encoded = "Hello World".ToBase64(); // "SGVsbG8gV29ybGQ="

// Environment variable replacement
string withEnv = "User: {USER}".ReplaceEnvVariable(); // "User: john"

// Date/time utilities
long timestamp = DateTime.Now.ToUnixTimestamp();
DateTime fromTimestamp = 1609459200L.FromEpoch();
```

## Core Components

### Configuration Classes

- `AbyssIrcConfig`: Main server configuration
- `NetworkConfig`: Network-related settings
- `AdminConfig`: Administrator settings
- `LimitConfig`: Server limits and constraints
- `MotdConfig`: Message of the day settings

### Interfaces

- `IAbyssStarStopService`: Interface for services that can be started and stopped
- Various event-related interfaces that form the base of the signaling system

### Directory Organization

Structured directory types for server resources:

- `Root`: Main server directory
- `Database`: Data storage
- `Scripts`: JavaScript scripts
- `Cache`: Temporary files
- `Certs`: SSL/TLS certificates
- `Messages`: Templates and message files
- `Logs`: Server logs

## Dependency on Other AbyssIrc Packages

AbyssIrc.Core is designed to have minimal external dependencies and serves as a foundation for other AbyssIrc packages:

- **AbyssIrc.Network**: Builds on Core to provide IRC protocol implementation
- **AbyssIrc.Signals**: Uses Core interfaces to implement the event system
- **AbyssIrc.Server**: Combines all modules to create the complete IRC server

## Requirements

- .NET 9.0 or later
- Compatible with Windows, Linux, and macOS

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! If you're interested in improving AbyssIrc.Core:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

For more information on AbyssIrc and related projects, visit the [AbyssIrc GitHub repository](https://github.com/tgiachi/abyssirc-server).
