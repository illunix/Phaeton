using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Phaeton.API.Exceptions;
using Phaeton.API.Swagger;
using Phaeton.Auth;
using Phaeton.Contexts;
using Phaeton.Dispatchers;
using Phaeton.Observability;
using Phaeton.gRPC;

namespace Phaeton.Framework;

public static class Extensions
{
    public static WebApplicationBuilder AddPhaetonFramework(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var appOptions = config
            .GetSection("app")
            .BindOptions<AppOptions>();
        var appInfo = new AppInfo(
            appOptions.Name,
            appOptions.Version
        );

        RenderLogo(appOptions);

        builder.AddLogging();

        builder.Services.AddSingleton(appInfo);

        builder.Services
            .AddErrorHandling()
            .AddHandlers(appOptions.Project)
            .AddDispatchers()
            .AddContexts()
            .AddPhaeton(config)
            .AddAuth(config)
            .AddSwaggerDocs(config)
            .AddgRPC(config);

        return builder;
    }

    public static WebApplication UsePhaetonFramework(this WebApplication app)
    {
        app
            .UseErrorHandling()
            .UseSwaggerDocs()
            .UseRouting();

        return app;
    }

    private static void RenderLogo(AppOptions app)
    {
        if (string.IsNullOrWhiteSpace(app.Name))
            return;

        Console.WriteLine($"{Figgle.FiggleFonts.Twisted.Render($"{app.Project}")}\t\t\t{app.Name} ({app.Version})\n\n");
    }
}