using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AbyssIrc.Core.Json;

public static class JsonUtils
{
    private static readonly List<IJsonTypeInfoResolver> JsonSerializerContexts = new();

    private static JsonSerializerOptions _jsonSerializerOptions = null!;

    static JsonUtils()
    {
        RebuildJsonSerializerContexts();
    }

    public static List<JsonConverter> JsonConverters { get; } = new()
    {
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    };

    public static void AddJsonConverter(JsonConverter converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        JsonConverters.Add(converter);
        _jsonSerializerOptions.Converters.Add(converter);
    }

    private static void RebuildJsonSerializerContexts()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = JsonTypeInfoResolver.Combine(JsonSerializerContexts.ToArray())
        };

        foreach (var converter in JsonConverters)
        {
            _jsonSerializerOptions.Converters.Add(converter);
        }
    }

    public static void RegisterJsonContext(JsonSerializerContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        JsonSerializerContexts.Add(context);
        RebuildJsonSerializerContexts();
    }


    public static string Serialize<T>(T obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        return JsonSerializer.Serialize(obj, typeof(T), _jsonSerializerOptions);
    }

    public static T Deserialize<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions) ??
               throw new JsonException("Deserialization failed.");
    }

    public static T DeserializeFromFile<T>(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"The file '{filePath}' does not exist.");
        }

        var json = File.ReadAllText(filePath);
        return Deserialize<T>(json);
    }


    public static void SerializeToFile<T>(T obj, string filePath)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(filePath);

        var json = Serialize(obj);
        File.WriteAllText(filePath, json);
    }
}
