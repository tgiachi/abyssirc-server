namespace AbyssIrc.Server.Routes;

public static class StatusRoute
{
    public static IEndpointRouteBuilder MapStatusRoute(this IEndpointRouteBuilder group)
    {
        var statusGroup = group.MapGroup("/status");

        statusGroup
            .MapGet("", () => Results.Ok("Ok"))
            .AllowAnonymous();

        return group;
    }
}
