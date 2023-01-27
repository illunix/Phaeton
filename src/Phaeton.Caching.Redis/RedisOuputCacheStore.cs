using Microsoft.AspNetCore.OutputCaching;
using StackExchange.Redis;

namespace Phaeton.Caching.Redis;

public sealed class RedisOuputCacheStore : IOutputCacheStore
{
    private readonly IDatabaseAsync _db;

    public RedisOuputCacheStore(IConnectionMultiplexer connMultiplexer)
        => _db = connMultiplexer.GetDatabase();

    public async ValueTask<byte[]?> GetAsync(
        string key,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(key);

        return await _db.StringGetAsync(key);
    }

    public async ValueTask SetAsync(
        string key,
        byte[] value,
        string[]? tags,
        TimeSpan validFor,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        foreach (var tag in tags ?? Array.Empty<string>())
        {
            await _db.SetAddAsync(
                tag,
                key
            );
        }

        await _db.StringSetAsync(
            key,
            value,
            validFor
        );
    }

    public async ValueTask EvictByTagAsync(
        string tag,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(tag);

        var cachedKeys = await _db.SetMembersAsync(tag);
        var keys = cachedKeys
            .Select(q => (RedisKey)q.ToString())
            .Concat(new[] { (RedisKey)tag })
            .ToArray();

        await _db.KeyDeleteAsync(keys);
    }
}