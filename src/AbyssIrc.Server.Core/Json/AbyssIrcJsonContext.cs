using System.Text.Json.Serialization;
using AbyssIrc.Server.Core.Data.Config;

namespace AbyssIrc.Server.Core.Json;

[JsonSerializable(typeof(AbyssIrcServerConfig))]
public partial class AbyssIrcJsonContext : JsonSerializerContext
{

}
