# Session Management - Architecture

**Epic:** 07 - Identity & Session Management
**Feature:** Session Management
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Session Management system implements a distributed session architecture with JWT-based access tokens, refresh tokens, device tracking, and multi-layer caching for high performance and scalability.

```
┌─────────────────────────────────────────────────────────────────┐
│                      API / Application Layer                    │
│              (Controllers, Authentication Middleware)           │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────────┐
│                    ISessionService (Service Layer)              │
│  ┌────────────────┬──────────────────┬────────────────────┐    │
│  │ CreateSession  │ ValidateSession  │ RefreshSession     │    │
│  │ RevokeSession  │ GetSessions      │ CleanupExpired     │    │
│  └────────────────┴──────────────────┴────────────────────┘    │
└────────────────────┬────────────────────────────────────────────┘
                     │
         ┌───────────┼───────────┬─────────────┬──────────────┐
         ↓           ↓           ↓             ↓              ↓
┌─────────────┐ ┌─────────┐ ┌──────────┐ ┌──────────────┐ ┌─────────┐
│ITokenService│ │ICache   │ │ISessionRp│ │IDevice       │ │IEventPub│
│             │ │(Redis)  │ │          │ │  Parser      │ │         │
│- GenerateJWT│ │         │ │- Save    │ │              │ │- Publish│
│- ValidateJWT│ │- Get/Set│ │- GetById │ │- ParseUA     │ │  Events │
│- GenRefresh │ │- Remove │ │- GetByAcc│ │- GetIP       │ │         │
└─────────────┘ └─────────┘ └──────────┘ └──────────────┘ └─────────┘
                                  │
                                  ↓
                    ┌─────────────────────────┐
                    │  Database (Persistence) │
                    │  ┌─────────────────┐    │
                    │  │    Sessions     │    │
                    │  └─────────────────┘    │
                    └─────────────────────────┘
```

---

## Core Components

### 1. SessionService (Main Service)

**Responsibilities:**
- Session lifecycle management
- Token generation and validation
- Session revocation
- Concurrent session enforcement
- Device tracking

**Key Design Decisions:**
- **Cache-first** - Redis for hot session data
- **Database fallback** - Persistence and recovery
- **JWT stateless** - Access tokens validated without DB lookup
- **Refresh tokens stateful** - Stored and validated against cache/DB

