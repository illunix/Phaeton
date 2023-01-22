using Phaeton.Framework;
using Phaeton.DependencyInjection;

var builder = WebApplication
    .CreateBuilder(args)
    .AddPhaetonFramework();

builder.Services.RegisterServicesFromAssembly();

var app = builder.Build();

app.MapPost(
    "/api/foo",
    async (
        IFooService fooService
    ) => 
    {
        fooService.Bar();

        return Results.Ok();
    }
);

app.UsePhaetonFramework();

app.Run();