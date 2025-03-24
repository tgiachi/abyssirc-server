using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AbyssIrc.Core.JsonContext;
using AbyssIrc.Core.Utils;

namespace AbyssIrc.Core.Extensions;

public static class JsonMethodExtension
{
    public static string ToJson<T>(this T obj, bool formatted = true)
    {
        var options = JsonUtils.GetDefaultJsonSettings();
        if (formatted)
        {
            options!.WriteIndented = true;
        }

        return JsonSerializer.Serialize(obj, options);
    }

    public static T FromJson<T>(this string json) =>
        JsonSerializer.Deserialize<T>(json, JsonUtils.GetDefaultJsonSettings())!;

    public static object FromJson(this string json, Type type) =>
        JsonSerializer.Deserialize(json, type, JsonUtils.GetDefaultJsonSettings())!;


    public static string ToJsonAot<T>(this T obj, params JsonSerializerContext[] contexts)
    {
        var defaultContext = new List<JsonSerializerContext> { AbyssJsonContext.Default };
        defaultContext.AddRange(contexts);

        foreach (var context in defaultContext)
        {
            var typeInfo = context.GetTypeInfo(typeof(T));
            if (typeInfo != null)
            {
                using var memoryStream = new MemoryStream();

                using var writer = new Utf8JsonWriter(
                    memoryStream,
                    new JsonWriterOptions
                    {
                        Indented = true,
                        SkipValidation = false,
                    }
                );

                JsonSerializer.Serialize(writer, obj, (JsonTypeInfo<T>)typeInfo);

                writer.Flush();


                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        throw new InvalidOperationException($"No context found for type {typeof(T).Name}");
    }

    public static T FromJsonAot<T>(this string json, params JsonSerializerContext[] contexts)
    {
        var defaultContext = new List<JsonSerializerContext> { AbyssJsonContext.Default };
        defaultContext.AddRange(contexts);

        foreach (var context in defaultContext)
        {
            var typeInfo = context.GetTypeInfo(typeof(T));
            if (typeInfo != null)
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);


                var reader = new Utf8JsonReader(jsonBytes);


                T? result = JsonSerializer.Deserialize(ref reader, (JsonTypeInfo<T>)typeInfo);

                if (result == null)
                {
                    throw new JsonException($"Failed to deserialize {typeof(T).Name}");
                }

                return result;
            }
        }

        throw new InvalidOperationException($"No context found for type {typeof(T).Name}");
    }
}
