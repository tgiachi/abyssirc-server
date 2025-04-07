namespace AbyssIrc.Server.Routes;

public static class StatusRoute
{
    public static RouteGroupBuilder MapStatusRoute(this RouteGroupBuilder group)
    {
        var statusGroup = group.MapGroup("/status");

        statusGroup.MapGet("", () => { return Results.Ok("Ok"); });

        return group;
    }
}
