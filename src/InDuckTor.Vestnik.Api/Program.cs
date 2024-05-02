using System.Reflection;
using InDuckTor.Shared.Security.Http;
using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Api.Configuration;
using InDuckTor.Vestnik.Api.Endpoints;
using InDuckTor.Vestnik.Api.Services;
using InDuckTor.Vestnik.Features.Account;
using InDuckTor.Vestnik.Infrastructure.Database;
using InDuckTor.Vestnik.Infrastructure.Firebase;
using InDuckTor.Vestnik.Infrastructure.Kafka;
using InDuckTor.Vestnik.Infrastructure.Kafka.Consumers;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services
    .AddInDuckTorAuthentication(configuration.GetSection(nameof(JwtSettings)))
    .AddInDuckTorSecurity();
services
    .AddSingleton<IUserIdProvider, InDuckTorUserIdProvider>()
    .AddSignalR();

services.AddDatabase();
services.AddVestnikKafka(configuration.GetSection("Kafka"));
services.AddHttpClients(configuration.GetSection("HttpClients"));
services.AddLazyCache();
services.AddFirebase(configuration.GetSection("Firebase"));

services.AddStrategiesFrom(
    Assembly.GetAssembly(typeof(AccountEventsHandler))!,
    Assembly.GetAssembly(typeof(AccountConsumer))!);

services.AddEndpointsApiExplorer();
services.AddVestnikSwaggerGen();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseStaticFiles();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseInDuckTorSecurity();

app.AddWebSocketsEndpoints();
app.AddClientAppRegistrationEndpoints();
app.AddToolsEndpoints();

app.Run();