using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OoBDev.Redis.Caching.Providers;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.Redis.Caching.Tests.Providers;

[TestClass]
public class RedisCachingProviderDevLocalTests
{
    public required TestContext TestContext { get; set; }

    private RedisCachingProvider CreateProvider()
    {
        var services = new ServiceCollection()
            //
            //.AddDebugTestConfigurations(
            //    (ConnectionMultiplexerFactory.SourceConfigurationKey, "localhost")
            //    )
            .TryAddRedisCachingServices()
            //.AddToolkitServices()
        ;

        var serviceProvider = services.BuildServiceProvider();

        var redis = ActivatorUtilities.CreateInstance<RedisCachingProvider>(serviceProvider);
        return redis;
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task FlushAsyncTest()
    {
        // Stage

        // Mock

        // Test
        var provider = this.CreateProvider();
        string key = null;


        await provider.FlushAsync(
            key);

        // Assert
        Assert.Fail();
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task RetreiveAsyncTest()
    {
        // Stage

        // Mock

        // Test
        var provider = this.CreateProvider();
        string key = "Claims/User/6bb0e835-06b1-44ae-8907-7f5e3ebb20d7";
        Type targetType = typeof(JObject);

        var result = await provider.RetreiveAsync(
            key,
            targetType);

        // Assert
        Assert.IsNotNull(result);

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task StoreAsyncTest()
    {
        // Stage

        // Mock

        // Test
        var provider = this.CreateProvider();
        var key = "Claims/User/6bb0e835-06b1-44ae-8907-7f5e3ebb20d7";
        var data = new
        {
            Hello = "World!",
            TimeStamp = DateTime.Now,
        };
        var expiration = new TimeSpan(5, 0, 0);

        await provider.StoreAsync(key, data, expiration);

        // Assert

        // Verify
    }
}
