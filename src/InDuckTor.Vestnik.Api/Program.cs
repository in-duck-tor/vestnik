using System.Reflection;
using InDuckTor.Shared.Security;
using InDuckTor.Shared.Security.Http;
using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Shared.Strategies;
using InDuckTor.Shared.Utils;
using InDuckTor.Vestnik.Api.Configuration;
using InDuckTor.Vestnik.Api.Endpoints;
using InDuckTor.Vestnik.Api.Services;
using InDuckTor.Vestnik.Features.Account;
using InDuckTor.Vestnik.Infrastructure.Database;
using InDuckTor.Vestnik.Infrastructure.Firebase;
using InDuckTor.Vestnik.Infrastructure.Kafka;
using InDuckTor.Vestnik.Infrastructure.Kafka.Consumers;
using Microsoft.AspNetCore.SignalR;
using AccountType = InDuckTor.Shared.Security.Context.AccountType;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddInDuckTorAuthentication(configuration.GetSection(nameof(JwtSettings)))
    .AddInDuckTorSecurity()
    // for debug purposes
    .AddAuthorization(options => { options.AddPolicy("SystemAccess", policyBuilder => { policyBuilder.RequireClaim(InDuckTorClaims.AccountType, AccountType.System.GetEnumMemberName()); }); });

services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true) // allow any origin
            .WithOrigins("https://localhost:63343") // Allow only this origin can also have multiple origins separated with comma
            .AllowCredentials();
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