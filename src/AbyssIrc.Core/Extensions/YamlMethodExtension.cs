using AbyssIrc.Core.Utils;

namespace AbyssIrc.Core.Extensions;

public static class YamlMethodExtension
{
    public static string ToYaml(this object obj)
    {
        return YamlUtils.Serialize(obj);
    }

    public static T? FromYaml<T>(this string yaml)
    {
        return YamlUtils.Deserialize<T>(yaml);
    }
}
