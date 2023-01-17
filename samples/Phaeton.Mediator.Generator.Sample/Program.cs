using Phaeton.Abstractions;
using Phaeton.Framework;
using Phaeton.Sample.API.Features;

var builder = WebApplication
    .CreateBuilder(args)
    .AddPhaetonFramework();

var app = builder.Build();

app.MapPost(
        "/api/weather-forecast",
        async (
            IMediator mediator
        ) => Results.Ok()
    )
    .WithTags("Account")
    .WithName("Sign In")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.MapPost(
    "/api/sign-up",
async (
        SignUp.Command req,
        IMediator mediator
    ) =>
{
    await mediator.Send(req);

    return Results.NoContent();
}
)
    .WithTags("Account")
    .WithName("Sign up")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.UsePhaetonFramework();

app.Run();