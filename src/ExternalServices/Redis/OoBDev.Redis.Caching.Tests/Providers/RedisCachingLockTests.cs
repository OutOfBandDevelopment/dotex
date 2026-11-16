using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Redis.Caching.Providers;
using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace OoBDev.Redis.Caching.Tests.Providers;

[TestClass]
public class RedisCachingLockTests
{
    private MockRepository mockRepository = null!;
    private Mock<IDatabase> database = null!;
    private RedisCachingProvider provider = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        mockRepository = new MockRepository(MockBehavior.Strict);
        var factory = mockRepository.Create<IConnectionMultiplexerFactory>();
        var multiplexer = mockRepository.Create<IConnectionMultiplexer>();
        database = mockRepository.Create<IDatabase>();
        factory.Setup(s => s.Create()).Returns(multiplexer.Object);
        multiplexer.Setup(s => s.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(database.Object);
        provider = new RedisCachingProvider(
            mockRepository.Create<IObjectConverter>().Object,
            mockRepository.Create<IJsonSerializer>().Object,
            factory.Object);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task TryAcquireAsync_Free_TakesPrefixedLockAndReleasesWithTheSameToken()
    {
        // Stage
        RedisValue taken = default;
        database.Setup(s => s.LockTakeAsync("lock:k", It.IsAny<RedisValue>(), TimeSpan.FromSeconds(30), It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, TimeSpan, CommandFlags>((_, token, _, _) => taken = token)
            .ReturnsAsync(true);
        database.Setup(s => s.LockReleaseAsync("lock:k", It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .Returns<RedisKey, RedisValue, CommandFlags>((_, token, _) => Task.FromResult(token == taken));

        // Test
        var held = await provider.TryAcquireAsync("k", TimeSpan.FromSeconds(30));
        Assert.IsNotNull(held);
        await held.DisposeAsync();

        // Verify
        Assert.IsFalse(taken.IsNullOrEmpty);
        database.Verify(s => s.LockReleaseAsync("lock:k", taken, It.IsAny<CommandFlags>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task TryAcquireAsync_HeldElsewhere_ReturnsNull()
    {
        database.Setup(s => s.LockTakeAsync("lock:k", It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        Assert.IsNull(await provider.TryAcquireAsync("k", TimeSpan.FromSeconds(30)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task TryAcquireAsync_BlankKey_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => provider.TryAcquireAsync(" ", TimeSpan.FromSeconds(1)));
    }
}
