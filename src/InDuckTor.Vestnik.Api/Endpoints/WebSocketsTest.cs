using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace InDuckTor.Vestnik.Api.Endpoints;

[SignalRHub]
public class MyHub : Hub
{
    public async Task Send(long userId, string message)
    {
        await Clients.All.SendAsync("SomeMethod", userId, message);
    }
}

public static class TestEndpoints
{
    public static IEndpointRouteBuilder WebSocketsEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1/ws")
            .WithTags("WebSockets")
            .WithOpenApi();

        groupBuilder.MapHub<MyHub>("myHub");

        groupBuilder.MapPost("/send", SendMyHubMessage)
            .WithOpenApi();

        return groupBuilder;
    }

    private static async Task<IResult> SendMyHubMessage([FromServices] MyHub myHub)
    {
        await myHub.Send(1, "Ebaaaat'");
        return TypedResults.NoContent();
    }
}