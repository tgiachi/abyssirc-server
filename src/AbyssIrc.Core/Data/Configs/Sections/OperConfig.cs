using AbyssIrc.Core.Data.Configs.Sections.Oper;

namespace AbyssIrc.Core.Data.Configs.Sections;

public class OperConfig
{
    public List<OperEntry> Users { get; set; } = new();
}
