using AbyssIrc.Server.Core.Data.Rest;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using Microsoft.AspNetCore.Mvc;

namespace AbyssIrc.Server.Routes;

public static class AuthRoutes
{
    public static RouteGroupBuilder MapAuthRoute(this RouteGroupBuilder group)
    {
        var authGroup = group.MapGroup("/auth").WithGroupName("Auth");


        authGroup.MapPost(
            "/login",
            async ([FromBody] LoginRequestData loginRequestData, IOperAuthService operAuthService) =>
            {
                var resultData = await operAuthService.AuthenticateOperAsync(
                    loginRequestData.Username,
                    loginRequestData.Password
                );

                if (resultData.IsSuccess)
                {
                    return Results.Ok(resultData);
                }

                return Results.Unauthorized();
            }
        );

        return group;
    }
}
