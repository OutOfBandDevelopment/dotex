# Session Management - API Design

**Epic:** 07 - Identity & Session Management
**Feature:** Session Management
**Last Updated:** 2026-01-22

---

## API Overview

The Session Management API provides interfaces for creating, validating, refreshing, and revoking sessions with JWT-based authentication and distributed session storage.

---

## Core Interfaces

### ISessionService

**Purpose:** Main service for session lifecycle management.

```csharp
namespace OoBDev.System.Identity;

/// <summary>
/// Session management service for authentication and authorization.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Creates new authenticated session.
    /// </summary>
    /// <param name="request">Session creation request</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Session result with access and refresh tokens</returns>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="ConcurrentSessionLimitException">Too many active sessions</exception>
    Task<SessionResult> CreateSessionAsync(CreateSessionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Validates access token.
    /// </summary>
    /// <param name="accessToken">JWT access token</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Validation result with principal if valid</returns>
    Task<ValidateSessionResult> ValidateSessionAsync(string accessToken, CancellationToken ct = default);

    /// <summary>
    /// Refreshes session with refresh token.
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>New access token and optionally rotated refresh token</returns>
    /// <exception cref="InvalidRefreshTokenException">Token invalid or expired</exception>
    Task<RefreshResult> RefreshSessionAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// Revokes session (sign out).
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="SessionNotFoundException">Session not found</exception>
    Task RevokeSessionAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Revokes all sessions for account (sign out everywhere).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    Task RevokeAllSessionsAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Gets session by ID.
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Session or null if not found</returns>
    Task<Session?> GetSessionAsync(Guid sessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets all sessions for account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>All sessions (active and revoked)</returns>
    Task<IEnumerable<Session>> GetAccountSessionsAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Cleans up expired sessions (background task).
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    Task CleanupExpiredSessionsAsync(CancellationToken ct = default);
}
```

---

### ITokenService

**Purpose:** JWT token generation and validation.

```csharp
namespace OoBDev.System.Identity;

/// <summary>
/// Token generation and validation service.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates JWT access token.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="claims">Additional claims</param>
    /// <returns>JWT token string</returns>
    string GenerateAccessToken(Guid accountId, IEnumerable<Claim> claims);

    /// <summary>
    /// Generates cryptographically secure refresh token.
    /// </summary>
    /// <returns>Base64-encoded refresh token</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates and decodes JWT access token.
    /// </summary>
    /// <param name="token">JWT token string</param>
    /// <returns>Claims principal or null if invalid</returns>
    ClaimsPrincipal? ValidateAccessToken(string token);
}
```

---

## Models

### Session
```csharp
namespace OoBDev.System.Identity;

public class Session
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }

    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";

    public DateTime CreatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }

    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }

    public DeviceInfo Device { get; set; } = new();
    public bool RememberMe { get; set; }
}

public class DeviceInfo
{
    public string UserAgent { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string DeviceType { get; set; } = ""; // Mobile, Desktop, Tablet
    public string Browser { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public string Location { get; set; } = ""; // Optional: City, Country
}
```

### Request/Response Models
```csharp
public class CreateSessionRequest
{
    public Guid AccountId { get; set; }
    public IEnumerable<Claim> Claims { get; set; } = Enumerable.Empty<Claim>();
    public string UserAgent { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class SessionResult
{
    public Guid SessionId { get; set; }
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public int ExpiresIn { get; set; } // Seconds
}

public class ValidateSessionResult
{
    public bool IsValid { get; set; }
    public Guid SessionId { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public string? Reason { get; set; }
}

public class RefreshResult
{
    public string AccessToken { get; set; } = "";
    public string RefreshToken { get; set; } = "";
    public int ExpiresIn { get; set; }
}
```

---

## Dependency Injection

### Service Registration
```csharp
namespace OoBDev.System.Identity.Extensions;

public static class SessionServiceExtensions
{
    /// <summary>
    /// Adds session management services.
    /// </summary>
    public static IServiceCollection AddSessionManagement(
        this IServiceCollection services,
        Action<SessionOptions>? configure = null)
    {
        // Configuration
        if (configure != null)
        {
            services.Configure(configure);
        }

        // Core services
        services.TryAddScoped<ISessionService, SessionService>();
        services.TryAddSingleton<ITokenService, JwtTokenService>();
        services.TryAddSingleton<IDeviceParser, UAParserDeviceParser>();

        // Repositories
        services.TryAddScoped<ISessionRepository, SessionRepository>();

        // Background service for cleanup
        services.AddHostedService<SessionCleanupBackgroundService>();

        return services;
    }

    /// <summary>
    /// Adds JWT authentication middleware.
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        Action<JwtOptions> configure)
    {
        services.Configure(configure);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = new JwtOptions();
                configure(jwtOptions);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
```

