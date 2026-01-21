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
public class RedisCachingProviderTests
{
    public required TestContext TestContext { get; set; }

    private MockRepository mockRepository = null!;

    private Mock<IObjectConverter> mockObjectConverter = null!;
    private Mock<IJsonSerializer> mockJsonSerializer = null!;
    private Mock<IConnectionMultiplexerFactory> mockConnectionMultiplexerFactory = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        this.mockRepository = new MockRepository(MockBehavior.Strict);

        this.mockObjectConverter = this.mockRepository.Create<IObjectConverter>();
        this.mockJsonSerializer = this.mockRepository.Create<IJsonSerializer>();
        this.mockConnectionMultiplexerFactory = this.mockRepository.Create<IConnectionMultiplexerFactory>();
    }

    private RedisCachingProvider CreateProvider() => new(
            this.mockObjectConverter.Object,
            mockJsonSerializer.Object,
            this.mockConnectionMultiplexerFactory.Object
        );

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task FlushAsyncTest_NullKey()
    {
        // Stage

        // Mock

        // Test
        var provider = this.CreateProvider();
        string? key = null;


        await provider.FlushAsync(key);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task FlushAsyncTest()
    {
        // Stage
        string key = Guid.NewGuid().ToString();

        // Mock
        var mockConnectionMultiplexer = mockRepository.Create<IConnectionMultiplexer>();
        var mockDatabase = mockRepository.Create<IDatabase>();
        mockConnectionMultiplexerFactory.Setup(s => s.Create()).Returns(mockConnectionMultiplexer.Object);
        mockConnectionMultiplexer.Setup(s => s.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
        mockDatabase.Setup(s => s.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).Returns(Task.FromResult(true));

        // Test
        var provider = this.CreateProvider();

        await provider.FlushAsync(key);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RetreiveAsyncTest_NullKey()
    {
        // Stage

        // Mock

        // Test
        var provider = this.CreateProvider();
        string? key = null;
        Type? targetType = null;


        var result = await provider.RetreiveAsync(key, targetType);

        // Assert
        Assert.IsNull(result);

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RetreiveAsyncTest()
    {
        // Stage
        var testValue = Guid.NewGuid().ToString();
        var redisValue = new RedisValue(testValue);

        string key = Guid.NewGuid().ToString();
        Type targetType = typeof(object);

        // Mock
        var mockConnectionMultiplexer = mockRepository.Create<IConnectionMultiplexer>();
        var mockDatabase = mockRepository.Create<IDatabase>();
        mockConnectionMultiplexerFactory.Setup(s => s.Create()).Returns(mockConnectionMultiplexer.Object);
        mockConnectionMultiplexer.Setup(s => s.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
        mockDatabase.Setup(s => s.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(redisValue);
        mockObjectConverter.Setup(s => s.Convert(It.IsAny<object>(), targetType)).Returns(testValue);

        // Test
        var provider = this.CreateProvider();

        var result = await provider.RetreiveAsync(key, targetType);

        // Assert
        Assert.AreEqual(testValue, result);

        // Verify
        this.mockRepository.VerifyAll();
    }


    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsyncTest_NullKey()
    {
        // Stage

        // Mock

        // Test
        var provider = this.CreateProvider();
        string? key = null;
        object? data = null;
        var expiration = TimeSpan.MinValue;

        await provider.StoreAsync(key, data, expiration);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsyncTest()
    {
        // Stage
        var key = Guid.NewGuid().ToString();
        var data = new { };
        var testValue = "{}";
        var expiration = new TimeSpan(1234, 23, 24);

        // Mock
        var mockConnectionMultiplexer = mockRepository.Create<IConnectionMultiplexer>();
        var mockDatabase = mockRepository.Create<IDatabase>();
        mockConnectionMultiplexerFactory.Setup(s => s.Create()).Returns(mockConnectionMultiplexer.Object);
        mockConnectionMultiplexer.Setup(s => s.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDatabase.Object);
        mockJsonSerializer.Setup(s => s.Serialize(It.IsAny<object>(), It.IsAny<Type>())).Returns(testValue);
        mockDatabase.Setup(s => s.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<Expiration>(),
            It.IsAny<ValueCondition>(),
            It.IsAny<CommandFlags>()
            )).ReturnsAsync(true);

        // Test
        var provider = this.CreateProvider();

        await provider.StoreAsync(key, data, expiration);

        // Assert

        // Verify
        this.mockRepository.VerifyAll();
    }
}
