using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Caching.Factories;
using OoBDev.TestUtilities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Factories;

[TestClass]
public class CachedProxyStampedeTests
{
    public required TestContext TestContext { get; set; }

    public interface ISlowSource
    {
        int Load();
    }

    public sealed class SlowSource : ISlowSource
    {
        public int Calls;

        [IsCacheable("slow-key", "00:10:00")]
        public int Load()
        {
            Interlocked.Increment(ref Calls);
            Thread.Sleep(150);
            return 42;
        }
    }

    private sealed class MemoryManager : ICachingManager
    {
        private readonly ConcurrentDictionary<string, object> _store = new(StringComparer.Ordinal);

        public string BuildKey(MethodInfo method, params object?[]? args) => "slow-key";

        public Task FlushAsync(string key)
        {
            _store.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task StoreAsync(string key, object data, TimeSpan lifeTime)
        {
            _store[key] = data;
            return Task.CompletedTask;
        }

        public Task<object?> RetrieveAsync(string key, Type targetType) =>
            Task.FromResult(_store.TryGetValue(key, out var value) ? value : null);

        public Task<T?> RetrieveAsync<T>(string key) =>
            Task.FromResult(_store.TryGetValue(key, out var value) ? (T?)value : default);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Load_ConcurrentMisses_ComputeOnce()
    {
        // Stage
        var source = new SlowSource();
        var proxy = CachedProxy<ISlowSource, SlowSource>.Create(source, new MemoryManager(), TestContext.GetLogger<SlowSource>());

        // Test
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(proxy.Load)));

        // Assert
        Assert.IsTrue(results.All(r => r == 42));
        Assert.AreEqual(1, source.Calls, "Only the first miss may compute; the rest must read its result.");
    }
}
