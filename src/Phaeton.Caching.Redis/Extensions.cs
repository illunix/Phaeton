using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Phaeton.Caching.Redis;

public static class Extensions
{
    public static IServiceCollection AddRedisOutputCache(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("redisCache");
        var options = section.BindOptions<RedisCachingOptions>();
        services.Configure<RedisCachingOptions>(section);

        if (!options.Enabled)
            return services;

        if (string.IsNullOrEmpty(options.Endpoint))
            ArgumentException.ThrowIfNullOrEmpty(options.Endpoint);

        services
            .AddSingleton<IConnectionMultiplexer>(q => ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                EndPoints = { options.Endpoint! },
                AbortOnConnectFail = false
            }))
            .AddOutputCache()
            .RemoveAll<IOutputCacheStore>()
            .AddSingleton<IOutputCacheStore, RedisOuputCacheStore>();

        return services;
    }
}
