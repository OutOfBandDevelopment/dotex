using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OoBDev.Caching.Factories;

/// <summary>
/// Dynamic proxy that intercepts method calls and applies caching logic based on attributes.
/// Uses <see cref="DispatchProxy"/> to create proxy instances at runtime.
/// </summary>
/// <typeparam name="TInterface">The interface type being proxied.</typeparam>
/// <typeparam name="TImplemention">The implementation type being decorated with caching.</typeparam>
#pragma warning disable CA1000 // the typed Create mirrors DispatchProxy.Create
public partial class CachedProxy<TInterface, TImplemention> : DispatchProxy
        where TImplemention : class, TInterface
{
    private TImplemention? _decorated;
    private ICachingManager? _cachingManager;
    private ILogger<TImplemention>? _logger;

    /// <summary>
    /// Intercepts method invocations and applies caching logic based on <see cref="IsCacheableAttribute"/> and <see cref="FlushCacheAttribute"/> attributes.
    /// </summary>
    /// <param name="targetMethod">The method being invoked.</param>
    /// <param name="args">The arguments passed to the method.</param>
    /// <returns>The result of the method invocation, potentially retrieved from cache or stored to cache.</returns>
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (_decorated == null || targetMethod == null) return null;

        var method = _decorated.GetType().GetMethod(targetMethod.Name, [.. targetMethod.GetParameters().Select(p => p.ParameterType)]);
        if (_cachingManager != null && method != null)
        {
            try
            {
                var isCachableAttribute = method.GetCustomAttribute<IsCacheableAttribute>();
                if (isCachableAttribute != null)
                {
                    var cachingKey = _cachingManager.BuildKey(method, args);
                    if (!string.IsNullOrWhiteSpace(cachingKey))
                    {
                        // do cacheable stuff

                        //TODO: add exception for void and non generic tasks

                        var targetReturnType = method.ReturnType;
                        var isTask = false;
                        if (targetReturnType.IsGenericType && targetReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            //checking to see if Task<>
                            targetReturnType = targetReturnType.GetGenericArguments().First();
                            isTask = true;
                        }
                        else if (targetReturnType == typeof(Task))
                        {
                            throw new NotSupportedException($"Caching of Task not supported");
                        }
                        else if (targetReturnType == typeof(void))
                        {
                            throw new NotSupportedException($"Caching of Void not supported");
                        }

                        var cachedResult = _cachingManager.RetrieveAsync(cachingKey, targetReturnType).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (cachedResult != null)
                        {
                            if (_logger != null) LogRetrieved(_logger, targetMethod.Name, cachingKey);
                            return isTask ? ResultAwaiter.Wrap(cachedResult) : cachedResult;
                        }

                        // look up original
                        var original = method.Invoke(_decorated, args);
                        if (original == null) return null; // no result to try to cache

                        if (isTask)
                        {
                            original = ResultAwaiter.Unwrap(targetReturnType, original);
                        }

                        // store original
                        _cachingManager.StoreAsync(cachingKey, original, isCachableAttribute.LifeTime).ConfigureAwait(false).GetAwaiter().GetResult();
                        if (_logger != null) LogStored(_logger, targetMethod.Name, cachingKey);

                        return isTask ? ResultAwaiter.Wrap(original) : original;
                    }
                }

                var flushCacheAttribute = method.GetCustomAttribute<FlushCacheAttribute>();
                if (flushCacheAttribute != null)
                {
                    var cachingKey = _cachingManager.BuildKey(method, args);
                    if (_logger != null) LogFlushed(_logger, targetMethod.Name, cachingKey);
                    // do flushable stuff
                    _cachingManager.FlushAsync(cachingKey).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                {
                    LogFailure(_logger, ex.Message);
                    LogFailureDetails(_logger, ex);
                }
                //Note: we are eating this exception on purpose.  Is caching fails we still want the process to continue

#if DEBUG
                throw; //Note: this is so it still blows up in local testing
#endif
            }
        }

        // do original stuff
        return method?.Invoke(_decorated, args);
    }

    /// <summary>
    /// Creates a new cacheable proxy instance that wraps the specified implementation.
    /// </summary>
    /// <param name="decorated">The implementation instance to wrap.</param>
    /// <param name="cachingManager">The caching manager for cache operations.</param>
    /// <param name="logger">The logger for recording cache operations.</param>
    /// <returns>A proxy instance that intercepts method calls and applies caching logic.</returns>
    public static TInterface Create(TImplemention decorated, ICachingManager cachingManager, ILogger<TImplemention> logger)
    {
        object? proxy = Create<TInterface, CachedProxy<TInterface, TImplemention>>();
        var unwrapped = (CachedProxy<TInterface, TImplemention>?)proxy;
        if (proxy != null && unwrapped != null)
        {
            unwrapped._decorated = decorated;
            unwrapped._cachingManager = cachingManager;
            unwrapped._logger = logger;
            return (TInterface)proxy;
        }
#pragma warning disable CS8603 // Possible null reference return.
        return default;
#pragma warning restore CS8603 // Possible null reference return.
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Retrieved from Cache::{Method}::{Key}")]
    private static partial void LogRetrieved(ILogger logger, string method, string? key);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Stored into Cache::{Method}::{Key}")]
    private static partial void LogStored(ILogger logger, string method, string? key);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Flush from Cache::{Method}::{Key}")]
    private static partial void LogFlushed(ILogger logger, string method, string? key);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "{Message}")]
    private static partial void LogFailure(ILogger logger, string message);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Caching operation failed")]
    private static partial void LogFailureDetails(ILogger logger, Exception exception);
}
