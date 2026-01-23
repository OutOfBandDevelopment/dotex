using System;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Providers;

/// <summary>
/// A no-op caching provider for testing scenarios where caching is not needed.
/// </summary>
internal class NullCachingProvider : ICachingProvider
{
    public Task FlushAsync(string? key) => Task.CompletedTask;

    public Task<object?> RetreiveAsync(string? key, Type? targetType) => Task.FromResult<object?>(null);

    public Task StoreAsync(string? key, object? data, TimeSpan expiration) => Task.CompletedTask;
}
