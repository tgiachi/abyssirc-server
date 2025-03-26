# Change Log

<a name="0.0.5"></a>
## [0.0.5](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.5) (2025-03-26)

### Features

* **README.md:** add detailed information about AbyssIRC project, including features, ([622ed6e](https://www.github.com/tgiachi/AbyssIrc/commit/622ed6e9925ebaaae52f4167dcac77d776a3170d))

<a name="0.0.4"></a>
## [0.0.4](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.4) (2025-03-26)

### Features

* add PingPongHandler to handle PING and PONG commands for connection monitoring ([398d082](https://www.github.com/tgiachi/AbyssIrc/commit/398d08263303d82340b25ffbe06ae14d5a90765d))
* **AbyssIrc:** add AbyssIrc.Signals project to the solution for signal handling ([56107b8](https://www.github.com/tgiachi/AbyssIrc/commit/56107b862ef5dd7beed1494b7ff1aa78e859377f))
* **AbyssIrc:** add LimitConfig class to store various server limits ([fc357f8](https://www.github.com/tgiachi/AbyssIrc/commit/fc357f8c2fcf87d597baa6236aa54fd64630fec1))
* **AbyssIrc.Core:** add new event classes for scheduler and variables ([c19423d](https://www.github.com/tgiachi/AbyssIrc/commit/c19423d4e28e57d36a1cd3f1cc27b976e3c78230))
* **AbyssIrc.Core.csproj:** add Microsoft.Extensions.DependencyInjection.Abstractions package ([d4e7a35](https://www.github.com/tgiachi/AbyssIrc/commit/d4e7a35a9fa740d5b9053eac21e361dc09b38bfe))
* **AbyssIrc.Core.csproj:** add new folder 'Data\Internal\' to project structure ([1d0ae24](https://www.github.com/tgiachi/AbyssIrc/commit/1d0ae24c62ca183a92917fb097092f08daabee6b))
* **AbyssIrc.Network:** add project reference to AbyssIrc.Core.csproj for better code organization ([5fbe5ed](https://www.github.com/tgiachi/AbyssIrc/commit/5fbe5ed964d468a69703e12cc51adf8be0a30a0b))
* **AbyssIrc.Server:** reorganize project references to improve structure ([d19867b](https://www.github.com/tgiachi/AbyssIrc/commit/d19867bd996e3a497fa0f2db558dc9c9c2953294))
* **AbyssIrc.sln:** add AbyssIrc.Tests project to solution ([d856ea0](https://www.github.com/tgiachi/AbyssIrc/commit/d856ea060292d4be233402d7377e44f2aae287c5))
* **AbyssIrc.sln:** move AbyssIrc.Network project to src folder for better project structure ([6e2a409](https://www.github.com/tgiachi/AbyssIrc/commit/6e2a4091d241725cec2d6fd25786de1185822ca1))
* **AbyssIrcConfig.cs:** add MotdConfig class to store Message of the Day configuration ([4f0bd11](https://www.github.com/tgiachi/AbyssIrc/commit/4f0bd11c0ccc0bff8e49df8763d4e7b837ca2c39))
* **config:** add new config file for AbyssIRC network settings ([123a8b1](https://www.github.com/tgiachi/AbyssIrc/commit/123a8b1bcc1d8e6848e352ea1768e279a6a1fbde))
* **config:** update MOTD in config.json to point to motd.txt file for better customization and readability ([cb8ecda](https://www.github.com/tgiachi/AbyssIrc/commit/cb8ecdaa4a4a8eed7daa16ac2aebf53bc075835e))
* **config.json:** add new configuration options for ping timeout, ping interval, ([075d28e](https://www.github.com/tgiachi/AbyssIrc/commit/075d28e1cda1515edfa3bff6d76a2fb386b05ede))
* **core:** add 'Certs' directory type to improve organization and clarity ([6649e7c](https://www.github.com/tgiachi/AbyssIrc/commit/6649e7cdfc22a427d168c4999da4d400f205f716))
* **Dockerfile:** remove unnecessary publish options to improve build efficiency ([050d181](https://www.github.com/tgiachi/AbyssIrc/commit/050d1818bc5b56fb6d4bc3ded8c89afe7fccde08))
* **ErrCannotSendToChan.cs, ErrNoRecipient.cs, ErrNoSuchNick.cs, ErrNoSuchServer.cs, ErrNoTextToSend.cs, ErrTooManyTargets.cs, PrivMsgCommand.cs, RplAway.cs:** add new IRC command classes and replies for handling various error scenarios and messages ([2f5663c](https://www.github.com/tgiachi/AbyssIrc/commit/2f5663cdc56bf8b08270b9d06475da8c30a41074))
* **IrcCommandParser.cs:** add Stopwatch to measure command parsing time ([9530d80](https://www.github.com/tgiachi/AbyssIrc/commit/9530d80db78668260f522b7999b76c6ea274cca1))
* **IrcHandlerDefinitionData.cs:** add MessageType property to IrcHandlerDefinitionData record for improved data representation ([1593f49](https://www.github.com/tgiachi/AbyssIrc/commit/1593f4946bf071dfb909b8c7fc9c35f0adee4952))
* **motd.txt:** update MOTD text with ASCII art logo and server information ([5730fac](https://www.github.com/tgiachi/AbyssIrc/commit/5730fac1938399e16c7e572774bcdb89fa22de3c))
* **NetworkConfig.cs:** change Port and SslPort properties to string type for better consistency and compatibility with configuration files ([908f7b6](https://www.github.com/tgiachi/AbyssIrc/commit/908f7b660ef58cc37e452687442d25ea68d2c196))
* **NoticeAuthCommand.cs:** add new class NoticeAuthCommand for handling server NOTICE AUTH messages during connection ([abf175b](https://www.github.com/tgiachi/AbyssIrc/commit/abf175b52b6fa0110d6a88fa90fc49e248fd3210))
* **QuitCommand.cs:** add QuitCommand class to handle IRC QUIT command and parse messages ([abc0669](https://www.github.com/tgiachi/AbyssIrc/commit/abc06690e6bf082f5d5a298cde8bac493c8d7ebe))
* **RegisterIrcCommandExtension.cs:** add RegisterIrcCommand method to register IRC commands easily ([a235c75](https://www.github.com/tgiachi/AbyssIrc/commit/a235c753be5546f2d15ff8f5fd66d2d5d62c85b6))
* **server:** add new SendIrcMessageEvent class to handle sending IRC messages ([15e5f49](https://www.github.com/tgiachi/AbyssIrc/commit/15e5f493750a1f65fe72f7c7d06284fc14664fc0))

### Bug Fixes

* **csproj:** update project versions to 0.0.3 for AbyssIrc.Core, AbyssIrc.Network, AbyssIrc.Server, and AbyssIrc.Signals to align with latest changes and improvements ([713816f](https://www.github.com/tgiachi/AbyssIrc/commit/713816f883ce5e34eded5cf3d7c9810d5529ca99))
* **Dockerfile:** fix indentation issue in the dotnet publish command to maintain consistency and readability ([94d11a1](https://www.github.com/tgiachi/AbyssIrc/commit/94d11a1dc854953dc233bcc1f839ba874e8e37b7))
* **Dockerfile:** remove extra whitespace to maintain consistency in the Dockerfile configuration. ([a2ba0b7](https://www.github.com/tgiachi/AbyssIrc/commit/a2ba0b78a000b546466750f1fb92fd399222eee1))

