using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using OoBDev.Extensions.Configuration;
using OoBDev.Redis.Caching.Providers;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
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
    [TestCategory(TestCategories.Integration)]
    public async Task FlushAsyncTest()
    {
        // Stage
        var connectionString = TestContext.GetRequiredProperty<string>("REDIS_CONNECTION_STRING");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Redis:ConnectionMultiplexer:Config", connectionString }
            })
            .Build();

        // Test
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .TryAddRedisCachingServices()
            .BuildServiceProvider();

        var provider = ActivatorUtilities.CreateInstance<RedisCachingProvider>(services);
        string? key = null;

        await provider.FlushAsync(key);

        // Assert - no exception thrown
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task RetreiveAsyncTest()
    {
        // Stage
        var connectionString = TestContext.GetRequiredProperty<string>("REDIS_CONNECTION_STRING");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Redis:ConnectionMultiplexer:Config", connectionString }
            })
            .Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .TryAddRedisCachingServices()
            .BuildServiceProvider();

        var provider = ActivatorUtilities.CreateInstance<RedisCachingProvider>(services);

        // First store a value
        var key = $"TestKey_{Guid.NewGuid():N}";
        var testData = new { Hello = "World!", Timestamp = DateTime.UtcNow };
        await provider.StoreAsync(key, testData, TimeSpan.FromMinutes(5));

        // Test
        Type targetType = typeof(JObject);
        var result = await provider.RetreiveAsync(key, targetType);

        // Assert
        Assert.IsNotNull(result);

        // Cleanup
        await provider.FlushAsync(key);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task StoreAsyncTest()
    {
        // Stage
        var connectionString = TestContext.GetRequiredProperty<string>("REDIS_CONNECTION_STRING");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Redis:ConnectionMultiplexer:Config", connectionString }
            })
            .Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .TryAddRedisCachingServices()
            .BuildServiceProvider();

        var provider = ActivatorUtilities.CreateInstance<RedisCachingProvider>(services);
        var key = $"TestKey_{Guid.NewGuid():N}";
        var data = new
        {
            Hello = "World!",
            TimeStamp = DateTime.UtcNow,
        };
        var expiration = new TimeSpan(5, 0, 0);

        // Test
        await provider.StoreAsync(key, data, expiration);

        // Verify by retrieving
        var result = await provider.RetreiveAsync(key, data.GetType());
        Assert.IsNotNull(result);

        // Cleanup
        await provider.FlushAsync(key);
    }
}
