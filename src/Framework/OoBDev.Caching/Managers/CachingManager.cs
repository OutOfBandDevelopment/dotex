using OoBDev.System.Utilities;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OoBDev.Caching.Managers;

/// <summary>
/// Manages cache operations including key generation, storage, retrieval, and flushing.
/// Delegates actual cache storage to a configured <see cref="ICachingProvider"/>.
/// </summary>
public class CachingManager : ICachingManager
{
    private readonly IStringFormatter _formatter;
    private readonly ISelectedService<ICachingProvider> _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingManager"/> class.
    /// </summary>
    /// <param name="formatter">The string formatter for building cache keys.</param>
    /// <param name="cache">The selected caching provider.</param>
    public CachingManager(
        IStringFormatter formatter,
        ISelectedService<ICachingProvider> cache
        )
    {
        _formatter = formatter;
        _cache = cache;
    }

    /// <inheritdoc/>
    public string BuildKey(MethodInfo method, params object?[]? args)
    {
        var isCachableAttribute = method.GetCustomAttribute<IsCacheableAttribute>();
        if (isCachableAttribute != null)
        {
            return _formatter.Format(isCachableAttribute.KeyFormatter, method, args) ??
                throw new NullReferenceException($"Unable to creating caching key");
        }

        var flushCacheAttribute = method.GetCustomAttribute<FlushCacheAttribute>();
        if (flushCacheAttribute != null)
        {
            if (!string.IsNullOrWhiteSpace(flushCacheAttribute.KeyFormatter))
                return _formatter.Format(flushCacheAttribute.KeyFormatter, method, args) ??
                    throw new NullReferenceException($"Unable to creating caching key");

            var targetMethod = flushCacheAttribute.MethodName == null ? null :
                flushCacheAttribute.TargetClass?.GetMethod(flushCacheAttribute.MethodName, [.. method.GetParameters().Select(p => p.ParameterType)]);
            if (targetMethod != null)
                return BuildKey(targetMethod, args);
        }

        throw new ApplicationException("Unable resolve Caching Key");
    }

    /// <inheritdoc/>
    public Task FlushAsync(string key) => _cache.Value?.FlushAsync(key) ?? Task.FromResult(0);

    /// <inheritdoc/>
    public async Task<T?> RetreiveAsync<T>(string key) =>
#pragma warning disable CS8603 // Possible null reference return.
        (T?)((await RetreiveAsync(key, typeof(T))) ?? default(T));
#pragma warning restore CS8603 // Possible null reference return.

    /// <inheritdoc/>
    public async Task<object?> RetreiveAsync(string key, Type targetType) =>
        _cache.Value switch
        {
            null => null,
            _ => await _cache.Value.RetreiveAsync(key, targetType)
        };

    /// <inheritdoc/>
    public Task StoreAsync(string key, object data, TimeSpan lifeTime) =>
        _cache.Value?.StoreAsync(key, data, lifeTime) ?? Task.FromResult(0);
}
