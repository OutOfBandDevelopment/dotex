using OoBDev.Caching.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Caching.Providers
{
    public class MicrosoftMemoryCachingProvider : ICachingProvider, IDisposable
    {
        private readonly IMemoryCache _cache;

        public MicrosoftMemoryCachingProvider(
            IOptions<MemoryCacheOptions> optionsAccessor
            )
        {
            _cache = new MemoryCache(optionsAccessor);
        }

        public void Dispose() => _cache.Dispose();

        public Task FlushAsync(string key)
        {
            _cache.Remove(key);
            return Task.FromResult(0);
        }

        public Task<object?> RetreiveAsync(string key, Type targetType) =>
            Task.FromResult(
                _cache.TryGetValue(key, out var value) ?
                    value :
                    null
            );

        public Task StoreAsync(string key, object data, TimeSpan expiration)
        {
            _cache.Set(key, data, expiration);
            return Task.FromResult(0);
        }
    }
}