**Implementation Pattern:**
```csharp
public class SessionService : ISessionService
{
    private readonly ITokenService _tokenService;
    private readonly ISessionRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly IDeviceParser _deviceParser;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SessionService> _logger;
    private readonly SessionOptions _options;

    public async Task<SessionResult> CreateSessionAsync(
        CreateSessionRequest request,
        CancellationToken ct = default)
    {
        // 1. Enforce concurrent session limit
        var existingSessions = await GetAccountSessionsAsync(request.AccountId, ct);
        var activeSessions = existingSessions.Where(s => !s.IsRevoked).ToList();

        if (activeSessions.Count >= _options.MaxConcurrentSessions)
        {
            // Terminate oldest session
            var oldestSession = activeSessions
                .OrderBy(s => s.LastActivityAt)
                .First();

            await RevokeSessionAsync(oldestSession.Id, ct);

            _logger.LogInformation(
                "Terminated oldest session {SessionId} due to concurrent session limit",
                oldestSession.Id);
        }

        // 2. Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(
            request.AccountId,
            request.Claims);

        var refreshToken = _tokenService.GenerateRefreshToken();

        // 3. Parse device information
        var deviceInfo = _deviceParser.Parse(request.UserAgent, request.IpAddress);

        // 4. Create session
        var session = new Session
        {
            Id = Guid.NewGuid(),
            AccountId = request.AccountId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            AccessTokenExpiresAt = DateTime.UtcNow.Add(_options.AccessTokenLifetime),
            RefreshTokenExpiresAt = DateTime.UtcNow.Add(
                request.RememberMe
                    ? _options.RememberMeRefreshTokenLifetime
                    : _options.RefreshTokenLifetime),
            Device = deviceInfo,
            RememberMe = request.RememberMe,
            IsRevoked = false
        };

        // 5. Save to database
        await _repository.SaveAsync(session, ct);

        // 6. Cache session
        await CacheSessionAsync(session, ct);

        // 7. Publish event
        await _eventPublisher.PublishAsync(
            new SessionCreatedEvent(session.Id, request.AccountId, deviceInfo),
            ct);

        _logger.LogInformation(
            "Session created: {SessionId}, Account: {AccountId}, Device: {DeviceType}",
            session.Id, request.AccountId, deviceInfo.DeviceType);

        return new SessionResult
        {
            SessionId = session.Id,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = (int)_options.AccessTokenLifetime.TotalSeconds
        };
    }

    public async Task<ValidateSessionResult> ValidateSessionAsync(
        string accessToken,
        CancellationToken ct = default)
    {
        // 1. Validate JWT (signature, expiration)
        var principal = _tokenService.ValidateAccessToken(accessToken);
        if (principal == null)
        {
            return new ValidateSessionResult { IsValid = false, Reason = "Invalid token" };
        }

        // 2. Extract session ID and account ID
        var sessionIdClaim = principal.FindFirst("sid");
        var accountIdClaim = principal.FindFirst("sub");

        if (sessionIdClaim == null || accountIdClaim == null)
        {
            return new ValidateSessionResult { IsValid = false, Reason = "Missing claims" };
        }

        if (!Guid.TryParse(sessionIdClaim.Value, out var sessionId))
        {
            return new ValidateSessionResult { IsValid = false, Reason = "Invalid session ID" };
        }

        // 3. Check if session revoked (cache-first)
        var cacheKey = $"session:{sessionId}";
        var cachedSession = await _cache.GetAsync<Session>(cacheKey, ct);

        if (cachedSession?.IsRevoked == true)
        {
            return new ValidateSessionResult { IsValid = false, Reason = "Session revoked" };
        }

        // 4. Update last activity (async, non-blocking)
        _ = UpdateLastActivityAsync(sessionId, ct);

        return new ValidateSessionResult
        {
            IsValid = true,
            SessionId = sessionId,
            Principal = principal
        };
    }

    public async Task<RefreshResult> RefreshSessionAsync(
        string refreshToken,
        CancellationToken ct = default)
    {
        // 1. Find session by refresh token
        var session = await _repository.GetByRefreshTokenAsync(refreshToken, ct);
        if (session == null)
        {
            throw new InvalidRefreshTokenException("Refresh token not found");
        }

        // 2. Validate not revoked
        if (session.IsRevoked)
        {
            throw new InvalidRefreshTokenException("Session revoked");
        }

        // 3. Validate not expired
        if (session.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException("Refresh token expired");
        }

        // 4. Generate new access token
        var claims = await GetSessionClaimsAsync(session.AccountId, ct);
        var newAccessToken = _tokenService.GenerateAccessToken(session.AccountId, claims);

        // 5. Optionally rotate refresh token
        var newRefreshToken = refreshToken;
        if (_options.RotateRefreshTokens)
        {
            newRefreshToken = _tokenService.GenerateRefreshToken();
            session.RefreshToken = newRefreshToken;
        }

        // 6. Update session
        session.AccessToken = newAccessToken;
        session.AccessTokenExpiresAt = DateTime.UtcNow.Add(_options.AccessTokenLifetime);
        session.LastActivityAt = DateTime.UtcNow;

        await _repository.UpdateAsync(session, ct);

        // 7. Update cache
        await CacheSessionAsync(session, ct);

        _logger.LogInformation(
            "Session refreshed: {SessionId}, Account: {AccountId}",
            session.Id, session.AccountId);

        return new RefreshResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = (int)_options.AccessTokenLifetime.TotalSeconds
        };
    }

    public async Task RevokeSessionAsync(Guid sessionId, CancellationToken ct = default)
    {
        // 1. Get session
        var session = await GetSessionAsync(sessionId, ct);
        if (session == null)
            throw new SessionNotFoundException($"Session {sessionId} not found");

        // 2. Mark as revoked
        session.IsRevoked = true;
        session.RevokedAt = DateTime.UtcNow;

        // 3. Update database
        await _repository.UpdateAsync(session, ct);

        // 4. Invalidate cache
        await _cache.RemoveAsync($"session:{sessionId}", ct);

        // 5. Publish event
        await _eventPublisher.PublishAsync(
            new SessionRevokedEvent(sessionId, session.AccountId),
            ct);

        _logger.LogInformation(
            "Session revoked: {SessionId}, Account: {AccountId}",
            sessionId, session.AccountId);
    }

    public async Task RevokeAllSessionsAsync(Guid accountId, CancellationToken ct = default)
    {
        // 1. Get all active sessions
        var sessions = await GetAccountSessionsAsync(accountId, ct);
        var activeSessions = sessions.Where(s => !s.IsRevoked).ToList();

        // 2. Revoke each session
        foreach (var session in activeSessions)
        {
            await RevokeSessionAsync(session.Id, ct);
        }

        _logger.LogInformation(
            "All sessions revoked for account {AccountId}, Count: {Count}",
            accountId, activeSessions.Count);
    }

    public async Task<IEnumerable<Session>> GetAccountSessionsAsync(
        Guid accountId,
        CancellationToken ct = default)
    {
        // 1. Check cache first
        var cacheKey = $"account-sessions:{accountId}";
        var cached = await _cache.GetAsync<IEnumerable<Session>>(cacheKey, ct);
        if (cached != null)
            return cached;

        // 2. Query database
        var sessions = await _repository.GetByAccountIdAsync(accountId, ct);

        // 3. Cache result
        await _cache.SetAsync(cacheKey, sessions, TimeSpan.FromMinutes(5), ct);

        return sessions;
    }

    private async Task CacheSessionAsync(Session session, CancellationToken ct)
    {
        var cacheKey = $"session:{session.Id}";
        var ttl = session.RefreshTokenExpiresAt - DateTime.UtcNow;

        await _cache.SetAsync(cacheKey, session, ttl, ct);
    }

    private async Task UpdateLastActivityAsync(Guid sessionId, CancellationToken ct)
    {
        try
        {
            var session = await GetSessionAsync(sessionId, ct);
            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
                await _repository.UpdateAsync(session, ct);
                await CacheSessionAsync(session, ct);
            }
        }
        catch (Exception ex)
        {
            // Non-critical - log and continue
            _logger.LogWarning(ex, "Failed to update last activity for session {SessionId}", sessionId);
        }
    }

    private async Task<IEnumerable<Claim>> GetSessionClaimsAsync(Guid accountId, CancellationToken ct)
    {
        // Get claims from account service or claims service
        // This would integrate with IClaimsService from Role & Claims Management
        return new[]
        {
            new Claim("sub", accountId.ToString()),
            new Claim("email", "user@example.com") // Get from account
        };
    }
}
```

