using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Features.Account;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace InDuckTor.Vestnik.Api.Endpoints;

public static class WebSocketEndpoints
{
    public static void AddWebSocketsEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1/ws/vestnik")
            .WithTags("WebSockets")
            .WithOpenApi();

        groupBuilder.MapHub<AccountEventsHub>("account-events")
            .WithOpenApi()
            .RequireAuthorization();

        groupBuilder.MapGet("hueta", Hueta);

        groupBuilder.MapGet("some", Some)
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static void Hueta([FromServices] IExecutor<IHueta, Unit, Unit> hueta, CancellationToken ct)
    {
        hueta.Execute(new(), ct);
    }

    private static Ok<string> Some([FromQuery(Name = "access_token")] string accessToken)
    {
        return TypedResults.Ok("");
    }
}