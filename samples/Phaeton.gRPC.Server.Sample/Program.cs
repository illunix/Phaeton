using Phaeton.Framework;
using Phaeton.gRPC;

var builder = WebApplication
    .CreateBuilder(args)
    .AddPhaetonFramework();

builder.Services.AddgRPCClients();

var app = builder.Build();

app.UsePhaetonFramework();
app.MapgRPCServices();

app.Run();