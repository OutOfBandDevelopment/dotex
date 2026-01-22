# Session Management - Testing Strategy

**Epic:** 07 - Identity & Session Management
**Feature:** Session Management
**Last Updated:** 2026-01-22

---

## Testing Overview

Comprehensive testing strategy covering JWT token management, session lifecycle, device tracking, concurrent session limits, and distributed caching with 85%+ coverage requirement.

---

## Test Categories

### Unit Tests (TestCategory.Unit)
- Token generation and validation
- Session service logic
- Device parsing
- Concurrent session enforcement
- Token expiration handling

**Target Coverage:** 90%+

### Integration Tests (TestCategory.Integration)
- Database operations with Docker SQL Server
- Redis caching with Docker Redis
- End-to-end session workflows
- Token refresh flows
- Session revocation propagation

**Target Coverage:** 80%+

### Performance Tests (TestCategory.Unit)
- Session validation < 5ms
- Token generation < 10ms
- Session creation < 50ms
- Cache hit ratios

**Target Coverage:** Key operations

---

## Unit Test Specifications

### 1. TokenService Tests

#### Test: GenerateAccessToken_ValidInput_ReturnsJWT
```csharp
[TestClass]
public class TokenServiceTests
{
    private TokenService _service;
    private JwtOptions _options;

    [TestInitialize]
    public void Setup()
    {
        _options = new JwtOptions
        {
            Secret = "ThisIsAVeryLongSecretKeyForHMACSHA256Signature",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenLifetime = TimeSpan.FromMinutes(15)
        };

        _service = new TokenService(
            Options.Create(_options),
            Mock.Of<ILogger<TokenService>>());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GenerateAccessToken_ValidInput_ReturnsJWT()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("email", "test@example.com"),
            new Claim("role", "User")
        };

        // Test
        var token = _service.GenerateAccessToken(accountId, claims);

        // Assert
        Assert.IsNotNull(token);
        Assert.IsTrue(token.Length > 0);

        // Verify token structure (header.payload.signature)
        var parts = token.Split('.');
        Assert.AreEqual(3, parts.Length);

        // Validate token
        var principal = _service.ValidateAccessToken(token);
        Assert.IsNotNull(principal);

        var subClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        Assert.IsNotNull(subClaim);
        Assert.AreEqual(accountId.ToString(), subClaim.Value);

        var emailClaim = principal.FindFirst("email");
        Assert.IsNotNull(emailClaim);
        Assert.AreEqual("test@example.com", emailClaim.Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ValidateAccessToken_ExpiredToken_ReturnsNull()
    {
        // Stage - Generate token with past expiration
        _options.AccessTokenLifetime = TimeSpan.FromSeconds(-10); // Negative = expired
        var service = new TokenService(Options.Create(_options), Mock.Of<ILogger<TokenService>>());

        var token = service.GenerateAccessToken(Guid.NewGuid(), Enumerable.Empty<Claim>());

        // Reset to normal lifetime for validation
        _options.AccessTokenLifetime = TimeSpan.FromMinutes(15);

        // Test
        var principal = _service.ValidateAccessToken(token);

        // Assert
        Assert.IsNull(principal);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ValidateAccessToken_InvalidSignature_ReturnsNull()
    {
        // Stage
        var token = _service.GenerateAccessToken(Guid.NewGuid(), Enumerable.Empty<Claim>());

        // Tamper with token (change signature)
        var parts = token.Split('.');
        parts[2] = "InvalidSignature";
        var tamperedToken = string.Join(".", parts);

        // Test
        var principal = _service.ValidateAccessToken(tamperedToken);

        // Assert
        Assert.IsNull(principal);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        // Test
        var token = _service.GenerateRefreshToken();

        // Assert
        Assert.IsNotNull(token);
        Assert.IsTrue(token.Length > 0);

        // Verify Base64
        try
        {
            var bytes = Convert.FromBase64String(token);
            Assert.AreEqual(32, bytes.Length); // 256 bits
        }
        catch
        {
            Assert.Fail("Token is not valid Base64");
        }
    }
}
```

---

### 2. SessionService Tests

