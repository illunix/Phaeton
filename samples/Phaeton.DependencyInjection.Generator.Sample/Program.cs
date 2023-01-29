using Phaeton.Framework;
using Phaeton.DependencyInjection;

var builder = WebApplication
    .CreateBuilder(args)
    .AddPhaetonFramework();

builder.Services.RegisterServicesFromAssembly();

var app = builder.Build();

app.UsePhaetonFramework();

app.Run();