namespace AbyssIrc.Core.Data.Configs.Sections;

public class AdminConfig
{
    public string ServerPassword { get; set; }
    public string AdminInfo1 { get; set; } = "AbyssIrc";
    public string AdminEmail { get; set; } = "admin@abyssirc.com";

    /// <summary>
    /// The name of the IRC network
    /// </summary>
    public string NetworkName { get; set; } = "AbyssIRC";
}
