using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Phaeton.DAL.Elasticsearch;

public static class Extensions
{
    public static IServiceCollection AddElasticsearch(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("elasticsearch");
        if (!section.Exists())
            return services;

        var options = section.BindOptions<ElasticsearchOptions>();

        services.Configure<ElasticsearchOptions>(section);

        switch (options.Endpoint)
        {
            case null:
                throw new ArgumentNullException(nameof(options.Endpoint));
            case "":
                throw new ArgumentException(nameof(options.Endpoint));
        }

        var settings = new ConnectionSettings(new Uri(options.Endpoint))
            .DefaultIndex("ad-offers");

        var client = new ElasticClient(settings);
        client.Indices.UpdateSettings(
            options.Index,
            q => q.IndexSettings(q => q.Setting(
                UpdatableIndexSettings.MaxResultWindow,
                5000000
            )
        ));

        services.AddSingleton<IElasticClient>(client);

        return services;
    }
}