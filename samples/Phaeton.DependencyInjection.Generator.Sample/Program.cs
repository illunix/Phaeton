using Phaeton.Abstractions;
using Phaeton.Framework;
using Phaeton.DependencyInjection;
using Phaeton.DependencyInjection.Generator.Sample.Abstractions.Services;

var builder = WebApplication
    .CreateBuilder(args)
    .AddPhaetonFramework();

builder.Services.RegisterServicesFromAssembly();

var app = builder.Build();

app.MapPost(
    "/api/foo",
    async (
        IFooService fooService
    ) => { return Results.Ok(fooService.Bar()); }
);

app.UsePhaetonFramework();

app.Run();