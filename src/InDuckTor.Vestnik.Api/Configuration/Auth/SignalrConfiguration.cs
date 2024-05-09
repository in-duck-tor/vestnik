using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Vestnik.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace InDuckTor.Vestnik.Api.Configuration.Auth;

public static class SignalrConfiguration
{
    public static void ConfigureSignalR(
        this IServiceCollection serviceCollection,
        JwtSettings jwtSettings)
    {
        serviceCollection
            .AddAuthentication()
            .AddJwtBearer("Signalr", options =>
            {
                options.TokenValidationParameters = TokenValidationFactory.CreateTokenValidationParameters(jwtSettings);
                options.Events = new()
                {
                    OnAuthenticationFailed = context =>
                    {
                        var request = context.HttpContext.Request;
                        Console.WriteLine($"Signalr auth failed for path {request.Path + request.QueryString}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var request = context.HttpContext.Request;
                        Console.WriteLine($"Signalr auth succeed for path {request.Path + request.QueryString}");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/v1/ws/vestnik"))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        serviceCollection
            .AddSingleton<IUserIdProvider, InDuckTorUserIdProvider>()
            .AddSignalR();
    }
}