using InDuckTor.Vestnik.Features.Account;

namespace InDuckTor.Vestnik.Api.Endpoints;

public static class WebSocketEndpoints
{
    public static void AddWebSocketsEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1/ws/vestnik")
            .WithTags("WebSockets")
            .WithOpenApi();

        groupBuilder.MapHub<AccountEventsHub>("account-events")
            .WithOpenApi();
        // .RequireAuthorization();
    }
}