---

### 2. TokenService (JWT Management)

**Responsibilities:**
- JWT access token generation
- Refresh token generation
- Token validation
- Signature verification

**Implementation Pattern:**
```csharp
public class TokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly ILogger<TokenService> _logger;

    public string GenerateAccessToken(Guid accountId, IEnumerable<Claim> claims)
    {
        var allClaims = new List<Claim>(claims)
        {
            new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: allClaims,
            expires: DateTime.UtcNow.Add(_options.AccessTokenLifetime),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));

        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero // No clock skew tolerance
            }, out _);

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Token validation failed");
            return null;
        }
    }
}
```

---

### 3. DeviceParser (Device Information)

**Responsibilities:**
- Parse User-Agent header
- Extract device type, browser, OS
- IP geolocation (optional)

**Implementation Pattern:**
```csharp
public class DeviceParser : IDeviceParser
{
    private readonly Parser _uaParser;

    public DeviceParser()
    {
        _uaParser = Parser.GetDefault();
    }

    public DeviceInfo Parse(string userAgent, string ipAddress)
    {
        var clientInfo = _uaParser.Parse(userAgent);

        return new DeviceInfo
        {
            UserAgent = userAgent,
            IpAddress = ipAddress,
            DeviceType = GetDeviceType(clientInfo),
            Browser = $"{clientInfo.UA.Family} {clientInfo.UA.Major}",
            OperatingSystem = $"{clientInfo.OS.Family} {clientInfo.OS.Major}",
            Location = "" // Implement IP geolocation if needed
        };
    }

    private string GetDeviceType(ClientInfo clientInfo)
    {
        if (clientInfo.Device.IsSpider)
            return "Bot";

        var family = clientInfo.Device.Family?.ToLower() ?? "";

        if (family.Contains("mobile") || family.Contains("phone"))
            return "Mobile";

        if (family.Contains("tablet") || family.Contains("ipad"))
            return "Tablet";

        return "Desktop";
    }
}
```

