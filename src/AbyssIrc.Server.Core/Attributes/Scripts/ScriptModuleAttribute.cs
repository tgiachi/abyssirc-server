namespace AbyssIrc.Server.Core.Attributes.Scripts;

[AttributeUsage(AttributeTargets.Class)]
public class ScriptModuleAttribute : Attribute
{
    public string Name { get; }

    public ScriptModuleAttribute(string name)
    {
        Name = name;
    }
}
