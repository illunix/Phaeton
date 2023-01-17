using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Phaeton.Abstractions;
using Phaeton.Hangfire.Redis.EventDispatcher;

namespace Phaeton.Dispatchers;

public static class Extensions
{
    public static IServiceCollection AddRedisEventDispatcher(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        var section = config.GetSection("eventDispatcher");
        if (!section.Exists())
            return services;

        var options = section.BindOptions<EventDispatcherOptions>();
        if (
            !options.InMemory &&
            options.Redis is not null
        )
            return services;

        if (options.Redis is not null)
            if (string.IsNullOrEmpty(options.Redis.ConnectionString))
                throw new ArgumentException(nameof(options.Redis.ConnectionString));

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