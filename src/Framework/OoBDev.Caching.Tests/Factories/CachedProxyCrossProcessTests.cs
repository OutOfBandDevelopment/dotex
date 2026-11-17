using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Caching.Factories;
using OoBDev.TestUtilities;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Factories;

[TestClass]
public class CachedProxyCrossProcessTests
{
    public required TestContext TestContext { get; set; }

    public interface IColdSource
    {
        int Load();
    }

    public sealed class ColdSource : IColdSource
    {
        public int Calls;

        [IsCacheable("cold-key", "00:10:00")]
        public int Load()
        {
            Interlocked.Increment(ref Calls);
            return 7;
        }
    }

    private sealed class LockingManager(int deniedAttempts) : ICachingManager
    {
        private readonly ConcurrentDictionary<string, object> store = new(StringComparer.Ordinal);
        private int attempts;

        public int Released;

        public int Attempts => attempts;

        public string BuildKey(MethodInfo method, params object?[]? args) => "cold-key";

        public Task FlushAsync(string key) => Task.CompletedTask;

        public Task StoreAsync(string key, object data, TimeSpan lifeTime)
        {
            store[key] = data;
            return Task.CompletedTask;
        }

        public Task<object?> RetrieveAsync(string key, Type targetType) =>
            Task.FromResult(store.TryGetValue(key, out var value) ? value : null);

        public Task<T?> RetrieveAsync<T>(string key) =>
            Task.FromResult(store.TryGetValue(key, out var value) ? (T?)value : default);

        public Task<IAsyncDisposable?> TryLockAsync(string key, TimeSpan lifeTime) =>
            Task.FromResult<IAsyncDisposable?>(Interlocked.Increment(ref attempts) <= deniedAttempts ? null : new Held(this));

        public void PublishFromOtherProcess(object value) => store["cold-key"] = value;

        private sealed class Held(LockingManager owner) : IAsyncDisposable
        {
            public ValueTask DisposeAsync()
            {
                Interlocked.Increment(ref owner.Released);
                return ValueTask.CompletedTask;
            }
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Load_LockGranted_ComputesStoresAndReleases()
    {
        // Stage
        var manager = new LockingManager(deniedAttempts: 0);
        var source = new ColdSource();
        var proxy = CachedProxy<IColdSource, ColdSource>.Create(source, manager, TestContext.GetLogger<ColdSource>());

        // Test
        var result = proxy.Load();

        // Assert
        Assert.AreEqual(7, result);
        Assert.AreEqual(1, source.Calls);
        Assert.AreEqual(1, manager.Released);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Load_LockHeldElsewhere_WaitsAndReadsTheOtherProcessResult()
    {
        // Stage
        var manager = new LockingManager(deniedAttempts: int.MaxValue);
        var source = new ColdSource();
        var proxy = CachedProxy<IColdSource, ColdSource>.Create(source, manager, TestContext.GetLogger<ColdSource>());
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            manager.PublishFromOtherProcess(99);
        });

        // Test
        var result = proxy.Load();

        // Assert
        Assert.AreEqual(99, result);
        Assert.AreEqual(0, source.Calls, "This process must not compute while another holds the lock.");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Load_LockHolderNeverPublishes_ComputesAfterTheLockLifetime()
    {
        // Stage
        var manager = new LockingManager(deniedAttempts: int.MaxValue);
        var source = new ColdSource();
        var proxy = CachedProxy<IColdSource, ColdSource>.Create(source, manager, TestContext.GetLogger<ColdSource>(), TimeSpan.FromMilliseconds(300));

        // Test
        var result = proxy.Load();

        // Assert
        Assert.AreEqual(7, result);
        Assert.AreEqual(1, source.Calls);
        Assert.IsTrue(manager.Attempts > 1);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task NoLock_DisposeAsync_Completes()
    {
        await NoLock.Instance.DisposeAsync();
        Assert.IsNotNull(NoLock.Instance);
    }
}
