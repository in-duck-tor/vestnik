using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Vestnik.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace InDuckTor.Vestnik.Api.Configuration.Auth;

public static class SignalrConfiguration
{
    public static void ConfigureSignalR(
        this IServiceCollection serviceCollection,
        string configurationName,
        IConfigurationSection jwtConfig)
    {
        serviceCollection
            .AddAuthentication()
            .AddJwtBearer("Signalr", options =>
            {
                var jwtSettings = jwtConfig.Get<JwtSettings>() ??
                                  throw new ArgumentException("Невозможно извлечь настройки JWT из конфигурации",
                                      configurationName);

                options.TokenValidationParameters = TokenValidationFactory.CreateTokenValidationParameters(jwtSettings);
                options.Events = new()
                {
                    OnAuthenticationFailed = _ =>
                    {
                        Console.WriteLine("\nSignalr auth failed\n");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = _ =>
                    {
                        Console.WriteLine("\nSignalr auth validated\n");
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