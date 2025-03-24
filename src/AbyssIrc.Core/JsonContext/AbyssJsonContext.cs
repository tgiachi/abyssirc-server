using System.Text.Json.Serialization;
using AbyssIrc.Core.Data.Configs;
using AbyssIrc.Core.Data.Configs.Sections;

namespace AbyssIrc.Core.JsonContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true)]
[JsonSerializable(typeof(AbyssIrcConfig))]
[JsonSerializable(typeof(AdminConfig))]
[JsonSerializable(typeof(NetworkConfig))]
public partial class AbyssJsonContext : JsonSerializerContext
{
}
