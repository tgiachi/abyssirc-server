using System.Text.Json.Serialization;
using AbyssIrc.Server.Core.Data.Config;
using AbyssIrc.Server.Core.Data.Config.Sections;

namespace AbyssIrc.Server.Core.Json.Context;

[JsonSerializable(typeof(AbyssIrcServerConfig))]
[JsonSerializable(typeof(NetworkServerConfig))]
[JsonSerializable(typeof(NetworkBindConfig))]
[JsonSerializable(typeof(SslServerConfig))]
[JsonSerializable(typeof(HostServerConfig))]
public partial class AbyssIrcJsonContext : JsonSerializerContext
{
}