---

## Usage Examples

### Example 1: Sign In (Create Session)

```csharp
using OoBDev.System.Identity;

public class AuthenticationController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly IAccountService _accountService;

    [HttpPost("signin")]
    public async Task<IActionResult> SignInAsync([FromBody] SignInRequest request)
    {
        // 1. Validate credentials (via IAccountService or authentication service)
        var account = await ValidateCredentialsAsync(request.Email, request.Password);
        if (account == null)
        {
            return Unauthorized(new { error = "Invalid credentials" });
        }

        // 2. Get claims for user
        var claims = await GetUserClaimsAsync(account.Id);

        // 3. Create session
        var session = await _sessionService.CreateSessionAsync(new CreateSessionRequest
        {
            AccountId = account.Id,
            Claims = claims,
            UserAgent = Request.Headers["User-Agent"].ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            RememberMe = request.RememberMe
        });

        // 4. Set refresh token in HttpOnly cookie
        Response.Cookies.Append("refresh_token", session.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(request.RememberMe ? 90 : 30)
        });

        return Ok(new
        {
            accessToken = session.AccessToken,
            expiresIn = session.ExpiresIn,
            tokenType = "Bearer"
        });
    }

    private async Task<IEnumerable<Claim>> GetUserClaimsAsync(Guid accountId)
    {
        // Get from IClaimsService
        return new[]
        {
            new Claim("email", "user@example.com"),
            new Claim("role", "User")
        };
    }
}
```

---

### Example 2: Validate Session (Middleware)

```csharp
public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        // 1. Extract access token from Authorization header
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // 2. Validate session
            var result = await sessionService.ValidateSessionAsync(token);

            if (result.IsValid && result.Principal != null)
            {
                // 3. Set user principal
                context.User = result.Principal;
            }
        }

        await _next(context);
    }
}

// Register middleware
app.UseMiddleware<SessionValidationMiddleware>();
```

---

### Example 3: Refresh Token

```csharp
[HttpPost("refresh")]
public async Task<IActionResult> RefreshAsync()
{
    // 1. Get refresh token from cookie
    if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
    {
        return Unauthorized(new { error = "Refresh token not found" });
    }

    try
    {
        // 2. Refresh session
        var result = await _sessionService.RefreshSessionAsync(refreshToken);

        // 3. Update refresh token cookie if rotated
        if (result.RefreshToken != refreshToken)
        {
            Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }

        return Ok(new
        {
            accessToken = result.AccessToken,
            expiresIn = result.ExpiresIn,
            tokenType = "Bearer"
        });
    }
    catch (InvalidRefreshTokenException ex)
    {
        // Remove invalid cookie
        Response.Cookies.Delete("refresh_token");

        return Unauthorized(new { error = ex.Message });
    }
}
```

---

### Example 4: Sign Out (Revoke Session)

```csharp
[HttpPost("signout")]
[Authorize]
public async Task<IActionResult> SignOutAsync()
{
    // 1. Get session ID from claims
    var sessionIdClaim = User.FindFirst("sid");
    if (sessionIdClaim == null || !Guid.TryParse(sessionIdClaim.Value, out var sessionId))
    {
        return BadRequest(new { error = "Invalid session" });
    }

    // 2. Revoke session
    await _sessionService.RevokeSessionAsync(sessionId);

    // 3. Remove refresh token cookie
    Response.Cookies.Delete("refresh_token");

    return Ok(new { message = "Signed out successfully" });
}

[HttpPost("signout-everywhere")]
[Authorize]
public async Task<IActionResult> SignOutEverywhereAsync()
{
    // 1. Get account ID from claims
    var accountIdClaim = User.FindFirst("sub");
    if (accountIdClaim == null || !Guid.TryParse(accountIdClaim.Value, out var accountId))
    {
        return BadRequest(new { error = "Invalid account" });
    }

    // 2. Revoke all sessions
    await _sessionService.RevokeAllSessionsAsync(accountId);

    // 3. Remove refresh token cookie
    Response.Cookies.Delete("refresh_token");

    return Ok(new { message = "Signed out from all devices" });
}
```

---

### Example 5: List Active Sessions

