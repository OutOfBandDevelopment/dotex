using OoBDev.Caching;
using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace OoBDev.Redis.Caching.Providers;


public class RedisCachingProvider : ICachingProvider
{
    private readonly Lazy<IConnectionMultiplexer> _redis;
    private readonly IObjectConverter _converter;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IConnectionMultiplexerFactory _factory;

    public RedisCachingProvider(
        IObjectConverter converter,
        IJsonSerializer jsonSerializer,
        IConnectionMultiplexerFactory factory
        )
    {
        _converter = converter;
        _jsonSerializer = jsonSerializer;
        _factory = factory;
        _redis = new Lazy<IConnectionMultiplexer>(() => _factory.Create());
    }

    public async Task FlushAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        var db = _redis.Value.GetDatabase();
        await db.KeyDeleteAsync(key).ConfigureAwait(false);
    }

    public async Task<object?> RetreiveAsync(string key, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var db = _redis.Value.GetDatabase();

        var result = await db.StringGetAsync(key).ConfigureAwait(false);

        if (!result.HasValue) return null;
        var value = _converter.Convert(result.ToString(), targetType);

        return value;
    }

    public async Task StoreAsync(string key, object data, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key) || data == null) return;

        var db = _redis.Value.GetDatabase();
        var json = _jsonSerializer.Serialize(data, data.GetType());
        if (json == null) return;

        var value = new RedisValue(json.ToString());

        var redisKey = new RedisKey(key);
        await db.StringSetAsync(redisKey, value, expiration).ConfigureAwait(false);
    }
}
