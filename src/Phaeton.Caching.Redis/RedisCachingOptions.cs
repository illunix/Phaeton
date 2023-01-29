namespace Phaeton.Caching.Redis;

public sealed class RedisCachingOptions
{
    public bool Enabled { get; init; }
    public string? Endpoint { get; init; }
}