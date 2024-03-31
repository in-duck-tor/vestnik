using System.Reflection;
using InDuckTor.Shared.Security.Http;
using InDuckTor.Shared.Security.Jwt;
using InDuckTor.Shared.Strategies;
using InDuckTor.Vestnik.Api.Configuration;
using InDuckTor.Vestnik.Api.Endpoints;
using InDuckTor.Vestnik.Api.Services;
using InDuckTor.Vestnik.Features.Account;
using InDuckTor.Vestnik.Infrastructure.Kafka;
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

services.AddVestnikKafka(configuration.GetSection("Kafka"));

services.AddHttpClients(configuration.GetSection("HttpClients"));

services.AddStrategiesFrom(Assembly.GetAssembly(typeof(AccountEventsHandler))!);

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

app.Run();