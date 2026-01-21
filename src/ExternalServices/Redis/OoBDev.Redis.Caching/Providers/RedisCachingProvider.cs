using OoBDev.Caching;
using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace OoBDev.Redis.Caching.Providers;

/// <summary>
/// Redis-based caching provider implementation using StackExchange.Redis.
/// Provides distributed caching capabilities with support for expiration and serialization.
/// </summary>
public class RedisCachingProvider : ICachingProvider
{
    private readonly Lazy<IConnectionMultiplexer> _redis;
    private readonly IObjectConverter _converter;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IConnectionMultiplexerFactory _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCachingProvider"/> class.
    /// </summary>
    /// <param name="converter">The object converter for type conversions.</param>
    /// <param name="jsonSerializer">The JSON serializer for data serialization.</param>
    /// <param name="factory">The factory for creating Redis connections.</param>
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

    /// <summary>
    /// Removes a cached item from Redis by its key.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    public async Task FlushAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        var db = _redis.Value.GetDatabase();
        await db.KeyDeleteAsync(key).ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves a cached item from Redis and converts it to the specified type.
    /// </summary>
    /// <param name="key">The cache key to retrieve.</param>
    /// <param name="targetType">The target type to convert the cached value to.</param>
    /// <returns>The cached object converted to the target type, or null if not found.</returns>
    public async Task<object?> RetreiveAsync(string key, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        var db = _redis.Value.GetDatabase();

        var result = await db.StringGetAsync(key).ConfigureAwait(false);

        if (!result.HasValue) return null;
        var value = _converter.Convert(result.ToString(), targetType);

        return value;
    }

    /// <summary>
    /// Stores an object in Redis cache with the specified expiration time.
    /// </summary>
    /// <param name="key">The cache key to store the data under.</param>
    /// <param name="data">The data object to cache.</param>
    /// <param name="expiration">The time span after which the cached item expires.</param>
    /// <returns>A task representing the asynchronous store operation.</returns>
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