---

## Data Flow

### Sequence: Session Creation

```
┌────────┐      ┌──────────────┐      ┌──────────────┐      ┌──────────┐
│ Client │      │SessionService│      │ITokenService │      │ICache    │
└───┬────┘      └──────┬───────┘      └──────┬───────┘      └────┬─────┘
    │                  │                     │                   │
    │ CreateSession    │                     │                   │
    ├─────────────────>│                     │                   │
    │                  │                     │                   │
    │                  │ CheckConcurrent     │                   │
    │                  │ SessionLimit        │                   │
    │                  │                     │                   │
    │                  │ GenerateAccessToken │                   │
    │                  ├────────────────────>│                   │
    │                  │                     │                   │
    │                  │ JWT                 │                   │
    │                  │<────────────────────┤                   │
    │                  │                     │                   │
    │                  │ GenerateRefreshToken│                   │
    │                  ├────────────────────>│                   │
    │                  │                     │                   │
    │                  │ RefreshToken        │                   │
    │                  │<────────────────────┤                   │
    │                  │                     │                   │
    │                  │ SaveToDatabase      │                   │
    │                  │                     │                   │
    │                  │ CacheSession        │                   │
    │                  ├────────────────────────────────────────>│
    │                  │                     │                   │
    │ SessionResult    │                     │                   │
    │<─────────────────┤                     │                   │
    │                  │                     │                   │
```

---

## Design Patterns

### 1. Cache-Aside Pattern
- Check cache first
- Fall back to database
- Update cache on write

### 2. Token-Based Authentication
- Stateless access tokens (JWT)
- Stateful refresh tokens
- Token rotation

### 3. Strategy Pattern
- ITokenService (JWT, OAuth)
- IDeviceParser (UAParser, custom)

---

## Performance Optimizations

### 1. Multi-Layer Caching
```csharp
// Layer 1: Session validation (JWT signature + cache check)
$"session:{sessionId}" → < 5ms

// Layer 2: Account sessions list
$"account-sessions:{accountId}" → < 100ms

// Layer 3: Database fallback
Database query → < 500ms
```

### 2. Async Non-Blocking Operations
- Last activity update
- Event publishing
- Session cleanup

### 3. Database Indexes
```sql
CREATE INDEX IX_Sessions_AccountId ON Sessions(AccountId);
CREATE INDEX IX_Sessions_RefreshToken ON Sessions(RefreshToken);
CREATE INDEX IX_Sessions_AccessTokenExpiresAt ON Sessions(AccessTokenExpiresAt);
CREATE INDEX IX_Sessions_IsRevoked ON Sessions(IsRevoked);
```

---

## Security Considerations

### 1. Token Storage
- Access tokens in memory or secure cookies
- Refresh tokens in HttpOnly, Secure cookies
- Never log tokens

### 2. Token Expiration
- Short-lived access tokens (15 minutes)
- Longer refresh tokens (30 days)
- Absolute session timeout (7 days)

### 3. Revocation
- Immediate cache invalidation
- Blacklist revoked refresh tokens
- Admin override capability

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
