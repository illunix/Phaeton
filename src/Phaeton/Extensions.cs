using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Phaeton.Abstractions;
using Phaeton.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Phaeton.Identity;

namespace Phaeton;

public static class Extensions
{
    public static T BindOptions<T>(
        this IConfiguration config,
        string sectionName
    ) where T : new()
        => config.GetSection(sectionName).BindOptions<T>();

    public static T BindOptions<T>(this IConfigurationSection section) where T : new()
    {
        var options = new T();
        section.Bind(options);
        return options;
    }

    public static IServiceCollection AddPhaeton(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("app");
        var options = section.BindOptions<AppOptions>();
        services.Configure<AppOptions>(section);

        return services
            .AddSingleton<IIdGen>(new IdentityGenerator(options.GeneratorId))
            .AddSingleton<IJsonSerializer, SystemTextJsonSerializer>()
            .Configure<JsonOptions>(jsonOptions =>
            {
                jsonOptions.SerializerOptions.PropertyNameCaseInsensitive = true;
                jsonOptions.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonOptions.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });
    }
}