#### Test: CreateSession_ValidRequest_CreatesSession
```csharp
[TestClass]
public class SessionServiceTests
{
    private Mock<ITokenService> _mockTokenService;
    private Mock<ISessionRepository> _mockRepository;
    private Mock<IDistributedCache> _mockCache;
    private Mock<IDeviceParser> _mockDeviceParser;
    private Mock<IEventPublisher> _mockEventPublisher;
    private SessionService _service;
    private SessionOptions _options;

    [TestInitialize]
    public void Setup()
    {
        _mockTokenService = new Mock<ITokenService>();
        _mockRepository = new Mock<ISessionRepository>();
        _mockCache = new Mock<IDistributedCache>();
        _mockDeviceParser = new Mock<IDeviceParser>();
        _mockEventPublisher = new Mock<IEventPublisher>();

        _options = new SessionOptions
        {
            MaxConcurrentSessions = 5,
            AccessTokenLifetime = TimeSpan.FromMinutes(15),
            RefreshTokenLifetime = TimeSpan.FromDays(30)
        };

        _service = new SessionService(
            _mockTokenService.Object,
            _mockRepository.Object,
            _mockCache.Object,
            _mockDeviceParser.Object,
            _mockEventPublisher.Object,
            Mock.Of<ILogger<SessionService>>(),
            Options.Create(_options));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CreateSession_ValidRequest_CreatesSession()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var request = new CreateSessionRequest
        {
            AccountId = accountId,
            Claims = new[] { new Claim("email", "test@example.com") },
            UserAgent = "Mozilla/5.0",
            IpAddress = "192.168.1.1",
            RememberMe = false
        };

        var deviceInfo = new DeviceInfo
        {
            DeviceType = "Desktop",
            Browser = "Chrome 120",
            OperatingSystem = "Windows 11"
        };

        _mockTokenService
            .Setup(t => t.GenerateAccessToken(accountId, request.Claims))
            .Returns("access_token_jwt");

        _mockTokenService
            .Setup(t => t.GenerateRefreshToken())
            .Returns("refresh_token_base64");

        _mockDeviceParser
            .Setup(p => p.Parse(request.UserAgent, request.IpAddress))
            .Returns(deviceInfo);

        _mockRepository
            .Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Session>());

        // Test
        var result = await _service.CreateSessionAsync(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(Guid.Empty, result.SessionId);
        Assert.AreEqual("access_token_jwt", result.AccessToken);
        Assert.AreEqual("refresh_token_base64", result.RefreshToken);
        Assert.AreEqual((int)_options.AccessTokenLifetime.TotalSeconds, result.ExpiresIn);

        // Verify repository save
        _mockRepository.Verify(r => r.SaveAsync(
            It.Is<Session>(s =>
                s.AccountId == accountId &&
                s.AccessToken == "access_token_jwt" &&
                s.RefreshToken == "refresh_token_base64" &&
                s.Device.DeviceType == "Desktop"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify cache set
        _mockCache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<Session>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CreateSession_ExceedsConcurrentLimit_RevokesOldest()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var request = new CreateSessionRequest
        {
            AccountId = accountId,
            UserAgent = "Mozilla/5.0",
            IpAddress = "192.168.1.1"
        };

        // Create 5 existing sessions (at limit)
        var existingSessions = Enumerable.Range(1, 5)
            .Select(i => new Session
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                LastActivityAt = DateTime.UtcNow.AddMinutes(-i),
                IsRevoked = false
            })
            .ToList();

        _mockRepository
            .Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSessions);

        _mockTokenService
            .Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<IEnumerable<Claim>>()))
            .Returns("new_token");

        _mockTokenService
            .Setup(t => t.GenerateRefreshToken())
            .Returns("new_refresh");

        _mockDeviceParser
            .Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(new DeviceInfo());

        // Test
        var result = await _service.CreateSessionAsync(request);

        // Assert - Oldest session should be revoked
        var oldestSession = existingSessions
            .OrderBy(s => s.LastActivityAt)
            .First();

        _mockRepository.Verify(r => r.UpdateAsync(
            It.Is<Session>(s => s.Id == oldestSession.Id && s.IsRevoked),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

---

#### Test: ValidateSession_ValidToken_ReturnsValid
```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
public async Task ValidateSession_ValidToken_ReturnsValid()
{
    // Stage
    var sessionId = Guid.NewGuid();
    var accountId = Guid.NewGuid();
    var token = "valid_jwt_token";

    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim("sid", sessionId.ToString()),
        new Claim("sub", accountId.ToString())
    }));

    _mockTokenService
        .Setup(t => t.ValidateAccessToken(token))
        .Returns(principal);

    _mockCache
        .Setup(c => c.GetAsync<Session>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Session?)null); // Not in cache (not revoked)

    // Test
    var result = await _service.ValidateSessionAsync(token);

    // Assert
    Assert.IsTrue(result.IsValid);
    Assert.AreEqual(sessionId, result.SessionId);
    Assert.IsNotNull(result.Principal);
}

