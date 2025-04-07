namespace AbyssIrc.Server.Core.Data.Configs.Sections.Oper;

public class OperEntry
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Host { get; set; } = "*";

    public bool CanUseWebServer { get; set; } = false;

    public override string ToString()
    {
        return $"{Username} {Password} {Host}";
    }
}