```csharp
[HttpGet("sessions")]
[Authorize]
public async Task<IActionResult> GetSessionsAsync()
{
    // 1. Get account ID from claims
    var accountIdClaim = User.FindFirst("sub");
    if (accountIdClaim == null || !Guid.TryParse(accountIdClaim.Value, out var accountId))
    {
        return BadRequest(new { error = "Invalid account" });
    }

    // 2. Get current session ID
    var currentSessionIdClaim = User.FindFirst("sid");
    var currentSessionId = currentSessionIdClaim != null && Guid.TryParse(currentSessionIdClaim.Value, out var sid)
        ? sid
        : (Guid?)null;

    // 3. Get all sessions
    var sessions = await _sessionService.GetAccountSessionsAsync(accountId);

    // 4. Filter active sessions and format
    var activeSessions = sessions
        .Where(s => !s.IsRevoked)
        .OrderByDescending(s => s.LastActivityAt)
        .Select(s => new
        {
            sessionId = s.Id,
            device = new
            {
                type = s.Device.DeviceType,
                browser = s.Device.Browser,
                os = s.Device.OperatingSystem,
                ipAddress = s.Device.IpAddress
            },
            createdAt = s.CreatedAt,
            lastActivityAt = s.LastActivityAt,
            isCurrent = s.Id == currentSessionId
        });

    return Ok(activeSessions);
}
```

---

### Example 6: Revoke Specific Session

```csharp
[HttpDelete("sessions/{sessionId}")]
[Authorize]
public async Task<IActionResult> RevokeSessionAsync(Guid sessionId)
{
    // 1. Get account ID from claims
    var accountIdClaim = User.FindFirst("sub");
    if (accountIdClaim == null || !Guid.TryParse(accountIdClaim.Value, out var accountId))
    {
        return BadRequest(new { error = "Invalid account" });
    }

    // 2. Get session to verify ownership
    var session = await _sessionService.GetSessionAsync(sessionId);
    if (session == null)
    {
        return NotFound(new { error = "Session not found" });
    }

    // 3. Verify user owns session (or is admin)
    if (session.AccountId != accountId && !User.IsInRole("Admin"))
    {
        return Forbid();
    }

    // 4. Revoke session
    await _sessionService.RevokeSessionAsync(sessionId);

    return Ok(new { message = "Session revoked successfully" });
}
```

---

### Example 7: Configuration

```csharp
// Program.cs or Startup.cs
services.AddSessionManagement(options =>
{
    options.MaxConcurrentSessions = 5;
    options.AccessTokenLifetime = TimeSpan.FromMinutes(15);
    options.RefreshTokenLifetime = TimeSpan.FromDays(30);
    options.RememberMeRefreshTokenLifetime = TimeSpan.FromDays(90);
    options.RotateRefreshTokens = true;
    options.AbsoluteSessionTimeout = TimeSpan.FromDays(7);
    options.IdleTimeout = TimeSpan.FromHours(2);
});

services.AddJwtAuthentication(options =>
{
    options.Secret = configuration["Jwt:Secret"];
    options.Issuer = configuration["Jwt:Issuer"];
    options.Audience = configuration["Jwt:Audience"];
    options.AccessTokenLifetime = TimeSpan.FromMinutes(15);
});

// Add distributed cache (Redis)
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration["Redis:ConnectionString"];
});
```

---

### Example 8: Background Session Cleanup

```csharp
public class SessionCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SessionCleanupBackgroundService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var sessionService = scope.ServiceProvider.GetRequiredService<ISessionService>();

                await sessionService.CleanupExpiredSessionsAsync(stoppingToken);

                _logger.LogDebug("Session cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session cleanup failed");
            }

            // Run cleanup every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("Session cleanup service stopped");
    }
}
```

---

## Error Handling

### Exception Types
```csharp
namespace OoBDev.System.Identity;

public class SessionException : Exception
{
    public SessionException(string message) : base(message) { }
}

public class SessionNotFoundException : SessionException
{
    public SessionNotFoundException(string message) : base(message) { }
}

public class InvalidRefreshTokenException : SessionException
{
    public InvalidRefreshTokenException(string message) : base(message) { }
}

public class ConcurrentSessionLimitException : SessionException
{
    public ConcurrentSessionLimitException(string message) : base(message) { }
}
```

---

## Best Practices

### 1. Secure Token Storage
```csharp
// ✅ GOOD - HttpOnly, Secure cookies
Response.Cookies.Append("refresh_token", token, new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict
});

// ❌ BAD - Exposed in JavaScript
return Ok(new { refreshToken = token });
```

### 2. Short-Lived Access Tokens
```csharp
// ✅ GOOD - 15 minute expiration
options.AccessTokenLifetime = TimeSpan.FromMinutes(15);

// ❌ BAD - Long expiration
options.AccessTokenLifetime = TimeSpan.FromDays(7);
```

### 3. Token Rotation
```csharp
// ✅ GOOD - Rotate refresh tokens
options.RotateRefreshTokens = true;

// ❌ BAD - Reuse refresh tokens
options.RotateRefreshTokens = false;
```

---

## Performance Considerations

### Caching Strategy
```csharp
// Session validation cached for 5 minutes
var result = await _sessionService.ValidateSessionAsync(token);

// Account sessions cached for 5 minutes
var sessions = await _sessionService.GetAccountSessionsAsync(accountId);
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
