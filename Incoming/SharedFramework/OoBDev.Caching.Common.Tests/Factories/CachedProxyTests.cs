using OoBDev.TestUtilities;
using OoBDev.Caching.Common.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.Caching.Contracts;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;
using OoBDev.TestUtilities.Logging;

namespace OoBDev.Caching.Common.Tests.Factories
{
    [TestClass]
    public class CachedProxyTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;
        private Mock<ICachingManager> mockCachingManager;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            mockCachingManager = mockRepository.Create<ICachingManager>();
        }

        public interface ITestObject
        {
            int? IsCacheable();
            int? Bypass();
            int? Flush();
        }
        public abstract class TestObject : ITestObject
        {
            public abstract int? Bypass();
            [FlushCache("Flush Key")]
            public abstract int? Flush();
            [IsCacheable("IsCacheable Key", "00:11:22")]
            public abstract int? IsCacheable();
        }

        private ITestObject CreateCachedProxy(TestObject testObject) =>
             CachedProxy<ITestObject, TestObject>.Create(
                 testObject, 
                 mockCachingManager.Object,
                 this.TestContext.GetTestLoggingServices<TestObject>()
                 );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_Bypassed()
        {
            // Stage
            var realResult = 999;

            // Mock
            var decorated = mockRepository.Create<TestObject>();
            decorated.Setup(s => s.Bypass()).Returns(realResult);

            // Test
            var proxy = CreateCachedProxy(decorated.Object);
            var returned = proxy.Bypass();

            // Assert
            Assert.AreEqual(realResult, returned);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_IsCached_CacheMiss()
        {
            // Stage
            int? cachedResult = null;
            int? realResult = 999;
            var cacheKey = "test key";

            // Mock
            var decorated = mockRepository.Create<TestObject>();
            decorated.Setup(s => s.IsCacheable()).Returns(realResult);
            mockCachingManager.Setup(s => s.BuildKey(It.IsAny<MethodInfo>(), It.IsAny<object[]>())).Returns(cacheKey);
            mockCachingManager.Setup(s => s.RetreiveAsync(cacheKey, typeof(int?))).ReturnsAsync(cachedResult);
            mockCachingManager.Setup(s => s.StoreAsync(cacheKey, realResult, new TimeSpan(0, 11, 22))).Returns(Task.FromResult(0));

            // Test
            var proxy = CreateCachedProxy(decorated.Object);
            var returned = proxy.IsCacheable();

            // Assert
            Assert.AreEqual(realResult, returned);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_FlushCache()
        {
            // Stage
            int? realResult = 999;
            var cacheKey = "test key";

            // Mock
            var decorated = mockRepository.Create<TestObject>();
            decorated.Setup(s => s.Flush()).Returns(realResult);
            mockCachingManager.Setup(s => s.BuildKey(It.IsAny<MethodInfo>(), It.IsAny<object[]>())).Returns(cacheKey);
            mockCachingManager.Setup(s => s.FlushAsync(cacheKey)).Returns(Task.FromResult(0));

            // Test
            var proxy = CreateCachedProxy(decorated.Object);
            var returned = proxy.Flush();

            // Assert
            Assert.AreEqual(realResult, returned);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}