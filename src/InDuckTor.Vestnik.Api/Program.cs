var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddSignalR();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.UseWebSockets();

app.Run();