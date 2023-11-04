using GameServer.Host.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGameServer(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapWebSocketConnections();

await app.RunAsync();

