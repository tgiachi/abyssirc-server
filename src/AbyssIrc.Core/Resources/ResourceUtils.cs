using System.Reflection;
using System.Text.RegularExpressions;

namespace AbyssIrc.Core.Resources;

/// <summary>
///     Provides utilities for working with embedded resources.
/// </summary>
public static partial class ResourceUtils
{
    /// <summary>
    ///     Reads the content of an embedded resource as a string.
    /// </summary>
    /// <param name="resourceName">The name of the resource to read.</param>
    /// <param name="assembly">The assembly containing the resource.</param>
    /// <returns>The content of the resource as a string.</returns>
    /// <exception cref="Exception">Thrown when the resource cannot be found in the specified assembly.</exception>
    /// <remarks>
    ///     This method handles resource names that may contain either forward slashes (/) or
    ///     backslashes (\) by converting them to dots, which is the standard separator for
    ///     resource names in .NET assemblies.
    /// </remarks>
    public static string? ReadEmbeddedResource(string resourceName, Assembly assembly)
    {
        var resourcePath = resourceName.Replace('/', '.').Replace('\\', '.');

        var fullResourceName = assembly.GetManifestResourceNames()
                                       .FirstOrDefault(name => name.EndsWith(resourcePath));

        if (fullResourceName == null)
        {
            throw new Exception($"Resource {resourceName} not found in assembly {assembly.FullName}");
        }

        using var stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream == null)
        {
            throw new Exception($"Resource {resourceName} not found in assembly {assembly.FullName}");
        }

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    /// <summary>
    ///     Gets a list of all embedded resources that match a given pattern
    /// </summary>
    /// <param name="assembly">The assembly to search in (if null, uses current assembly)</param>
    /// <param name="directoryPath">Directory path to search (e.g. "Assets.Templates")</param>
    /// <returns>A list of resource names found</returns>
    public static IEnumerable<string> GetEmbeddedResourceNames(Assembly assembly = null, string directoryPath = null)
    {
        // If no assembly is specified, use the current one
        assembly ??= Assembly.GetExecutingAssembly();

        // Get all resources in the assembly
        var resourceNames = assembly.GetManifestResourceNames();

        // If no directory path is specified, return all resources
        if (string.IsNullOrEmpty(directoryPath))
        {
            return resourceNames;
        }

        // Replace any path separators with dots, as required by the embedded resource format
        var normalizedPath = directoryPath.Replace('/', '.').Replace('\\', '.');

        // If it doesn't end with a dot, add one to ensure we're looking for that specific path
        if (!normalizedPath.EndsWith("."))
        {
            normalizedPath += ".";
        }

        // Filter resources that contain the specified path
        return resourceNames.Where(name => name.Contains(normalizedPath));
    }

    public static string EmbeddedNameToPath(string resourceName, string assemblyPrefix)
    {
        if (resourceName.StartsWith(assemblyPrefix + ".", StringComparison.Ordinal))
        {
            resourceName = resourceName[(assemblyPrefix.Length + 1)..];
        }

        return resourceName.Replace('.', '/');
    }

    /// <summary>
    ///     Gets a list of all files in a specific embedded directory
    /// </summary>
    /// <param name="assembly">The assembly to search in (if null, uses current assembly)</param>
    /// <param name="directoryPath">Directory path to search (e.g. "Assets/Templates")</param>
    /// <returns>A list of file names (without the full path)</returns>
    public static IEnumerable<string> GetEmbeddedResourceFileNames(
        Assembly assembly = null,
        string directoryPath = "Assets/Templates"
    )
    {
        // Normalize the path for embedded resource format
        var normalizedPath = directoryPath.Replace('/', '.').Replace('\\', '.');

        // Get all resources in the specified path
        var resources = GetEmbeddedResourceNames(assembly, normalizedPath);

        // Extract file names from the full paths
        var fileNames = new List<string>();

        foreach (var resource in resources)
        {
            // Extract the final part of the resource name (file name with extension)
            var fileName = resource.Substring(resource.LastIndexOf('.') + 1);

            // If not empty, add it to the list
            if (!string.IsNullOrEmpty(fileName))
            {
                fileNames.Add(fileName);
            }
        }

        return fileNames;
    }

    /// <summary>
    ///     Reads the content of an embedded resource as a string
    /// </summary>
    /// <param name="resourcePath">Resource path (e.g. "Assets/Templates/welcome.scriban")</param>
    /// <param name="assembly">The assembly to search in (if null, uses current assembly)</param>
    /// <returns>The content of the resource as a string</returns>
    public static byte[] GetEmbeddedResourceContent(string resourcePath, Assembly assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        // Normalize the path for embedded resource format
        var normalizedPath = resourcePath.Replace('/', '.').Replace('\\', '.');

        // Get the full resource name
        var assemblyName = assembly.GetName().Name;
        var fullResourceName = $"{assemblyName}.{normalizedPath}";

        // Check if the resource exists
        if (!assembly.GetManifestResourceNames().Contains(fullResourceName))
        {
            // Try to find a partial match
            var resourceNames = assembly.GetManifestResourceNames();
            var matchingResource = resourceNames.FirstOrDefault(n => n.EndsWith(normalizedPath));

            if (matchingResource != null)
            {
                fullResourceName = matchingResource;
            }
            else
            {
                throw new FileNotFoundException($"Embedded resource not found: {resourcePath}");
            }
        }

        // Read the resource content
        using var stream = assembly.GetManifestResourceStream(fullResourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"Unable to open resource: {fullResourceName}");
        }

        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    public static string ConvertResourceNameToPath(string resourceName, string baseNamespace)
    {
        if (!resourceName.StartsWith(baseNamespace + "."))
        {
            throw new ArgumentException("Resource name does not start with the given base namespace.");
        }

        var relativeName = resourceName[(baseNamespace.Length + 1)..];

        var lastDotIndex = relativeName.LastIndexOf('.');

        if (lastDotIndex == -1)
        {
            throw new ArgumentException("Resource name does not contain a valid extension.");
        }

        var pathPart = relativeName[..lastDotIndex].Replace('.', Path.DirectorySeparatorChar);
        var extension = relativeName[(lastDotIndex + 1)..];

        return $"{pathPart}.{extension}";
    }

    /// <summary>
    ///     Extracts the file name from an embedded resource path
    /// </summary>
    /// <param name="resourceName">Full resource name</param>
    /// <returns>File name without path</returns>
    public static string GetFileNameFromResourcePath(string resourceName)
    {
        // Use a regex to extract the file name
        var match = FileNameRegex().Match(resourceName);

        return
            match.Success ? match.Groups[1].Value : resourceName; // If it fails to find a pattern, return the original name
    }

    [GeneratedRegex(@"\.([^\.]+)$")]
    private static partial Regex FileNameRegex();
}
