using System.Reflection;

namespace AbyssIrc.Core.Utils;

public static class ResourceUtils
{
    public static string? ReadEmbeddedResource(string resourceName, Assembly assembly)
    {
        var fullResourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(resourceName));

        if (fullResourceName == null)
        {
            throw new Exception($"Resource {resourceName} not found in assembly {assembly.FullName}");
        }

        using var stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        throw new Exception($"Resource {resourceName} not found in assembly {assembly.FullName}");
    }
}