[TestMethod]
[TestCategory(TestCategories.Unit)]
public async Task ValidateSession_RevokedSession_ReturnsInvalid()
{
    // Stage
    var sessionId = Guid.NewGuid();
    var token = "valid_jwt_token";

    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim("sid", sessionId.ToString()),
        new Claim("sub", Guid.NewGuid().ToString())
    }));

    var revokedSession = new Session
    {
        Id = sessionId,
        IsRevoked = true
    };

    _mockTokenService
        .Setup(t => t.ValidateAccessToken(token))
        .Returns(principal);

    _mockCache
        .Setup(c => c.GetAsync<Session>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(revokedSession);

    // Test
    var result = await _service.ValidateSessionAsync(token);

    // Assert
    Assert.IsFalse(result.IsValid);
    Assert.AreEqual("Session revoked", result.Reason);
}
```

---

#### Test: RefreshSession_ValidToken_ReturnsNewAccessToken
```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
public async Task RefreshSession_ValidToken_ReturnsNewAccessToken()
{
    // Stage
    var refreshToken = "valid_refresh_token";
    var session = new Session
    {
        Id = Guid.NewGuid(),
        AccountId = Guid.NewGuid(),
        RefreshToken = refreshToken,
        RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(30),
        IsRevoked = false
    };

    _mockRepository
        .Setup(r => r.GetByRefreshTokenAsync(refreshToken, It.IsAny<CancellationToken>()))
        .ReturnsAsync(session);

    _mockTokenService
        .Setup(t => t.GenerateAccessToken(session.AccountId, It.IsAny<IEnumerable<Claim>>()))
        .Returns("new_access_token");

    // Test
    var result = await _service.RefreshSessionAsync(refreshToken);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual("new_access_token", result.AccessToken);

    // Verify session updated
    _mockRepository.Verify(r => r.UpdateAsync(
        It.Is<Session>(s => s.AccessToken == "new_access_token"),
        It.IsAny<CancellationToken>()),
        Times.Once);
}
```

---

### 3. DeviceParser Tests

```csharp
[TestClass]
public class DeviceParserTests
{
    private DeviceParser _parser;

    [TestInitialize]
    public void Setup()
    {
        _parser = new DeviceParser();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Parse_DesktopChrome_ReturnsDesktop()
    {
        // Stage
        var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
        var ipAddress = "192.168.1.1";

        // Test
        var result = _parser.Parse(userAgent, ipAddress);

        // Assert
        Assert.AreEqual("Desktop", result.DeviceType);
        Assert.IsTrue(result.Browser.StartsWith("Chrome"));
        Assert.IsTrue(result.OperatingSystem.StartsWith("Windows"));
        Assert.AreEqual(ipAddress, result.IpAddress);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Parse_MobileSafari_ReturnsMobile()
    {
        // Stage
        var userAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";
        var ipAddress = "10.0.0.1";

        // Test
        var result = _parser.Parse(userAgent, ipAddress);

        // Assert
        Assert.AreEqual("Mobile", result.DeviceType);
        Assert.IsTrue(result.Browser.Contains("Safari") || result.Browser.Contains("Mobile"));
        Assert.IsTrue(result.OperatingSystem.Contains("iOS") || result.OperatingSystem.Contains("iPhone"));
    }
}
```

---

## Integration Test Specifications

### 1. Session Lifecycle End-to-End

```csharp
[TestClass]
public class SessionLifecycleIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    private ISessionService _sessionService;

    [TestInitialize]
    public async Task Setup()
    {
        var sqlConnectionString = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING");
        var redisConnectionString = TestContext.GetRequiredProperty<string>("REDIS_CONNECTION_STRING");

        var services = new ServiceCollection();
        services.AddSessionManagement();
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(sqlConnectionString));
        services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnectionString);

        var provider = services.BuildServiceProvider();
        _sessionService = provider.GetRequiredService<ISessionService>();

        // Migrate database
        var dbContext = provider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateValidateRefreshRevoke_CompleteFlow_Success()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var request = new CreateSessionRequest
        {
            AccountId = accountId,
            Claims = new[] { new Claim("email", "test@example.com") },
            UserAgent = "Mozilla/5.0 (Windows NT 10.0)",
            IpAddress = "192.168.1.1"
        };

        // Test 1: Create session
        var createResult = await _sessionService.CreateSessionAsync(request);
        Assert.IsNotNull(createResult);
        Assert.IsNotNull(createResult.AccessToken);
        Assert.IsNotNull(createResult.RefreshToken);

        // Test 2: Validate session
        var validateResult = await _sessionService.ValidateSessionAsync(createResult.AccessToken);
        Assert.IsTrue(validateResult.IsValid);
        Assert.AreEqual(createResult.SessionId, validateResult.SessionId);

        // Test 3: Refresh session
        var refreshResult = await _sessionService.RefreshSessionAsync(createResult.RefreshToken);
        Assert.IsNotNull(refreshResult);
        Assert.IsNotNull(refreshResult.AccessToken);
        Assert.AreNotEqual(createResult.AccessToken, refreshResult.AccessToken);

