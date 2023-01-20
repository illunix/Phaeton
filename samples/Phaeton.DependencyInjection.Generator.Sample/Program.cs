using Phaeton.Framework;
using Phaeton.DependencyInjection;
using SocialMediaTradex.Services.Users.Core.Abstractions.Services;

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

        return Results.Ok();
    }
);

app.UsePhaetonFramework();

app.Run();