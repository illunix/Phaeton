using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Phaeton.Observability;

public static class Extensions
{
    private const string ConsoleOutputTemplate = "{Timestamp:HH:mm} [{Level:u3}] {Message}{NewLine}{Exception}";
    private const string AppSectionName = "app";
    private const string SerilogSectionName = "serilog";

    public static WebApplicationBuilder AddLogging(
        this WebApplicationBuilder builder,
        Action<LoggerConfiguration> configure = null
    )
    {
        builder.Host.AddLogging(configure);

        return builder;
    }

    private static IHostBuilder AddLogging(
        this IHostBuilder builder,
        Action<LoggerConfiguration> configure = null
    )
        => builder.UseSerilog((ctx, loggerConfiguration) =>
        {
            var appOptions = ctx.Configuration.BindOptions<AppOptions>(AppSectionName);
            var loggerOptions = ctx.Configuration.BindOptions<SerilogOptions>(SerilogSectionName);

            Configure(
                loggerOptions,
                appOptions,
                loggerConfiguration,
                ctx.HostingEnvironment.EnvironmentName
            );
            configure?.Invoke(loggerConfiguration);
        });

    private static void Configure(SerilogOptions serilogOptions, AppOptions appOptions,
        LoggerConfiguration loggerConfiguration, string environmentName)
    {
        var level = GetLogEventLevel(serilogOptions.Level);

        loggerConfiguration.Enrich.FromLogContext()
            .MinimumLevel.Is(level)
            .Enrich.WithProperty("Environment", environmentName)
            .Enrich.WithProperty("Application", appOptions.Name)
            .Enrich.WithProperty("Version", appOptions.Version);


        Configure(loggerConfiguration, serilogOptions);
    }

    private static void Configure(
        LoggerConfiguration loggerConfiguration,
        SerilogOptions options
    )
    {
        var consoleOptions = options.Console;
        var seqOptions = options.Seq;

        if (consoleOptions.Enabled)
            loggerConfiguration.WriteTo.Console(
                outputTemplate: ConsoleOutputTemplate,
                theme: AnsiConsoleTheme.Code
            );

        if (seqOptions.Enabled)
            loggerConfiguration.WriteTo.Seq(
                seqOptions.Url,
                apiKey: seqOptions.ApiKey
            );
    }

    private static LogEventLevel GetLogEventLevel(string level)
        => Enum.TryParse<LogEventLevel>(
            level,
            true,
            out var logLevel
        ) ? logLevel : LogEventLevel.Information;
}