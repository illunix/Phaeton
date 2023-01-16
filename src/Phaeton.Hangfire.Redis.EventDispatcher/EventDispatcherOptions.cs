namespace Phaeton.Hangfire.Redis.EventDispatcher;

internal sealed class EventDispatcherOptions
{
    public bool InMemory { get; init; } = true;
    public RedisOptions? Redis { get; init; }

    public sealed class RedisOptions
    {
        public bool Enabled { get; init; }
        public string? ConnectionString { get; init; }
    }
}