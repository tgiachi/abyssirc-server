using AbyssIrc.Server.Core.Data.Rest;
using AbyssIrc.Server.Core.Interfaces.Services.Server;
using Microsoft.AspNetCore.Mvc;

namespace AbyssIrc.Server.Routes;

public static class AuthRoutes
{
    public static IEndpointRouteBuilder MapAuthRoute(this IEndpointRouteBuilder group)
    {
        var authGroup = group.MapGroup("/auth");

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
            )
            .WithName("Login")
            .WithTags("Auth")
            .Produces(200)
            .Produces(401)
            .AllowAnonymous();

        return authGroup;
    }
}
