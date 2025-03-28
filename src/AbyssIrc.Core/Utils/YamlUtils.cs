namespace AbyssIrc.Core.Utils;

public static class YamlUtils
{
    public static T? Deserialize<T>(string yaml)
    {
        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.LowerCaseNamingConvention.Instance)
            .Build();

        return deserializer.Deserialize<T>(yaml);
    }

    public static string Serialize<T>(T obj)
    {
        var serializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.LowerCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(obj);
    }
}
