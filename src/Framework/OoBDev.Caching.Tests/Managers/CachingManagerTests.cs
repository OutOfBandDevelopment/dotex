using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Caching.Managers;
using OoBDev.System.Utilities;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Managers;

[TestClass]
public class CachingManagerTests
{
    public required TestContext TestContext { get; set; }

    private MockRepository mockRepository;

    private Mock<IStringFormatter> mockStringFormatter;
    private Mock<ISelectedService<ICachingProvider>> mockCache;
    private Mock<ICachingProvider> mockCachingProvider;

    [TestInitialize]
    public void TestInitialize()
    {
        this.mockRepository = new MockRepository(MockBehavior.Strict);

        this.mockStringFormatter = this.mockRepository.Create<IStringFormatter>();
        this.mockCache = this.mockRepository.Create<ISelectedService<ICachingProvider>>();
        this.mockCachingProvider = this.mockRepository.Create<ICachingProvider>();
    }

    private CachingManager CreateManager() => new(this.mockStringFormatter.Object, this.mockCache.Object);

    public abstract class TestObject
    {
        public const string FlushKey = "Flush Key";
        public const string IsCacheableKey = "IsCacheable Key";

        public abstract int? Bypass();
        [FlushCache(FlushKey)]
        public abstract int? Flush();
        [FlushCache(typeof(TestObject), nameof(IsCacheable))]
        public abstract int? FlushDirected();
        [IsCacheable(IsCacheableKey, "00:11:22")]
        public abstract int? IsCacheable();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void BuildKeyTest_IsCacheable()
    {
        // Stage
        var method = typeof(TestObject).GetMethod(nameof(TestObject.IsCacheable));
        var args = Array.Empty<object>();
        var expected = "test results";

        // Mock
        mockStringFormatter.Setup(s => s.Format(TestObject.IsCacheableKey, method, args)).Returns(expected);

        // Test
        var manager = this.CreateManager();
        var result = manager.BuildKey(method, args);

        // Assert
        Assert.AreEqual(expected, result);

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void BuildKeyTest_Flush()
    {
        // Stage
        var method = typeof(TestObject).GetMethod(nameof(TestObject.Flush));
        var args = Array.Empty<object>();
        var expected = "test results";

        // Mock
        mockStringFormatter.Setup(s => s.Format(TestObject.FlushKey, method, args)).Returns(expected);

        // Test
        var manager = this.CreateManager();
        var result = manager.BuildKey(method, args);
        Assert.AreEqual(expected, result);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void BuildKeyTest_FlushDirected()
    {
        // Stage
        var method = typeof(TestObject).GetMethod(nameof(TestObject.FlushDirected));
        var args = Array.Empty<object>();
        var expected = "test results";
        var directedMethod = typeof(TestObject).GetMethod(nameof(TestObject.IsCacheable));

        // Mock
        //  mockStringFormatter.Setup(s => s.Format(TestObject.FlushKey, method, args)).Returns(expected);
        mockStringFormatter.Setup(s => s.Format(TestObject.IsCacheableKey, directedMethod, args)).Returns(expected);

        // Test
        var manager = this.CreateManager();
        var result = manager.BuildKey(method, args);
        Assert.AreEqual(expected, result);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void BuildKeyTest_Bypass()
    {
        // Stage
        var method = typeof(TestObject).GetMethod(nameof(TestObject.Bypass));
        var args = Array.Empty<object>();

        // Mock

        // Test
        var manager = this.CreateManager();
        Assert.Throws<ApplicationException>(() => manager.BuildKey(method, args));

        // Verify
        this.mockRepository.VerifyAll();
    }

    //public Task FlushAsync(string key) => _cache.FlushAsync(key);
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task FlushAsyncTest()
    {
        // Stage
        string key = "test key";

        // Mock
        mockCachingProvider.Setup(s => s.FlushAsync(key)).Returns(Task.FromResult(0));
        mockCache.Setup(s => s.Value).Returns(mockCachingProvider.Object);


        // Test
        var manager = this.CreateManager();
        await manager.FlushAsync(key);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RetreiveAsyncTest()
    {
        // Stage
        string key = "test key";
        object data = new { };
        Type targetType = data.GetType();

        // Mock
        mockCachingProvider.Setup(s => s.RetreiveAsync(key, targetType)).ReturnsAsync(data);
        mockCache.Setup(s => s.Value).Returns(mockCachingProvider.Object);

        // Test
        var manager = this.CreateManager();
        var result = await manager.RetreiveAsync(key, targetType);

        // Assert
        Assert.AreEqual(data, result);

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsyncTest()
    {
        // Stage
        string key = "test key";
        object data = new { };
        TimeSpan lifeTime = new TimeSpan(12, 34, 45);

        // Mock
        mockCachingProvider.Setup(s => s.StoreAsync(key, data, lifeTime)).Returns(Task.FromResult(0));
        mockCache.Setup(s => s.Value).Returns(mockCachingProvider.Object);

        // Test
        var manager = this.CreateManager();
        await manager.StoreAsync(key, data, lifeTime);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }
}
