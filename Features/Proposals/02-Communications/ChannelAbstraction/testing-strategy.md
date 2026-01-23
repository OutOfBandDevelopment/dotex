# Channel Abstraction - Testing Strategy

**Epic:** 2 - Communications Platform
**Feature:** Channel Abstraction
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks
- **Integration Tests** - End-to-end scenarios with real database
- **Performance Tests** - Benchmark provider lookup and caching
- **Concurrency Tests** - Thread-safety verification

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (5 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Integration Tests│  (15 tests)
                  │                   │
                  └───────────────────┘
            ┌─────────────────────────────┐
            │       Unit Tests            │  (55+ tests)
            │                             │
            └─────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. Channel Tests

**File:** `ChannelTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Communications;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Communications.Tests;

[TestClass]
public class ChannelTests
{
    [TestMethod]
    public void Constructor_ValidParameters_CreatesChannel()
    {
        // Arrange & Act
        var channel = new Channel("support-email", "email", "sendgrid");

        // Assert
        Assert.AreEqual("support-email", channel.Name);
        Assert.AreEqual("email", channel.Protocol);
        Assert.AreEqual("sendgrid", channel.Provider);
        Assert.IsTrue(channel.IsEnabled);
        Assert.IsNotNull(channel.Configuration);
        Assert.IsNotNull(channel.CreatedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_EmptyName_ThrowsArgumentException()
    {
        // Act
        var channel = new Channel("", "email", "sendgrid");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_NullProtocol_ThrowsArgumentException()
    {
        // Act
        var channel = new Channel("test", null, "sendgrid");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_InvalidNameFormat_ThrowsArgumentException()
    {
        // Act - Name with spaces not allowed
        var channel = new Channel("invalid name", "email", "sendgrid");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Constructor_ProtocolWithUppercase_ThrowsArgumentException()
    {
        // Act - Protocol must be lowercase
        var channel = new Channel("test", "Email", "sendgrid");
    }

    [TestMethod]
    public void IsEnabled_SetToFalse_DisablesChannel()
    {
        // Arrange
        var channel = new Channel("test", "email", "sendgrid");

        // Act
        channel.IsEnabled = false;

        // Assert
        Assert.IsFalse(channel.IsEnabled);
    }

    [TestMethod]
    public void Configuration_AddKeyValue_StoresConfiguration()
    {
        // Arrange
        var channel = new Channel("test", "email", "sendgrid");

        // Act
        channel.Configuration["ApiKey"] = "SG.xxx";
        channel.Configuration["FromEmail"] = "test@example.com";

        // Assert
        Assert.AreEqual("SG.xxx", channel.Configuration["ApiKey"]);
        Assert.AreEqual("test@example.com", channel.Configuration["FromEmail"]);
    }
}
```

---

#### 2. ChannelFactory Tests

**File:** `ChannelFactoryTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class ChannelFactoryTests
{
    [TestMethod]
    public void CreateEmailChannel_ValidParameters_CreatesChannel()
    {
        // Arrange
        var config = new Dictionary<string, object>
        {
            ["ApiKey"] = "SG.xxx",
            ["FromEmail"] = "test@example.com"
        };

        // Act
        var channel = ChannelFactory.CreateEmailChannel(
            "support-email",
            "sendgrid",
            config);

        // Assert
        Assert.AreEqual("support-email", channel.Name);
        Assert.AreEqual("email", channel.Protocol);
        Assert.AreEqual("sendgrid", channel.Provider);
        Assert.AreEqual("SG.xxx", channel.Configuration["ApiKey"]);
    }

    [TestMethod]
    public void CreateSmsChannel_ValidParameters_CreatesChannel()
    {
        // Arrange
        var config = new Dictionary<string, object>
        {
            ["AccountSid"] = "AC123",
            ["AuthToken"] = "xxx",
            ["FromPhoneNumber"] = "+15551234567"
        };

        // Act
        var channel = ChannelFactory.CreateSmsChannel(
            "alerts-sms",
            "twilio",
            config);

        // Assert
        Assert.AreEqual("alerts-sms", channel.Name);
        Assert.AreEqual("sms", channel.Protocol);
        Assert.AreEqual("twilio", channel.Provider);
    }

    [TestMethod]
    public void CreateSlackChannel_ValidParameters_CreatesChannel()
    {
        // Arrange & Act
        var channel = ChannelFactory.CreateSlackChannel(
            "sales-slack",
            "https://hooks.slack.com/services/xxx",
            "#sales");

        // Assert
        Assert.AreEqual("sales-slack", channel.Name);
        Assert.AreEqual("slack", channel.Protocol);
        Assert.AreEqual("slack-api", channel.Provider);
        Assert.AreEqual("https://hooks.slack.com/services/xxx",
            channel.Configuration["WebhookUrl"]);
        Assert.AreEqual("#sales", channel.Configuration["Channel"]);
    }

    [TestMethod]
    public void Create_CustomProtocol_CreatesChannel()
    {
        // Arrange
        var config = new Dictionary<string, object>
        {
            ["Endpoint"] = "https://api.custom.com"
        };

        // Act
        var channel = ChannelFactory.Create(
            "custom-channel",
            "custom",
            "custom-provider",
            config);

        // Assert
        Assert.AreEqual("custom", channel.Protocol);
        Assert.AreEqual("custom-provider", channel.Provider);
    }
}
```

---

#### 3. ChannelRegistry Tests

**File:** `ChannelRegistryTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class ChannelRegistryTests
{
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<ILogger<ChannelRegistry>> _mockLogger;
    private ChannelRegistry _registry;

    [TestInitialize]
    public void Setup()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<ChannelRegistry>>();

        // Setup empty provider collection
        _mockServiceProvider
            .Setup(sp => sp.GetService(typeof(IEnumerable<IChannelProvider>)))
            .Returns(Enumerable.Empty<IChannelProvider>());

        _registry = new ChannelRegistry(_mockServiceProvider.Object, _mockLogger.Object);
    }

    [TestMethod]
    public void RegisterProvider_ValidProvider_StoresProvider()
    {
        // Arrange
        var mockProvider = new Mock<IChannelProvider>();
        mockProvider.Setup(p => p.ProviderName).Returns("sendgrid");
        mockProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });

        // Act
        _registry.RegisterProvider(mockProvider.Object);

        // Assert
        var provider = _registry.GetProvider("email", "sendgrid");
        Assert.IsNotNull(provider);
        Assert.AreEqual("sendgrid", provider.ProviderName);
    }

    [TestMethod]
    public void RegisterProvider_MultipleProtocols_RegistersForAllProtocols()
    {
        // Arrange
        var mockProvider = new Mock<IChannelProvider>();
        mockProvider.Setup(p => p.ProviderName).Returns("multi-provider");
        mockProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email", "sms" });

        // Act
        _registry.RegisterProvider(mockProvider.Object);

        // Assert
        var emailProvider = _registry.GetProvider("email", "multi-provider");
        var smsProvider = _registry.GetProvider("sms", "multi-provider");

        Assert.IsNotNull(emailProvider);
        Assert.IsNotNull(smsProvider);
        Assert.AreSame(emailProvider, smsProvider);  // Same instance
    }

    [TestMethod]
    public void GetProvider_ProviderNotFound_ReturnsNull()
    {
        // Act
        var provider = _registry.GetProvider("email", "nonexistent");

        // Assert
        Assert.IsNull(provider);
    }

    [TestMethod]
    public void GetProvider_WrongProtocol_ReturnsNull()
    {
        // Arrange
        var mockProvider = new Mock<IChannelProvider>();
        mockProvider.Setup(p => p.ProviderName).Returns("sendgrid");
        mockProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });
        _registry.RegisterProvider(mockProvider.Object);

        // Act
        var provider = _registry.GetProvider("sms", "sendgrid");  // Wrong protocol

        // Assert
        Assert.IsNull(provider);
    }

    [TestMethod]
    public void GetProvidersByProtocol_MultipleProviders_ReturnsAll()
    {
        // Arrange
        var sendGridProvider = new Mock<IChannelProvider>();
        sendGridProvider.Setup(p => p.ProviderName).Returns("sendgrid");
        sendGridProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });

        var smtpProvider = new Mock<IChannelProvider>();
        smtpProvider.Setup(p => p.ProviderName).Returns("smtp");
        smtpProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });

        _registry.RegisterProvider(sendGridProvider.Object);
        _registry.RegisterProvider(smtpProvider.Object);

        // Act
        var providers = _registry.GetProvidersByProtocol("email").ToList();

        // Assert
        Assert.AreEqual(2, providers.Count);
        Assert.IsTrue(providers.Any(p => p.ProviderName == "sendgrid"));
        Assert.IsTrue(providers.Any(p => p.ProviderName == "smtp"));
    }

    [TestMethod]
    public void GetProvidersByProtocol_NoProviders_ReturnsEmpty()
    {
        // Act
        var providers = _registry.GetProvidersByProtocol("nonexistent");

        // Assert
        Assert.IsFalse(providers.Any());
    }

    [TestMethod]
    public void GetSupportedProtocols_MultipleProviders_ReturnsAllProtocols()
    {
        // Arrange
        var emailProvider = new Mock<IChannelProvider>();
        emailProvider.Setup(p => p.ProviderName).Returns("sendgrid");
        emailProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });

        var smsProvider = new Mock<IChannelProvider>();
        smsProvider.Setup(p => p.ProviderName).Returns("twilio");
        smsProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "sms" });

        _registry.RegisterProvider(emailProvider.Object);
        _registry.RegisterProvider(smsProvider.Object);

        // Act
        var protocols = _registry.GetSupportedProtocols().ToList();

        // Assert
        Assert.AreEqual(2, protocols.Count);
        Assert.IsTrue(protocols.Contains("email"));
        Assert.IsTrue(protocols.Contains("sms"));
    }

    [TestMethod]
    public void RegisterProvider_Concurrent_ThreadSafe()
    {
        // Arrange
        var providers = Enumerable.Range(0, 100).Select(i =>
        {
            var mock = new Mock<IChannelProvider>();
            mock.Setup(p => p.ProviderName).Returns($"provider-{i}");
            mock.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });
            return mock.Object;
        }).ToList();

        // Act - Register providers concurrently
        Parallel.ForEach(providers, provider =>
        {
            _registry.RegisterProvider(provider);
        });

        // Assert - All providers registered
        var emailProviders = _registry.GetProvidersByProtocol("email").ToList();
        Assert.AreEqual(100, emailProviders.Count);
    }
}
```

---

#### 4. ChannelRepository Tests

**File:** `ChannelRepositoryTests.cs`

**Test Cases:**

```csharp
[TestClass]
[TestCategory(TestCategories.Unit)]
public class ChannelRepositoryTests
{
    private Mock<CommunicationsDbContext> _mockDbContext;
    private Mock<IMemoryCache> _mockCache;
    private Mock<ILogger<ChannelRepository>> _mockLogger;
    private ChannelRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        _mockDbContext = new Mock<CommunicationsDbContext>();
        _mockCache = new Mock<IMemoryCache>();
        _mockLogger = new Mock<ILogger<ChannelRepository>>();

        _repository = new ChannelRepository(
            _mockDbContext.Object,
            _mockCache.Object,
            _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetByNameAsync_ChannelInCache_ReturnsCachedChannel()
    {
        // Arrange
        var channel = new Channel("test", "email", "sendgrid");
        var cacheKey = "Channel:test";

        object cachedChannel = channel;
        _mockCache.Setup(c => c.TryGetValue(cacheKey, out cachedChannel))
            .Returns(true);

        // Act
        var result = await _repository.GetByNameAsync("test");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Name);

        // Verify database NOT queried (cache hit)
        _mockDbContext.Verify(db => db.Channels, Times.Never);
    }

    [TestMethod]
    public async Task GetByNameAsync_ChannelNotInCache_QueriesDatabase()
    {
        // Arrange
        var channel = new Channel("test", "email", "sendgrid");
        var cacheKey = "Channel:test";

        object cachedChannel = null;
        _mockCache.Setup(c => c.TryGetValue(cacheKey, out cachedChannel))
            .Returns(false);

        // Mock database query (simplified - actual implementation uses DbSet)
        // (In real tests, use in-memory database or mock DbSet properly)

        // Act
        // var result = await _repository.GetByNameAsync("test");

        // Assert
        // Verify cache.Set called to cache the result
        // _mockCache.Verify(c => c.Set(cacheKey, It.IsAny<IChannel>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_UniqueChannel_CreatesAndCaches()
    {
        // Arrange
        var channel = new Channel("new-channel", "email", "sendgrid");

        // Mock database - no existing channel
        // (Implementation depends on DbContext mocking strategy)

        // Act
        // var result = await _repository.CreateAsync(channel);

        // Assert
        // Verify channel saved to database
        // Verify channel cached
        // _mockCache.Verify(c => c.Set(It.IsAny<string>(), channel, It.IsAny<TimeSpan>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var channel = new Channel("duplicate", "email", "sendgrid");

        // Mock database - existing channel with same name
        // (Setup DbContext to return existing channel)

        // Act
        await _repository.CreateAsync(channel);

        // Should throw InvalidOperationException
    }

    [TestMethod]
    public async Task UpdateAsync_ExistingChannel_UpdatesAndInvalidatesCache()
    {
        // Arrange
        var channel = new Channel("test", "email", "sendgrid");
        channel.Configuration["ApiKey"] = "new-key";

        // Act
        // await _repository.UpdateAsync(channel);

        // Assert
        // Verify database updated
        // Verify cache invalidated
        // _mockCache.Verify(c => c.Remove("Channel:test"), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public async Task UpdateAsync_NonexistentChannel_ThrowsInvalidOperationException()
    {
        // Arrange
        var channel = new Channel("nonexistent", "email", "sendgrid");

        // Act
        await _repository.UpdateAsync(channel);

        // Should throw
    }

    [TestMethod]
    public async Task DeleteAsync_ExistingChannel_SoftDeletesAndInvalidatesCache()
    {
        // Arrange
        var channelName = "test";

        // Act
        // await _repository.DeleteAsync(channelName);

        // Assert
        // Verify channel.IsArchived = true
        // Verify cache invalidated
        // _mockCache.Verify(c => c.Remove($"Channel:{channelName}"), Times.Once);
    }

    [TestMethod]
    public async Task GetByProtocolAsync_MultipleChannels_ReturnsAll()
    {
        // Arrange
        // Mock database with multiple email channels

        // Act
        // var channels = await _repository.GetByProtocolAsync("email");

        // Assert
        // Verify all email channels returned
        // Verify archived channels excluded
    }
}
```

---

#### 5. Channel Provider Tests

**File:** `SendGridEmailProviderTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class SendGridEmailProviderTests
{
    private Mock<HttpClient> _mockHttpClient;
    private Mock<ILogger<SendGridEmailProvider>> _mockLogger;
    private SendGridEmailProvider _provider;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _mockLogger = new Mock<ILogger<SendGridEmailProvider>>();

        _provider = new SendGridEmailProvider(_mockHttpClient.Object, _mockLogger.Object);
    }

    [TestMethod]
    public void ProviderName_Always_ReturnsSendGrid()
    {
        // Act
        var name = _provider.ProviderName;

        // Assert
        Assert.AreEqual("sendgrid", name);
    }

    [TestMethod]
    public void SupportedProtocols_Always_ReturnsEmail()
    {
        // Act
        var protocols = _provider.SupportedProtocols;

        // Assert
        Assert.AreEqual(1, protocols.Length);
        Assert.AreEqual("email", protocols[0]);
    }

    [TestMethod]
    public void SupportsSending_Always_ReturnsTrue()
    {
        // Assert
        Assert.IsTrue(_provider.SupportsSending);
    }

    [TestMethod]
    public void SupportsReceiving_Always_ReturnsFalse()
    {
        // Assert
        Assert.IsFalse(_provider.SupportsReceiving);  // SendGrid is send-only
    }

    [TestMethod]
    public void SupportsWebhooks_Always_ReturnsTrue()
    {
        // Assert
        Assert.IsTrue(_provider.SupportsWebhooks);
    }

    [TestMethod]
    public async Task CanSendAsync_ValidChannelAndMessage_ReturnsTrue()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel(
            "test",
            "sendgrid",
            new Dictionary<string, object>
            {
                ["ApiKey"] = "SG.xxx",
                ["FromEmail"] = "test@example.com"
            });

        var message = new EmailMessage
        {
            To = new[] { "recipient@example.com" },
            Subject = "Test",
            HtmlContent = "<p>Test</p>"
        };

        // Act
        var canSend = await _provider.CanSendAsync(channel, message);

        // Assert
        Assert.IsTrue(canSend);
    }

    [TestMethod]
    public async Task CanSendAsync_MissingApiKey_ReturnsFalse()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel(
            "test",
            "sendgrid",
            new Dictionary<string, object>
            {
                ["FromEmail"] = "test@example.com"
                // Missing ApiKey
            });

        var message = new EmailMessage();

        // Act
        var canSend = await _provider.CanSendAsync(channel, message);

        // Assert
        Assert.IsFalse(canSend);
    }

    [TestMethod]
    public async Task CanSendAsync_WrongMessageType_ReturnsFalse()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel(
            "test",
            "sendgrid",
            new Dictionary<string, object>
            {
                ["ApiKey"] = "SG.xxx",
                ["FromEmail"] = "test@example.com"
            });

        var message = new SmsMessage();  // Wrong type!

        // Act
        var canSend = await _provider.CanSendAsync(channel, message);

        // Assert
        Assert.IsFalse(canSend);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task SendAsync_WrongMessageType_ThrowsArgumentException()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel("test", "sendgrid", new Dictionary<string, object>());
        var message = new SmsMessage();  // Wrong type

        // Act
        await _provider.SendAsync(channel, message);
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public async Task ReceiveAsync_Always_ThrowsNotSupportedException()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel("test", "sendgrid", new Dictionary<string, object>());

        // Act
        await _provider.ReceiveAsync(channel);  // Should throw
    }
}
```

---

## Integration Tests

### Test Scenarios

#### 1. End-to-End Channel Lifecycle

**File:** `ChannelLifecycleIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class ChannelLifecycleIntegrationTests
{
    private IChannelRepository _repository;
    private IChannelRegistry _registry;
    private IMemoryCache _cache;

    [TestInitialize]
    public void Setup()
    {
        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<CommunicationsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        var dbContext = new CommunicationsDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<ChannelRepository>>().Object;

        _repository = new ChannelRepository(dbContext, _cache, logger);
        _registry = new ChannelRegistry(
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<ChannelRegistry>>().Object);
    }

    [TestMethod]
    public async Task CreateAndRetrieveChannel_Success()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel(
            "integration-test-email",
            "sendgrid",
            new Dictionary<string, object>
            {
                ["ApiKey"] = "SG.test",
                ["FromEmail"] = "test@example.com"
            });

        // Act
        await _repository.CreateAsync(channel);
        var retrieved = await _repository.GetByNameAsync("integration-test-email");

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("integration-test-email", retrieved.Name);
        Assert.AreEqual("email", retrieved.Protocol);
        Assert.AreEqual("sendgrid", retrieved.Provider);
    }

    [TestMethod]
    public async Task UpdateChannel_ConfigurationChanged_PersistsChanges()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel(
            "update-test",
            "sendgrid",
            new Dictionary<string, object>
            {
                ["ApiKey"] = "SG.old"
            });

        await _repository.CreateAsync(channel);

        // Act
        var retrieved = await _repository.GetByNameAsync("update-test");
        retrieved.Configuration["ApiKey"] = "SG.new";
        await _repository.UpdateAsync(retrieved);

        // Assert
        var updated = await _repository.GetByNameAsync("update-test");
        Assert.AreEqual("SG.new", updated.Configuration["ApiKey"]);
    }

    [TestMethod]
    public async Task DeleteChannel_SoftDelete_ChannelArchived()
    {
        // Arrange
        var channel = ChannelFactory.CreateEmailChannel(
            "delete-test",
            "sendgrid",
            new Dictionary<string, object>());

        await _repository.CreateAsync(channel);

        // Act
        await _repository.DeleteAsync("delete-test");

        // Assert
        var retrieved = await _repository.GetByNameAsync("delete-test");
        Assert.IsNull(retrieved);  // Archived channels not returned
    }

    [TestMethod]
    public async Task GetByProtocol_MultipleChannels_ReturnsCorrectChannels()
    {
        // Arrange
        await _repository.CreateAsync(
            ChannelFactory.CreateEmailChannel("email-1", "sendgrid", new Dictionary<string, object>()));
        await _repository.CreateAsync(
            ChannelFactory.CreateEmailChannel("email-2", "smtp", new Dictionary<string, object>()));
        await _repository.CreateAsync(
            ChannelFactory.CreateSmsChannel("sms-1", "twilio", new Dictionary<string, object>()));

        // Act
        var emailChannels = await _repository.GetByProtocolAsync("email");
        var smsChannels = await _repository.GetByProtocolAsync("sms");

        // Assert
        Assert.AreEqual(2, emailChannels.Count());
        Assert.AreEqual(1, smsChannels.Count());
    }
}
```

---

## Performance Tests

**File:** `PerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]  // Run manually for performance analysis
public class PerformanceTests
{
    [TestMethod]
    public async Task ChannelLookup_Cached_PerformanceBenchmark()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CommunicationsDbContext>()
            .UseInMemoryDatabase("PerformanceDb")
            .Options;

        var dbContext = new CommunicationsDbContext(options);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<ChannelRepository>>().Object;
        var repository = new ChannelRepository(dbContext, cache, logger);

        var channel = ChannelFactory.CreateEmailChannel("perf-test", "sendgrid", new Dictionary<string, object>());
        await repository.CreateAsync(channel);

        // Act - First lookup (database)
        var stopwatch = Stopwatch.StartNew();
        await repository.GetByNameAsync("perf-test");
        stopwatch.Stop();
        var dbTime = stopwatch.ElapsedMilliseconds;

        // Act - Second lookup (cache)
        stopwatch.Restart();
        await repository.GetByNameAsync("perf-test");
        stopwatch.Stop();
        var cacheTime = stopwatch.ElapsedMilliseconds;

        // Assert
        Console.WriteLine($"Database lookup: {dbTime}ms");
        Console.WriteLine($"Cache lookup: {cacheTime}ms");
        Console.WriteLine($"Cache speedup: {dbTime / (double)cacheTime:F2}x");

        Assert.IsTrue(cacheTime < dbTime);  // Cache should be faster
        Assert.IsTrue(cacheTime < 10);  // Cache lookup < 10ms
    }

    [TestMethod]
    public void ProviderLookup_Registry_PerformanceBenchmark()
    {
        // Arrange
        var registry = new ChannelRegistry(
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<ChannelRegistry>>().Object);

        // Register 100 providers
        for (int i = 0; i < 100; i++)
        {
            var mockProvider = new Mock<IChannelProvider>();
            mockProvider.Setup(p => p.ProviderName).Returns($"provider-{i}");
            mockProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });
            registry.RegisterProvider(mockProvider.Object);
        }

        // Act - 1000 lookups
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            registry.GetProvider("email", $"provider-{i % 100}");
        }
        stopwatch.Stop();

        // Assert
        Console.WriteLine($"1000 provider lookups: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Average per lookup: {stopwatch.ElapsedMilliseconds / 1000.0:F2}ms");

        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);  // < 100ms total
    }
}
```

---

## Concurrency Tests

**File:** `ConcurrencyTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class ConcurrencyTests
{
    [TestMethod]
    public void ProviderRegistration_Concurrent_ThreadSafe()
    {
        // Arrange
        var registry = new ChannelRegistry(
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<ChannelRegistry>>().Object);

        var providers = Enumerable.Range(0, 100).Select(i =>
        {
            var mock = new Mock<IChannelProvider>();
            mock.Setup(p => p.ProviderName).Returns($"provider-{i}");
            mock.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });
            return mock.Object;
        }).ToList();

        // Act - Register concurrently
        Parallel.ForEach(providers, provider =>
        {
            registry.RegisterProvider(provider);
        });

        // Assert - All registered
        var emailProviders = registry.GetProvidersByProtocol("email").ToList();
        Assert.AreEqual(100, emailProviders.Count);
    }

    [TestMethod]
    public async Task ChannelLookup_ConcurrentReads_ThreadSafe()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CommunicationsDbContext>()
            .UseInMemoryDatabase("ConcurrencyDb")
            .Options;

        var dbContext = new CommunicationsDbContext(options);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = new Mock<ILogger<ChannelRepository>>().Object;
        var repository = new ChannelRepository(dbContext, cache, logger);

        var channel = ChannelFactory.CreateEmailChannel("concurrent-test", "sendgrid", new Dictionary<string, object>());
        await repository.CreateAsync(channel);

        // Act - 100 concurrent reads
        var tasks = Enumerable.Range(0, 100).Select(_ => Task.Run(async () =>
        {
            return await repository.GetByNameAsync("concurrent-test");
        }));

        var results = await Task.WhenAll(tasks);

        // Assert - All reads successful
        Assert.AreEqual(100, results.Length);
        Assert.IsTrue(results.All(r => r != null));
        Assert.IsTrue(results.All(r => r.Name == "concurrent-test"));
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| Channel | 90% | Constructor validation, configuration |
| ChannelFactory | 85% | All factory methods |
| ChannelRegistry | 90% | Register, lookup, thread safety |
| ChannelRepository | 85% | CRUD operations, caching |
| Channel Providers | 80% | CanSend, Send, configuration validation |

---

## Continuous Integration

### CI Pipeline Tests

**Run on every commit:**
```bash
# Unit tests (fast)
dotnet test --filter "TestCategory=Unit"
```

**Run daily:**
```bash
# Integration tests (database)
dotnet test --filter "TestCategory=Integration"

# Performance tests
dotnet test --filter "TestCategory=DevLocal"
```

---

## Test Maintenance

### Adding New Tests

**When adding new features:**
1. Add unit tests for new methods
2. Add integration tests for end-to-end scenarios
3. Update coverage goals if needed
4. Document new test patterns

**Test naming convention:**
```
[MethodName]_[Scenario]_[ExpectedBehavior]

Examples:
- CreateEmailChannel_ValidParameters_CreatesChannel
- GetProvider_ProviderNotFound_ReturnsNull
- RegisterProvider_Concurrent_ThreadSafe
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 2 Overview](../README-REVISED.md)
