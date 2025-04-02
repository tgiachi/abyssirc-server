
using System.Text.Json;

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
}
