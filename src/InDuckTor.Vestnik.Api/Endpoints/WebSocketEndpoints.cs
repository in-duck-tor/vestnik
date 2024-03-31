using InDuckTor.Vestnik.Features;
using InDuckTor.Vestnik.Features.Account;

namespace InDuckTor.Vestnik.Api.Endpoints;

public static class WebSocketEndpoints
{
    public static IEndpointRouteBuilder AddWebSocketsEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1/ws/vestnik")
            .WithTags("WebSockets")
            .WithOpenApi();

        groupBuilder.MapHub<AccountEventsHub>("account-events")
            .WithOpenApi()
            .RequireAuthorization();

        return groupBuilder;
    }
}