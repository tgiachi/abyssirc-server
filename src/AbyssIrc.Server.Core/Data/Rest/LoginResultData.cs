namespace AbyssIrc.Server.Core.Data.Rest;

public record LoginResultData(string JwtToken, string RefreshToken, bool IsSuccess, DateTime ExpiresAt);
