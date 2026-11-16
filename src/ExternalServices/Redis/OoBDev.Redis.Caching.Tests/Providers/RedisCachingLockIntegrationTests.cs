using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Extensions.Configuration;
using OoBDev.Redis.Caching.Providers;
using OoBDev.System;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Redis.Caching.Tests.Providers;

[TestClass]
public class RedisCachingLockIntegrationTests
{
    public required TestContext TestContext { get; set; }

    private RedisCachingProvider CreateProvider()
    {
        var connectionString = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("REDIS_CONNECTION_STRING"));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { { "Redis:ConnectionMultiplexer:Config", connectionString } })
            .Build();
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .TryAddSystemExtensions(configuration, new())
            .TryAddRedisCachingServices()
            .BuildServiceProvider();
        return ActivatorUtilities.CreateInstance<RedisCachingProvider>(services);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task TryAcquireAsync_TwoProviders_OnlyOneHoldsUntilReleased()
    {
        // Stage
        var key = $"Test_{Guid.NewGuid():N}";
        var first = CreateProvider();
        var second = CreateProvider();

        // Test
        var held = await first.TryAcquireAsync(key, TimeSpan.FromSeconds(30));
        var contender = await second.TryAcquireAsync(key, TimeSpan.FromSeconds(30));
        Assert.IsNotNull(held);
        Assert.IsNull(contender, "A second process must not get a lock that is held.");

        await held.DisposeAsync();
        var afterRelease = await second.TryAcquireAsync(key, TimeSpan.FromSeconds(30));

        // Assert
        Assert.IsNotNull(afterRelease);
        await afterRelease.DisposeAsync();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task TryAcquireAsync_OwnerDies_LockExpires()
    {
        var key = $"Test_{Guid.NewGuid():N}";
        var first = CreateProvider();
        var second = CreateProvider();

        _ = await first.TryAcquireAsync(key, TimeSpan.FromMilliseconds(300));
        await Task.Delay(600);

        var taken = await second.TryAcquireAsync(key, TimeSpan.FromSeconds(5));
        Assert.IsNotNull(taken);
        await taken.DisposeAsync();
    }
}
