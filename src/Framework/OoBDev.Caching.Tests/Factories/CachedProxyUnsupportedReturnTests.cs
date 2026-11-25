using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Caching.Factories;
using OoBDev.TestUtilities;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Factories;

[TestClass]
public class CachedProxyUnsupportedReturnTests
{
    public required TestContext TestContext { get; set; }

    public interface IWorker
    {
        void Run();
        Task RunAsync();
        Task<int> CountAsync();
    }

    public class Worker : IWorker
    {
        public int Calls { get; private set; }

        [IsCacheable("run", "00:01:00")]
        public void Run() => Calls++;

        [IsCacheable("run-async", "00:01:00")]
        public Task RunAsync()
        {
            Calls++;
            return Task.CompletedTask;
        }

        [IsCacheable("count", "00:01:00")]
        public Task<int> CountAsync() => Task.FromResult(++Calls);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Invoke_CacheableVoidMethod_CallsThroughAndNeverTouchesTheCache()
    {
        TestContext.WriteLine("Stage: a strict caching manager with no setups, so any cache call fails the test");
        var manager = new Mock<ICachingManager>(MockBehavior.Strict);
        var worker = new Worker();
        var proxy = CachedProxy<IWorker, Worker>.Create(worker, manager.Object, TestContext.GetLogger<Worker>());

        TestContext.WriteLine("Test: call the cacheable void method twice");
        proxy.Run();
        proxy.Run();

        TestContext.WriteLine($"Assert: the real method ran {worker.Calls} times");
        Assert.AreEqual(2, worker.Calls);
        manager.VerifyNoOtherCalls();
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Invoke_CacheableNonGenericTask_CallsThroughAndNeverTouchesTheCache()
    {
        TestContext.WriteLine("Stage: a strict caching manager with no setups");
        var manager = new Mock<ICachingManager>(MockBehavior.Strict);
        var worker = new Worker();
        var proxy = CachedProxy<IWorker, Worker>.Create(worker, manager.Object, TestContext.GetLogger<Worker>());

        TestContext.WriteLine("Test: await the cacheable Task method twice");
        await proxy.RunAsync().ConfigureAwait(false);
        await proxy.RunAsync().ConfigureAwait(false);

        TestContext.WriteLine($"Assert: the real method ran {worker.Calls} times");
        Assert.AreEqual(2, worker.Calls);
        manager.VerifyNoOtherCalls();
    }
}
