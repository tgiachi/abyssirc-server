using AbyssIrc.Core.Extensions;

namespace AbyssIrc.Server.Core.Directories;

public class DirectoriesConfig
{
    private readonly string[] _directories;

    public DirectoriesConfig(string rootDirectory, string[] directories)
    {
        _directories = directories;
        Root = rootDirectory;

        Init();
    }

    public string Root { get; }

    public string this[string directoryType] => GetPath(directoryType);


    public string this[Enum directoryType] => GetPath(directoryType.ToString());

    public string GetPath<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return GetPath(Enum.GetName(value));
    }

    public string GetPath(string directoryType)
    {
        var path = Path.Combine(Root, directoryType.ToSnakeCase());

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    private void Init()
    {
        if (!Directory.Exists(Root))
        {
            Directory.CreateDirectory(Root);
        }

        var directoryTypes = _directories.ToList();


        foreach (var path in directoryTypes.Select(GetPath)
                     .Where(path => !Directory.Exists(path)))
        {
            Directory.CreateDirectory(path);
        }
    }

    public override string ToString()
    {
        return Root;
    }
}
