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
        var defaultContext = new List<JsonSerializerContext> { new AbyssJsonContext() };

        defaultContext.AddRange(contexts);
        foreach (var context in defaultContext)
        {
            var typeInfo = context.GetTypeInfo(typeof(T));
            if (typeInfo != null)
            {
                return JsonSerializer.Serialize(obj, (JsonTypeInfo<T>)typeInfo);
            }
        }

        throw new InvalidOperationException($"No context found for type {typeof(T).Name}");
    }

    public static T FromJsonAot<T>(this string json, params JsonSerializerContext[] contexts)
    {
        var defaultContext = new List<JsonSerializerContext> { new AbyssJsonContext() };

        defaultContext.AddRange(contexts);
        foreach (var context in defaultContext)
        {
            var typeInfo = context.GetTypeInfo(typeof(T));
            if (typeInfo != null)
            {
                return JsonSerializer.Deserialize<T>(json, (JsonTypeInfo<T>)typeInfo)!;
            }
        }

        throw new InvalidOperationException($"No context found for type {typeof(T).Name}");
    }
}
