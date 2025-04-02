using AbyssIrc.Server.Core.Data.Configs.Sections.Oper;

namespace AbyssIrc.Server.Core.Data.Configs.Sections;

public class OperConfig
{
    public List<OperEntry> Users { get; set; } = new();
}
