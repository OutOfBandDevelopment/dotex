using OoBDev.Caching.Common.Factories;
using OoBDev.Caching.Contracts;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Caching.Common.Tests.Factories
{
    [TestClass]
    public class CacheableFactoryTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IServiceProvider> mockServiceProvider;
        private Mock<ICachingManager> mockCachingManager;
        private Mock<IConfiguration> mockConfiguration;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockServiceProvider = this.mockRepository.Create<IServiceProvider>();
            this.mockCachingManager = this.mockRepository.Create<ICachingManager>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private CacheableFactory CreateFactory()
        {
            return new CacheableFactory(
                this.mockServiceProvider.Object,
                this.mockCachingManager.Object,
                this.mockConfiguration.Object);
        }

        public interface ITestObject
        {
        }
        public class TestObject : ITestObject
        {
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_Enabled()
        {
            // Stage

            // Mock
            mockConfiguration.Setup(s => s[CacheableFactory.DisabledConfigurationKey]).Returns("false");
            mockServiceProvider.Setup(s => s.GetService(typeof(ILogger<TestObject>)))
                                            .Returns(this.TestContext.GetTestLoggingServices<TestObject>()
                                            );

            // Test
            var factory = this.CreateFactory();

            var result = factory.Create<ITestObject, TestObject>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotInstanceOfType(result, typeof(TestObject));
            Assert.IsInstanceOfType(result, typeof(CachedProxy<ITestObject, TestObject>));

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_Disabled()
        {
            // Stage

            // Mock
            mockConfiguration.Setup(s => s[CacheableFactory.DisabledConfigurationKey]).Returns("true");

            // Test
            var factory = this.CreateFactory();

            var result = factory.Create<ITestObject, TestObject>();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(TestObject));

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
