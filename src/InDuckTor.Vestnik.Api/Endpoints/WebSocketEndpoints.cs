using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;

namespace InDuckTor.Vestnik.Api.Endpoints;

[SignalRHub]
public class MyHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public async Task Send(long userId, string message)
    {
        await Clients.All.SendAsync("SomeMethod", userId, message);
    }
}

public static class WebSocketEndpoints
{
    public static IEndpointRouteBuilder AddWebSocketsEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1/ws")
            .WithTags("WebSockets")
            .WithOpenApi();

        groupBuilder.MapHub<MyHub>("my-hub")
            .WithOpenApi()
            .RequireAuthorization();

        return groupBuilder;
    }
}