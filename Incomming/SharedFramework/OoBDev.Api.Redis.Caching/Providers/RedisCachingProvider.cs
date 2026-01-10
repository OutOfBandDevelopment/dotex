using OoBDev.Caching.Contracts;
using OoBDev.Toolkit.Common;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace OoBDev.Api.Redis.Caching.Providers
{

    public class RedisCachingProvider : ICachingProvider
    {
        private readonly Lazy<IConnectionMultiplexer> _redis;
        private readonly IObjectConverter _converter;
        private readonly IConnectionMultiplexerFactory _factory;

        public RedisCachingProvider(
            IObjectConverter converter,
            IConnectionMultiplexerFactory factory
            )
        {
            _converter = converter;
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
            var value = await _converter.ConvertAsync(result.ToString(), targetType).ConfigureAwait(false);

            return value;
        }

        public async Task StoreAsync(string key, object data, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key) || data == null) return;

            var db = _redis.Value.GetDatabase();
            var json = await _converter.ToJsonAsync(data).ConfigureAwait(false);
            if (json == null) return;

            var value = new RedisValue(json.ToString());

            var redisKey = new RedisKey(key);
            await db.StringSetAsync(redisKey, value, expiration).ConfigureAwait(false);
        }
    }
}
