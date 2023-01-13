using Phaeton.Framework;

var builder = WebApplication
    .CreateBuilder(args)
    .AddPhaetonFramework();

var app = builder.Build();

app.UsePhaetonFramework();

app.Run();