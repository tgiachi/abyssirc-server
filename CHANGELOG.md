# Change Log

<a name="0.2.4"></a>
## [0.2.4](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.2.4) (2025-04-14)

<a name="0.2.3"></a>
## [0.2.3](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.2.3) (2025-04-07)

### Features

* added RPL_WHO, WhoCommand and WhoIsCommand ([d338b82](https://www.github.com/tgiachi/abyssirc-server/commit/d338b826e596dd0161c02ce176c7c85ecf54d6ea))
* **AbyssIrc.Core.csproj:** update TargetFrameworks to include netstandard2.1 ([9476f99](https://www.github.com/tgiachi/abyssirc-server/commit/9476f99b68af1271329b39e94f5b5bb27d963062))
* **AbyssIrc.Server.csproj:** add Swashbuckle.AspNetCore and Swashbuckle.AspNetCore.Swagger ([57a131c](https://www.github.com/tgiachi/abyssirc-server/commit/57a131c1b6c1d03492cbe19490baeab8cf192c4d))
* **Extensions:** add methods to convert base64 strings to byte arrays and vice versa ([cf7035b](https://www.github.com/tgiachi/abyssirc-server/commit/cf7035bcb2dbc6748018a36b0fa0487ba4d4242e))
* **LoginRequestData.cs:** add Required attribute to Username and Password properties for validation ([00e845f](https://www.github.com/tgiachi/abyssirc-server/commit/00e845f2f0529202805068722bfcdeb69e061535))
* **ModuleExtension.cs:** add ModuleExtension class to provide methods for adding modules to IServiceCollection ([0acec9f](https://www.github.com/tgiachi/abyssirc-server/commit/0acec9f6f46c0bbca8cb3ca8ba3ec93a64d28930))
* **WhoHandler.cs:** add support for handling Who and WhoIs commands for channels and nicknames to provide information about users on the server ([c95094c](https://www.github.com/tgiachi/abyssirc-server/commit/c95094c853a86904e25326475117da558dfeb789))

<a name="0.2.2"></a>
## [0.2.2](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.2.2) (2025-04-05)

### Features

* **AbyssIrc.Server.Core:** add support for loading and managing plugins ([2b12679](https://www.github.com/tgiachi/abyssirc-server/commit/2b126791f0fd5036ec1bbbfdaf434de80e2f8f3f))
* **IAbyssIrcPlugin.cs:** create IAbyssIrcPlugin interface for defining plugin structure ([9ad6873](https://www.github.com/tgiachi/abyssirc-server/commit/9ad687369731191b8542536043ab36bda224b0a5))
* **IPluginManagerService.cs:** add method LoadPlugin to load a specific plugin ([5a78c56](https://www.github.com/tgiachi/abyssirc-server/commit/5a78c56a6c8c35deb84e4873e1965d23ef2194b6))

### Bug Fixes

* **AbyssIrc.Server.csproj:** update Scalar.AspNetCore package version to 2.1.6 ([8545360](https://www.github.com/tgiachi/abyssirc-server/commit/8545360ddb47dc6c3da13c45ee0ef55aec1c9a81))

<a name="0.2.1"></a>
## [0.2.1](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.2.1) (2025-04-04)

### Features

* **WebServerConfig.cs:** set IsOpenApiEnabled default value to true for interactive API documentation ([4289767](https://www.github.com/tgiachi/abyssirc-server/commit/428976798b7f2acb1f58fb52e8212fadc54d8d71))

<a name="0.2.0"></a>
## [0.2.0](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.2.0) (2025-04-04)

### Features

* **AbyssIrc.Server.Core.csproj:** add Utils folder to project structure ([db57f94](https://www.github.com/tgiachi/abyssirc-server/commit/db57f9492c56824360772657c0ab649542108ca1))
* **ChannelsHandler.cs:** add support for handling AddUserJoinChannelEvent to allow users to join channels ([6d00d25](https://www.github.com/tgiachi/abyssirc-server/commit/6d00d259f1e45db0aee80b80aee305ae1cf7efc1))
* **README.md:** add milestone progress section for "Nautilus" release with core IRC features ([959fcd6](https://www.github.com/tgiachi/abyssirc-server/commit/959fcd682a78d15206cf4fd82422c34810cc43f0))

<a name="0.1.15"></a>
## [0.1.15](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.15) (2025-04-03)

### Features

* Added OperCommand, rewritted QuitCommand, ([c396942](https://www.github.com/tgiachi/abyssirc-server/commit/c3969429fc657c21ae5755e59b86be2c618bcc4f))
* **AbyssIrc.Protocol.Messages:** add KillCommand, RplEndOfWho, and WhoCommand classes to handle IRC protocol messages and commands ([34e1896](https://www.github.com/tgiachi/abyssirc-server/commit/34e1896226050dce2b1e551f90f5d4c089524b47))

### Bug Fixes

* names of RPL e ERR and added OPER and KILL messages ([a5d347c](https://www.github.com/tgiachi/abyssirc-server/commit/a5d347ca174ddf574161dd4348848a4a3157decc))

<a name="0.1.14"></a>
## [0.1.14](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.14) (2025-04-03)

### Bug Fixes

* **release.yml:** update setup-dotnet action to v4 to use the latest version of .NET for the workflow ([ef314e5](https://www.github.com/tgiachi/abyssirc-server/commit/ef314e522dace82316c49d585f75648c6c2f263a))

<a name="0.1.13"></a>
## [0.1.13](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.13) (2025-04-03)

### Bug Fixes

* **release.yml:** update tag_name to include '-release' suffix for better clarity and consistency ([205307b](https://www.github.com/tgiachi/abyssirc-server/commit/205307b5cbfe0f7dd03eb13ad6d776f89c6faa62))

<a name="0.1.12"></a>
## [0.1.12](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.12) (2025-04-03)

<a name="0.1.11"></a>
## [0.1.11](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.11) (2025-04-03)

<a name="0.1.10"></a>
## [0.1.10](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.10) (2025-04-03)

<a name="0.1.9"></a>
## [0.1.9](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.9) (2025-04-03)

### Features

* **release.yml:** add GitHub Actions workflow for creating releases and building artifacts ([1a75345](https://www.github.com/tgiachi/abyssirc-server/commit/1a7534509144e474b9332842bafc2ea41f4b8dfe))

### Bug Fixes

* **AbyssIrc.Core.csproj:** update YamlDotNet package version to 16.3.0 ([6663d34](https://www.github.com/tgiachi/abyssirc-server/commit/6663d341445a2fcb2be509c23e152c2f8f977ac1))

<a name="0.1.8"></a>
## [0.1.8](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.8) (2025-04-03)

### Features

* renamed AbyssIrc.Network to AbyssIrc.Protocol.Messages ([28aa425](https://www.github.com/tgiachi/abyssirc-server/commit/28aa4257517eef55add24bca804f57d9dc92b0b4))

<a name="0.1.7"></a>
## [0.1.7](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.7) (2025-04-02)

### Features

* **AbyssIrc.sln:** add AbyssIrc.Server.Core project to the solution ([c96ff08](https://www.github.com/tgiachi/abyssirc-server/commit/c96ff086bbc6095626106ec3f5e6e59130858a6c))
* **server:** add new files for core project structure and configurations ([9378ab7](https://www.github.com/tgiachi/abyssirc-server/commit/9378ab77ca7dae4247c23349159e508d2d24c23c))

### Bug Fixes

* **AbyssIrc.Server.Core.csproj:** update project version from 0.1.0 to 0.1.6 ([598e282](https://www.github.com/tgiachi/abyssirc-server/commit/598e28225e5ad49869522d0c711f719ed3e55f23))

<a name="0.1.6"></a>
## [0.1.6](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.6) (2025-04-02)

### Features

* **RplBanList.cs:** add RPL_BANLIST command class to handle ban list replies in IRC protocol ([4b71578](https://www.github.com/tgiachi/abyssirc-server/commit/4b715780adf26bf7225de8dadc17e03cf8e96f9d))
* **TimeHandler.cs:** add TimeHandler class to handle TimeCommand and send time response ([0395077](https://www.github.com/tgiachi/abyssirc-server/commit/039507755666c2678b8ae585a557a342585fd450))

<a name="0.1.5"></a>
## [0.1.5](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.5) (2025-04-02)

### Features

* **AbyssIrc.Core.csproj:** add Humanizer.Core package reference for human-readable values ([b49de55](https://www.github.com/tgiachi/abyssirc-server/commit/b49de5566ad98b6f5a84be42e3b18fd5d6e13f0c))
* **AbyssIrcConfig.cs:** add ProcessQueueConfig property to AbyssIrcConfig class ([28a478c](https://www.github.com/tgiachi/abyssirc-server/commit/28a478c9bccfe5b2a6b7d085d8caf42ea7c09007))
* **core:** add DiagnosticMetricEvent record to handle diagnostic metrics in the core ([451bb30](https://www.github.com/tgiachi/abyssirc-server/commit/451bb3008e726ed8faa6d8c6d1f4563a40283b07))
* **ErrUserOnChannelCommand.cs:** add new class ErrUserOnChannelCommand to handle ERR_USERONCHANNEL error response ([ec57eef](https://www.github.com/tgiachi/abyssirc-server/commit/ec57eefd018cb227f170c049ceea68ba2e9f3d95))
* **JsLoggerModule.cs:** add JsLoggerModule to handle logging from JavaScript scripts ([71d2026](https://www.github.com/tgiachi/abyssirc-server/commit/71d20260473cd8ba69b872132777cdd4f97fcbc0))
* **KickCommand.cs:** add KickCommand class to handle kicking users from a channel ([3bf316a](https://www.github.com/tgiachi/abyssirc-server/commit/3bf316aa8224c15e7575c9001f3656b8faf8cd58))

### Bug Fixes

* **tests:** update NUnit.Analyzers package version to 4.7.0 to stay up to date with the latest features and improvements ([a567f1f](https://www.github.com/tgiachi/abyssirc-server/commit/a567f1fdc7efd385ac52eb682b18417c8317e5f8))

<a name="0.1.4"></a>
## [0.1.4](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.4) (2025-04-01)

### Features

* **AbyssIrc.Network:** enhance RplCreated class to support parsing and storing ([17b74d6](https://www.github.com/tgiachi/abyssirc-server/commit/17b74d6917a2addf26c3e6e3cee62336d02ef609))
* **ChannelData.cs:** add methods to set topic protection, anti-spam control, only allow messages from users with voice, and remove topic protection ([d9c055d](https://www.github.com/tgiachi/abyssirc-server/commit/d9c055d9fc34cba1768f813ddca1583c250a787b))
* **IChannelManagerService.cs:** add GetChannelsOfNickname method to retrieve a list of channels a nickname is part of ([f5e65b2](https://www.github.com/tgiachi/abyssirc-server/commit/f5e65b2aea43e2f596152edbaa117c61b8e18291))
* **LimitConfig.cs:** increase MaxModes property value from 6 to 12 for better flexibility ([d93bd04](https://www.github.com/tgiachi/abyssirc-server/commit/d93bd04b12a4483f6c8d18af1ceecdbea5b1a5d7))
* **PrivMsgCommand.cs:** refactor Parse method to improve readability and error handling ([6c3c3be](https://www.github.com/tgiachi/abyssirc-server/commit/6c3c3bea46e7c7ff97420d651dfe27f3d5e242f4))
* **RplMotd.cs:** improve parsing logic to remove leading "- " from Text property ([ceec42c](https://www.github.com/tgiachi/abyssirc-server/commit/ceec42cc223ac54c229b179c6c506edba4f73952))

<a name="0.1.3"></a>
## [0.1.3](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.3) (2025-03-31)

### Features

* **AbyssIrc.Network:** add ListFilterType enum to define filter types for LIST command ([4d364e7](https://www.github.com/tgiachi/abyssirc-server/commit/4d364e778a2f4560a5e981964e494de6ccfad463))
* **ErrNoPrivileges.cs:** add new class ErrNoPrivileges to handle IRC ERR_NOPRIVILEGES error response ([ed77308](https://www.github.com/tgiachi/abyssirc-server/commit/ed7730824d73f79dc834eccc4146d6ed3d94321c))

### Bug Fixes

* **ChannelsHandler.cs:** fix the order of arguments in AddNicknameToChannel method call to match the method signature ([6b3c846](https://www.github.com/tgiachi/abyssirc-server/commit/6b3c8463d714bb94ed77b086eb02ddec41f5e1fb))

<a name="0.1.2"></a>
## [0.1.2](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.2) (2025-03-31)

### Features

* **ErrChanOpPrivsNeeded.cs:** add new class representing IRC ERR_CHANOPRIVSNEEDED error response ([51cf243](https://www.github.com/tgiachi/abyssirc-server/commit/51cf243bd7aa559ae4e48da23415438558034b24))
* **ErrNotOnChannel.cs:** add new class ErrNotOnChannel to handle ERR_NOTONCHANNEL error response in IRC protocol ([7644a36](https://www.github.com/tgiachi/abyssirc-server/commit/7644a363a8c985d0ebe22b50394f2e0ad4a551eb))

<a name="0.1.1"></a>
## [0.1.1](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.1) (2025-03-31)

### Features

* **AbyssIrc.Network:** delete unused reply command files to declutter the project and improve maintainability ([c74ee26](https://www.github.com/tgiachi/abyssirc-server/commit/c74ee26ef1e751513423410ffd0441eaaab2a31b))
* **ChannelData.cs:** add methods to get prefixed nickname and member list for channels ([3fd41e1](https://www.github.com/tgiachi/abyssirc-server/commit/3fd41e18830150a53cac00beeae73a8dd2c79e6b))
* **ChannelManagerService:** implement methods to register channels, add and remove nicknames to channels ([fc950c4](https://www.github.com/tgiachi/abyssirc-server/commit/fc950c423e95009cc0fe4031f1785a657cede37e))
* **ErrAlreadyInChannel.cs:** add new class ErrAlreadyInChannel to handle ERR_USERONCHANNEL error response ([963d13b](https://www.github.com/tgiachi/abyssirc-server/commit/963d13b2e0d2a55d7857e5921c8e5d7cdb4cb477))
* **ErrBadChanMask.cs, ErrBadChannelKey.cs, ErrBannedFromChan.cs, ErrChannelIsFull.cs, ErrInviteOnlyChan.cs, ErrTooManyChannels.cs:** Add new error classes to handle specific IRC error responses and their parsing and formatting functionalities. ([b10b479](https://www.github.com/tgiachi/abyssirc-server/commit/b10b479fdb0bb627f59ba5a050b86eecba331752))
* **IChannelManagerService.cs:** create new interface IChannelManagerService to define channel management operations ([a02ceb6](https://www.github.com/tgiachi/abyssirc-server/commit/a02ceb67baa50cd465bb07b3d9c866d44828f0fc))
* **JoinCommand.cs:** refactor JoinCommand to use JoinChannelData class for channels and keys ([21cd846](https://www.github.com/tgiachi/abyssirc-server/commit/21cd8462a5e269b7b7d4629b7a9403f8b38677d9))
* **NamesCommand.cs:** improve comments and semantics for NamesCommand class ([6348403](https://www.github.com/tgiachi/abyssirc-server/commit/6348403bf8f4852e4050aea93506e478230bc613))
* **Program.cs:** add NamesCommand to ChannelsHandler for handling IRC commands ([32e3c76](https://www.github.com/tgiachi/abyssirc-server/commit/32e3c762945be05605b6eb7fece3c020d24479d7))
* **replies:** add new classes for RPL_CREATED, RPL_ENDOFINFO, RPL_ENDOFNAMES, ([9d56bb6](https://www.github.com/tgiachi/abyssirc-server/commit/9d56bb6b6ac6c8b36a865851d420b2b06ed88bfc))
* **RplNamReply.cs:** add RPL_NAMREPLY command to list users in a channel ([6ddd977](https://www.github.com/tgiachi/abyssirc-server/commit/6ddd97758c21f28ea15a68142604d9868a338954))

<a name="0.1.0"></a>
## [0.1.0](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.1.0) (2025-03-30)

<a name="0.0.23"></a>
## [0.0.23](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.23) (2025-03-30)

<a name="0.0.22"></a>
## [0.0.22](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.22) (2025-03-30)

<a name="0.0.21"></a>
## [0.0.21](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.21) (2025-03-30)

<a name="0.0.20"></a>
## [0.0.20](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.20) (2025-03-30)

<a name="0.0.19"></a>
## [0.0.19](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.19) (2025-03-30)

<a name="0.0.19"></a>
## [0.0.19](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.19) (2025-03-30)

<a name="0.0.18"></a>
## [0.0.18](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.18) (2025-03-30)

### Features

* **AbyssIrc.Network:** add AdminCommand, InfoCommand, RplEndOfInfoCommand, ([a479a03](https://www.github.com/tgiachi/abyssirc-server/commit/a479a0370f147370ffcef9141fe7c9be1a9780bb))
* **AbyssIrc.Network.csproj:** add Microsoft.Extensions.Logging.Abstractions package reference ([e11c5dc](https://www.github.com/tgiachi/abyssirc-server/commit/e11c5dc2da76f67ee76cd8bb709d17fa84dc6d67))
* **ErrAlreadyRegisteredCommand.cs:** add new class representing ERR_ALREADYREGISTERED error ([dd260da](https://www.github.com/tgiachi/abyssirc-server/commit/dd260da2d4917ab1850bd501611b4d1362d9d795))
* **ListCommand.cs:** add ListCommand class to handle IRC LIST command for listing channels ([6ae2f22](https://www.github.com/tgiachi/abyssirc-server/commit/6ae2f22f04d51a7953fb41d1a7724e48a3991982))
* **nuget-publish.yml:** add GitHub workflow for publishing NuGet packages on version tags ([cc7062b](https://www.github.com/tgiachi/abyssirc-server/commit/cc7062b857d4b67adcf2fb1bcfb1edd7a390a1fe))

### Bug Fixes

* **PassHandler.cs:** handle case where password is empty by sending ErrNeedMoreParamsCommand ([91af621](https://www.github.com/tgiachi/abyssirc-server/commit/91af62147fc30ed2a19c6a2baa8dd819c9ad069b))

<a name="0.0.17"></a>
## [0.0.17](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.17) (2025-03-29)

### Features

* **ErrNeedMoreParamsCommand.cs:** add new class ErrNeedMoreParamsCommand to handle ERR_NEEDMOREPARAMS numeric reply in IRC protocol ([5a48551](https://www.github.com/tgiachi/abyssirc-server/commit/5a485518fb3ece9d659d54ce68439986a93cc8d9))
* **ErrNoPrivilegesCommand.cs:** add new class ErrNoPrivilegesCommand to handle unauthorized RESTART attempts ([3c1d375](https://www.github.com/tgiachi/abyssirc-server/commit/3c1d375007f327f5e58b1e61c75402a77279b61a))
* **license:** add MIT License text to the project for legal clarity and permissions ([2a25b89](https://www.github.com/tgiachi/abyssirc-server/commit/2a25b89573566093c3a0a6fc99b2b0c2f8a98f55))
* **ModeCommand.cs:** add support for determining mode target type based on the first character in the target string ([c745a75](https://www.github.com/tgiachi/abyssirc-server/commit/c745a7552aa459f33abbaf4621c075a0f208f37a))
* **OperEntryTests.cs:** add unit tests for OperEntry class to ensure proper creation and handling of data ([df7f8ed](https://www.github.com/tgiachi/abyssirc-server/commit/df7f8ed6832a188c50eb3c59ba2157ca917a861f))
* **RplMyInfoCommand.cs:** add RPL_MYINFO command to provide server information to clients ([423bc6f](https://www.github.com/tgiachi/abyssirc-server/commit/423bc6f6e197401877e10b1793e4dac019cc6586))
* **ServerRestartRequestEvent.cs:** add new event class to handle server restart requests ([8bdc92f](https://www.github.com/tgiachi/abyssirc-server/commit/8bdc92f8cb46da3ded52132d245dfb8109d68d1d))

### Bug Fixes

* **abysslogo.png:** update abysslogo.png file to resolve binary file differences ([31366e2](https://www.github.com/tgiachi/abyssirc-server/commit/31366e214deb13ef96a0e62211aa0d386bf2e9d0))
* **README.md:** update license information to specify MIT license for the project ([0bb1a35](https://www.github.com/tgiachi/abyssirc-server/commit/0bb1a35d379114b1e8ed445d9d3a8b42e843b72c))

<a name="0.0.16"></a>
## [0.0.16](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.16) (2025-03-28)

### Features

* **UserhostCommand.cs:** add UserhostCommand class to handle IRC USERHOST command ([644a639](https://www.github.com/tgiachi/abyssirc-server/commit/644a6392277969225990bc7530de3259652ee52b))

<a name="0.0.15"></a>
## [0.0.15](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.15) (2025-03-28)

### Features

* **MircNotationConverter.cs:** add MircNotationConverter utility class to convert text formatting ([bd131e6](https://www.github.com/tgiachi/abyssirc-server/commit/bd131e6a6048ec8748d98d32b907d8406012042b))
* **RegisterScriptModuleExtension.cs:** add support for registering script modules with a specified module type ([d3b6c9b](https://www.github.com/tgiachi/abyssirc-server/commit/d3b6c9b87c1e8cd566cb20158a5d292fd8bbc617))

<a name="0.0.14"></a>
## [0.0.14](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.14) (2025-03-28)

### Features

* **IsonCommand.cs:** add new ISON command class to handle checking online status of specific users ([03e50ee](https://www.github.com/tgiachi/abyssirc-server/commit/03e50ee128afb261887a417a810ffa6019aa3e84))
* **Program.cs:** add additional log file for specific logs and filter by specific namespaces ([bc6705b](https://www.github.com/tgiachi/abyssirc-server/commit/bc6705b5343194a4fddef2aeb8d6541f1a015470))
* **README.md:** update project name to 'AbyssIRC Server' for clarity and consistency ([fe06b01](https://www.github.com/tgiachi/abyssirc-server/commit/fe06b01b90c7d1a5fc3f0eb2759cc0a82ff9feb8))
* **TcpService.cs:** add support for differentiating between Plain and Secure ([c1eb3cf](https://www.github.com/tgiachi/abyssirc-server/commit/c1eb3cf162dc77d1dc316dfd88d2ef55e6bd664a))

<a name="0.0.13"></a>
## [0.0.13](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.13) (2025-03-28)

### Features

* **AbyssIrc.Core:** add YamlDotNet package reference to project for YAML support ([8bfddf4](https://www.github.com/tgiachi/abyssirc-server/commit/8bfddf4f3b9bf83cf13901e87484e230a2cab30a))
* **LimitConfig.cs:** add new properties to LimitConfig class for maximum silence, maximum modes, maximum away message length, case mapping, maximum channels a user can join, status message, and ELIST parameter ([b9cdce2](https://www.github.com/tgiachi/abyssirc-server/commit/b9cdce20ce24d73bd8351ab3a4d86e95598fd680))
* **RplISupportCommand.cs:** add RplISupportCommand class to handle IRC RPL_ISUPPORT ([a27522f](https://www.github.com/tgiachi/abyssirc-server/commit/a27522f4065bfc0b3f09f146883164a29d690e8f))
* **server:** add AbyssServerData class to store server hostname ([4fd7032](https://www.github.com/tgiachi/abyssirc-server/commit/4fd703230df6b28f471d1cee296cc20748cd9f62))

### Bug Fixes

* **README.md:** update AbyssIrc repository URL in CI/CD badge to point to the correct repository ([fbd1b87](https://www.github.com/tgiachi/abyssirc-server/commit/fbd1b87c9542fa0cccdeddbe90661b24ff71c9bc))

<a name="0.0.12"></a>
## [0.0.12](https://www.github.com/tgiachi/abyssirc-server/releases/tag/v0.0.12) (2025-03-27)

### Features

* **ErrErroneusNicknameCommand.cs:** add new class representing ERR_ERRONEUSNICKNAME error response ([02c796b](https://www.github.com/tgiachi/abyssirc-server/commit/02c796b90a1179edc83057062a029b247d3124dc))
* **ErrUnknownCommandCommand.cs:** add empty line for better code readability ([2834e9c](https://www.github.com/tgiachi/abyssirc-server/commit/2834e9c800f7939acc773cd53435d5b401fb7864))

### Bug Fixes

* formatted and intended ([b6002c7](https://www.github.com/tgiachi/abyssirc-server/commit/b6002c7075349553310cc26da0a697c1b84bd0db))

<a name="0.0.11"></a>
## [0.0.11](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.11) (2025-03-27)

<a name="0.0.10"></a>
## [0.0.10](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.10) (2025-03-27)

### Features

* **.gitignore:** add ignore rule for irc_certs directory ([7eb6f54](https://www.github.com/tgiachi/AbyssIrc/commit/7eb6f54282a0cfa7c7130f9637a6faed7d614111))
* **AbyssIrc.Network:** add new classes for various IRC numeric replies ([8771c4c](https://www.github.com/tgiachi/AbyssIrc/commit/8771c4c3e510418ad2eaed00119d8bc14e8909d3))
* **certs:** add script to generate SSL certificates for IRC server ([1b1ffe4](https://www.github.com/tgiachi/AbyssIrc/commit/1b1ffe40d202a34672edaea451b5d738cffe1a73))
* **certs:** add script to generate SSL certificates for IRC server ([75bcd2f](https://www.github.com/tgiachi/AbyssIrc/commit/75bcd2f0966fff3edb41981ca4b24c6e0dbff775))
* **RplEndOfInviteList.cs:** add new class representing RPL_ENDOFINVITELIST ([2404abb](https://www.github.com/tgiachi/AbyssIrc/commit/2404abbf59f1cabf2ca8709c23f2be6327d956ed))

<a name="0.0.9"></a>
## [0.0.9](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.9) (2025-03-27)

### Features

* **commands:** add new reply classes for various IRC numeric replies ([e5af9d6](https://www.github.com/tgiachi/AbyssIrc/commit/e5af9d63a4cc7111e8b96532e7b5eb9406d36065))
* **ErrNicknameInUse.cs:** add new class ErrNicknameInUse to handle ERR_NICKNAMEINUSE numeric reply ([818190c](https://www.github.com/tgiachi/AbyssIrc/commit/818190c67edd09abfee3e2c028a94cd406d27b26))

<a name="0.0.8"></a>
## [0.0.8](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.8) (2025-03-26)

### Bug Fixes

* **Dockerfile:** remove unnecessary --self-contained flag from dotnet publish command to prevent unnecessary bundling of runtime with the application ([9fbac90](https://www.github.com/tgiachi/AbyssIrc/commit/9fbac90181a91f517284740ad02b69cd24a59e72))

<a name="0.0.7"></a>
## [0.0.7](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.7) (2025-03-26)

### Features

* **Dockerfile:** switch base and build images to Alpine versions for smaller image size ([57e1c15](https://www.github.com/tgiachi/AbyssIrc/commit/57e1c15043d29d98ff8af620230775c0a9cc6577))

<a name="0.0.6"></a>
## [0.0.6](https://www.github.com/tgiachi/AbyssIrc/releases/tag/v0.0.6) (2025-03-26)

### Features

* **AbyssIrc.Core.csproj:** update PackageReference formatting for better readability ([b1d385a](https://www.github.com/tgiachi/AbyssIrc/commit/b1d385ae101b41d82b8271410fed52ab005a6879))
* **cluster:** add docker-compose.yml for setting up IRC servers and HAProxy ([9b71d95](https://www.github.com/tgiachi/AbyssIrc/commit/9b71d95525de3ca881e77cc40c810386c1dd99c5))
* **ErrNoSuchNick.cs:** add constructor to initialize ErrNoSuchNick properties ([e4b6a78](https://www.github.com/tgiachi/AbyssIrc/commit/e4b6a78eadbff591cd805d30b9e13cbe8e181815))
* **scripts:** add HookEvent method to events object to allow hooking into custom events ([497ab26](https://www.github.com/tgiachi/AbyssIrc/commit/497ab26f0d015f8be43314a584f3c4005af2bb79))

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

