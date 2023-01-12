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
        ) => Results.Ok( await mediator.Send(new GetWeatherForecast.Query()))
    )
    .WithTags("Account")
    .WithName("Sign In")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.UsePhaetonFramework();

app.Run();

public sealed record Foo() : IQuery<GetWeatherForecast.WeatherForecast>;