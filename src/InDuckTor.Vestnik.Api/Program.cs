using InDuckTor.Shared.Kafka;
using InDuckTor.Vestnik.Api.Configuration;
using InDuckTor.Vestnik.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddSignalR();

services.AddEndpointsApiExplorer();
services.AddVestnikSwaggerGen();

services.AddInDuckTorKafka();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.WebSocketsEndpoints();

app.Run();