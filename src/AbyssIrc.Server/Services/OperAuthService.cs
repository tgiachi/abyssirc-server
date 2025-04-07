using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AbyssIrc.Core.Extensions;
using AbyssIrc.Core.Utils;
using AbyssIrc.Server.Core.Data.Configs;
using AbyssIrc.Server.Core.Data.Rest;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using Microsoft.IdentityModel.Tokens;

namespace AbyssIrc.Server.Services;

public class OperAuthService : IOperAuthService
{
    private readonly ILogger _logger;
    private readonly AbyssIrcConfig _abyssIrcConfig;


    private Dictionary<string, string> _userRefreshTokens = new();


    public OperAuthService(ILogger<OperAuthService> logger, AbyssIrcConfig abyssIrcConfig)
    {
        _logger = logger;
        _abyssIrcConfig = abyssIrcConfig;
    }

    public async Task<bool> AuthenticateAsync(string userName, string password)
    {
        var userExists = _abyssIrcConfig.Opers.Users.FirstOrDefault(s => s.Username == userName);

        if (userExists == null)
        {
            _logger.LogWarning("Oper user {UserName} not found", userName);
            return false;
        }

        var cleanedPassword = userExists.Password.Replace("hash:", "");
        var passwordHash = cleanedPassword.Split(":")[0];
        var salt = cleanedPassword.Split(":")[1];

        return HashUtils.VerifyPassword(password, passwordHash, salt);
    }

    public async Task<LoginResultData> AuthenticateOperAsync(string userName, string password)
    {
        var isAuth = await AuthenticateAsync(userName, password);

        if (!isAuth)
        {
            return new LoginResultData(null, null, false, DateTime.Now);
        }

        var jwtToken = GenerateJwtToken(userName);
        var refreshToken = HashUtils.GenerateRandomRefreshToken(64);
        var expiresAt = DateTime.UtcNow.AddMinutes(_abyssIrcConfig.WebServer.JwtAuthConfig.ExpirationInMinutes);


        _userRefreshTokens[userName] = refreshToken;

        _logger.LogInformation("User {UserName} authenticated successfully", userName);

        return new LoginResultData(jwtToken, refreshToken, true, expiresAt);
    }

    private string GenerateJwtToken(string username)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, username.GetMd5Checksum())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_abyssIrcConfig.WebServer.JwtAuthConfig.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _abyssIrcConfig.WebServer.JwtAuthConfig.Issuer,
            audience: _abyssIrcConfig.WebServer.JwtAuthConfig.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_abyssIrcConfig.WebServer.JwtAuthConfig.ExpirationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
