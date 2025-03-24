using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbyssIrc.Core.Utils;

public static class JsonUtils
{
    private static JsonSerializerOptions? _jsonSerializerOptions;

    public static JsonSerializerOptions? GetDefaultJsonSettings(bool formatted = true) =>
        _jsonSerializerOptions ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = formatted,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
        };
}
