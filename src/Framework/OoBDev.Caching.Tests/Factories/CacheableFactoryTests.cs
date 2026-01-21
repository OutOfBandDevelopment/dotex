using OoBDev.Caching.Factories;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Caching.Tests.Factories;

[TestClass]
public class CacheableFactoryTests
{
    public required TestContext TestContext { get; set; }

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

    private CacheableFactory CreateFactory() => new(
            this.mockServiceProvider.Object,
            this.mockCachingManager.Object,
            this.mockConfiguration.Object
        );

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
        mockServiceProvider.Setup(s => s.GetService(typeof(Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)))
                                        .Returns(null);
        mockServiceProvider.Setup(s => s.GetService(typeof(ILogger<TestObject>)))
                                        .Returns(this.TestContext.GetLogger<TestObject>()
                                        );

        // Test
        var factory = this.CreateFactory();

        var result = factory.Create<ITestObject, TestObject>();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotInstanceOfType<TestObject>(result);
        Assert.IsInstanceOfType<CachedProxy<ITestObject, TestObject>>(result);

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
        mockServiceProvider.Setup(s => s.GetService(typeof(Microsoft.Extensions.DependencyInjection.IServiceProviderIsService)))
                                        .Returns(null);

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
