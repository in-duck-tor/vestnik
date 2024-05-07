using System.Reflection;
using InDuckTor.Shared.Security;
using InDuckTor.Shared.Security.Http;
using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Shared.Strategies;
using InDuckTor.Shared.Utils;
using InDuckTor.Vestnik.Api.Configuration;
using InDuckTor.Vestnik.Api.Configuration.Auth;
using InDuckTor.Vestnik.Api.Endpoints;
using InDuckTor.Vestnik.Api.Services;
using InDuckTor.Vestnik.Features.Account;
using InDuckTor.Vestnik.Infrastructure.Database;
using InDuckTor.Vestnik.Infrastructure.Firebase;
using InDuckTor.Vestnik.Infrastructure.Kafka;
using InDuckTor.Vestnik.Infrastructure.Kafka.Consumers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using AccountType = InDuckTor.Shared.Security.Context.AccountType;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

var jwtConfig = configuration.GetSection(nameof(JwtSettings));
services
    .AddAuthentication()
    .AddJwtBearer("Signalr", options =>
    {
        var jwtSettings = jwtConfig.Get<JwtSettings>() ??
                          throw new ArgumentException("Невозможно извлечь настройки JWT из конфигурации",
                              nameof(configuration));

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

services
    .AddInDuckTorAuthentication(jwtConfig)
    .AddInDuckTorSecurity()
    .AddAuthorizationBuilder()
    // for debug purposes
    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("Bearer", "Signalr")
        .Build())
    .AddPolicy("SystemAccess",
        policyBuilder =>
        {
            policyBuilder.RequireClaim(InDuckTorClaims.AccountType, AccountType.System.GetEnumMemberName());
        });

services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowAnyMethod().AllowAnyHeader()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();

        // .WithOrigins("https://localhost:63343", "http://localhost:63343");
    });
});

services
    .AddSingleton<IUserIdProvider, InDuckTorUserIdProvider>()
    .AddSignalR();

services.AddDatabase();
services.AddVestnikKafka(configuration.GetSection("Kafka"));
services.AddHttpClients(configuration.GetSection("HttpClients"));
services.AddLazyCache();
services.AddFirebase(configuration.GetSection("Firebase"));

services.AddStrategiesFrom(
        Assembly.GetAssembly(typeof(AccountCreatedEventHandler))!,
        Assembly.GetAssembly(typeof(IHueta))!,
        Assembly.GetAssembly(typeof(AccountConsumer))!)
    .AddVestnikServices();

services.ConfigureJsonConverters();
services.AddEndpointsApiExplorer();
services.AddVestnikSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseStaticFiles();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.UseInDuckTorSecurity();

app.AddWebSocketsEndpoints();
app.AddClientAppRegistrationEndpoints();
app.AddToolsEndpoints();

app.Run();