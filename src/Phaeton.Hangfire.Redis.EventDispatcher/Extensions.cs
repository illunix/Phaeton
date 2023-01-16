using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Phaeton.Abstractions;
using Phaeton.Hangfire.Redis.EventDispatcher;

namespace Phaeton.Dispatchers;

public static class Extensions
{
    public static IServiceCollection AddHangfireEventDispatcher(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("eventDispatcher");
        if (!section.Exists())
            return services;

        var options = section.BindOptions<EventDispatcherOptions>();
        if (!options.InMemory)
            return services;

        if (options.Redis is null)
            throw new ArgumentException("Redis section is null, use in memory event dispatcher or create redis section.");

        services.Configure<EventDispatcherOptions>(section);

        services.AddHangfire(q =>
        {
            q.UseRedisStorage(options.Redis.ConnectionString);
            q.UseSerializer();
        });

        services.AddHangfireServer();

        return services;
    }

    public static void Publish(
        this IMediator mediator, 
        string jobName, 
        IEvent @event
    )
        => new BackgroundJobClient().Enqueue<MediatorHangfireBridge>(bridge => bridge.Publish(
            jobName,
            @event
        ));

    public static void Publish(
        this IMediator mediator, 
        IEvent @event
    )
        => new BackgroundJobClient().Enqueue<MediatorHangfireBridge>(bridge => bridge.Publish(@event));

    private static void UseSerializer(this IGlobalConfiguration config)
        => config.UseSerializerSettings(new() { TypeNameHandling = TypeNameHandling.All });
}