        // Test 4: Revoke session
        await _sessionService.RevokeSessionAsync(createResult.SessionId);

        // Test 5: Validate revoked session
        var validateRevokedResult = await _sessionService.ValidateSessionAsync(refreshResult.AccessToken);
        Assert.IsFalse(validateRevokedResult.IsValid);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Clean up test data
    }
}
```

---

## Performance Test Specifications

### 1. Session Validation Performance

```csharp
[TestClass]
public class SessionPerformanceTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateSession_CacheHit_CompletesUnder5ms()
    {
        // Stage
        var mockTokenService = new Mock<ITokenService>();
        var mockCache = new Mock<IDistributedCache>();

        var sessionId = Guid.NewGuid();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("sid", sessionId.ToString()),
            new Claim("sub", Guid.NewGuid().ToString())
        }));

        mockTokenService
            .Setup(t => t.ValidateAccessToken(It.IsAny<string>()))
            .Returns(principal);

        // Cache hit - session not revoked
        mockCache
            .Setup(c => c.GetAsync<Session>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        var service = new SessionService(
            mockTokenService.Object,
            Mock.Of<ISessionRepository>(),
            mockCache.Object,
            Mock.Of<IDeviceParser>(),
            Mock.Of<IEventPublisher>(),
            Mock.Of<ILogger<SessionService>>(),
            Options.Create(new SessionOptions()));

        // Test
        var stopwatch = Stopwatch.StartNew();
        var result = await service.ValidateSessionAsync("token");
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5,
            $"Session validation took {stopwatch.ElapsedMilliseconds}ms (expected < 5ms)");
    }
}
```

---

## Test Data Builders

### Session Test Data Builder
```csharp
public class SessionBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _accountId = Guid.NewGuid();
    private string _accessToken = "test_access_token";
    private string _refreshToken = "test_refresh_token";
    private DateTime _createdAt = DateTime.UtcNow;
    private DateTime _accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
    private DateTime _refreshTokenExpiresAt = DateTime.UtcNow.AddDays(30);
    private bool _isRevoked = false;
    private DeviceInfo _device = new() { DeviceType = "Desktop" };

    public SessionBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public SessionBuilder WithAccountId(Guid accountId)
    {
        _accountId = accountId;
        return this;
    }

    public SessionBuilder Revoked()
    {
        _isRevoked = true;
        return this;
    }

    public SessionBuilder Expired()
    {
        _accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(-10);
        return this;
    }

    public Session Build()
    {
        return new Session
        {
            Id = _id,
            AccountId = _accountId,
            AccessToken = _accessToken,
            RefreshToken = _refreshToken,
            CreatedAt = _createdAt,
            LastActivityAt = DateTime.UtcNow,
            AccessTokenExpiresAt = _accessTokenExpiresAt,
            RefreshTokenExpiresAt = _refreshTokenExpiresAt,
            IsRevoked = _isRevoked,
            Device = _device
        };
    }
}

// Usage:
var session = new SessionBuilder()
    .WithAccountId(accountId)
    .Revoked()
    .Build();
```

---

## Test Coverage Requirements

### By Component

| Component | Unit Coverage | Integration Coverage |
|-----------|---------------|---------------------|
| SessionService | 95%+ | 85%+ |
| TokenService | 95%+ | 80%+ |
| DeviceParser | 90%+ | N/A |
| SessionRepository | 80%+ | 90%+ |

### Critical Paths (100% Coverage Required)

1. Token generation and validation
2. Session revocation (cache invalidation)
3. Concurrent session limit enforcement
4. Refresh token rotation
5. Expired session handling

---

## Test Environment

### Docker Services (Integration Tests)
```yaml
# SQL Server for session storage
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest

# Redis for distributed caching
redis:
  image: redis:7-alpine
  ports:
    - "6379:6379"
```

### Test Configuration (.runsettings)
```xml
<RunSettings>
  <TestRunParameters>
    <Parameter name="SQLSERVER_CONNECTION_STRING" value="Server=localhost;Database=SessionTest;..." />
    <Parameter name="REDIS_CONNECTION_STRING" value="localhost:6379" />
  </TestRunParameters>
</RunSettings>
```

---

## Continuous Integration

### Test Execution Order

1. **Fast Unit Tests** (< 5 minutes)
   - Token service tests
   - Session service tests
   - Device parser tests

2. **Integration Tests** (< 10 minutes)
   - Database operations
   - Redis caching
   - End-to-end flows

3. **Performance Tests** (< 2 minutes)
   - Validation latency
   - Token generation

### Success Criteria

- ✅ All unit tests pass (85%+ coverage)
- ✅ All integration tests pass (80%+ coverage)
- ✅ Performance tests meet thresholds
- ✅ No critical paths uncovered

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 07 Overview](../README.md)
