using System.Security.Cryptography;
using AbyssIrc.Core.Extensions;

namespace AbyssIrc.Server.Core.Data.Configs.Sections;

public class JwtAuthConfig
{
    public string Issuer { get; set; } = "AbyssIrc";
    public string Audience { get; set; } = "AbyssIrc";
    public string Secret { get; set; } = RandomNumberGenerator.GetBytes(128).ToBase64();

    public int ExpirationInMinutes { get; set; } = 60 * 24 * 31; // 31 day
}
