# Account Management - API Design

**Epic:** 07 - Identity & Session Management
**Feature:** Account Management
**Last Updated:** 2026-01-22

---

## API Overview

The Account Management API provides interfaces for account lifecycle management, password operations, MFA setup, and Azure AD B2C integration.

---

## Core Interfaces

### IAccountService

**Purpose:** Main service for account management operations.

```csharp
namespace OoBDev.System.Identity;

/// <summary>
/// Account management service for CRUD operations, password management, and MFA.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Creates new user account.
    /// </summary>
    /// <param name="request">Account creation details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created account</returns>
    /// <exception cref="DuplicateAccountException">Email or username already exists</exception>
    /// <exception cref="PasswordComplexityException">Password does not meet requirements</exception>
    Task<Account> CreateAccountAsync(CreateAccountRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates account information.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="request">Update details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Updated account</returns>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="DuplicateAccountException">Email or username already exists</exception>
    Task<Account> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deactivates account with reason.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="reason">Deactivation reason</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    Task DeactivateAccountAsync(Guid accountId, DeactivationReason reason, CancellationToken ct = default);

    /// <summary>
    /// Reactivates previously deactivated account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="InvalidOperationException">Account cannot be reactivated</exception>
    Task ReactivateAccountAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Gets account by ID.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Account or null if not found</returns>
    Task<Account?> GetAccountAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Gets account by email (case-insensitive).
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Account or null if not found</returns>
    Task<Account?> GetAccountByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets account by username (case-insensitive).
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Account or null if not found</returns>
    Task<Account?> GetAccountByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>
    /// Searches accounts with criteria.
    /// </summary>
    /// <param name="criteria">Search criteria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated results</returns>
    Task<PagedResult<Account>> SearchAccountsAsync(AccountSearchCriteria criteria, CancellationToken ct = default);

    /// <summary>
    /// Changes account password (requires current password).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="currentPassword">Current password</param>
    /// <param name="newPassword">New password</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="InvalidPasswordException">Current password incorrect</exception>
    /// <exception cref="PasswordComplexityException">New password does not meet requirements</exception>
    /// <exception cref="PasswordReuseException">Password recently used</exception>
    Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword, CancellationToken ct = default);

    /// <summary>
    /// Initiates password reset workflow.
    /// </summary>
    /// <param name="email">Account email</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Password reset token (sent via email)</returns>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    Task<PasswordResetToken> InitiatePasswordResetAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Completes password reset with token.
    /// </summary>
    /// <param name="token">Reset token</param>
    /// <param name="newPassword">New password</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="InvalidTokenException">Token invalid or expired</exception>
    /// <exception cref="PasswordComplexityException">Password does not meet requirements</exception>
    Task CompletePasswordResetAsync(string token, string newPassword, CancellationToken ct = default);

    /// <summary>
    /// Initiates MFA setup for account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>MFA setup result with QR code and backup codes</returns>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    Task<MfaSetupResult> InitiateMfaSetupAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Completes MFA setup with verification code.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="verificationCode">6-digit TOTP code</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="MfaException">Verification code invalid</exception>
    Task CompleteMfaSetupAsync(Guid accountId, string verificationCode, CancellationToken ct = default);

    /// <summary>
    /// Disables MFA for account (requires password).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="password">Account password</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="InvalidPasswordException">Password incorrect</exception>
    Task DisableMfaAsync(Guid accountId, string password, CancellationToken ct = default);

    /// <summary>
    /// Links external OAuth provider to account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="provider">Provider name (e.g., "google", "microsoft")</param>
    /// <param name="externalUserId">External user ID from provider</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="DuplicateLinkException">Provider already linked to this or another account</exception>
    Task LinkExternalProviderAsync(Guid accountId, string provider, string externalUserId, CancellationToken ct = default);

    /// <summary>
    /// Unlinks external OAuth provider from account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="provider">Provider name</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    Task UnlinkExternalProviderAsync(Guid accountId, string provider, CancellationToken ct = default);

    /// <summary>
    /// Gets linked external providers for account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Linked providers</returns>
    Task<IEnumerable<ExternalProvider>> GetLinkedProvidersAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Verifies email with verification token.
    /// </summary>
    /// <param name="token">Verification token</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="InvalidTokenException">Token invalid or expired</exception>
    Task VerifyEmailAsync(string token, CancellationToken ct = default);
}
```

---

### IManageGraphUser

**Purpose:** Azure AD B2C integration via Microsoft Graph API.

```csharp
namespace OoBDev.System.Identity.AzureAd;

/// <summary>
/// Azure AD B2C user management via Microsoft Graph API.
/// </summary>
public interface IManageGraphUser
{
    /// <summary>
    /// Creates user in Azure AD B2C.
    /// </summary>
    /// <param name="request">User creation details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Azure AD user ID</returns>
    /// <exception cref="GraphServiceException">Graph API error</exception>
    Task<string> CreateUserAsync(GraphUserCreate request, CancellationToken ct = default);

    /// <summary>
    /// Updates user in Azure AD B2C.
    /// </summary>
    /// <param name="azureId">Azure AD user ID</param>
    /// <param name="request">Update details</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="GraphServiceException">Graph API error</exception>
    Task UpdateUserAsync(string azureId, GraphUserUpdate request, CancellationToken ct = default);

    /// <summary>
    /// Deletes user from Azure AD B2C.
    /// </summary>
    /// <param name="azureId">Azure AD user ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="GraphServiceException">Graph API error</exception>
    Task DeleteUserAsync(string azureId, CancellationToken ct = default);

    /// <summary>
    /// Gets user from Azure AD B2C.
    /// </summary>
    /// <param name="azureId">Azure AD user ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Graph user or null if not found</returns>
    Task<GraphUser?> GetUserAsync(string azureId, CancellationToken ct = default);

    /// <summary>
    /// Gets user by email from Azure AD B2C.
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Graph user or null if not found</returns>
    Task<GraphUser?> GetUserByEmailAsync(string email, CancellationToken ct = default);
}
```

---

## Models

### Account
```csharp
namespace OoBDev.System.Identity;

public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }

    public AccountStatus Status { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool MfaEnabled { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public DeactivationReason? DeactivationReason { get; set; }

    public string? AzureAdB2CId { get; set; }
    public ICollection<ExternalProvider> LinkedProviders { get; set; } = new List<ExternalProvider>();
}

public enum AccountStatus
{
    PendingVerification,
    Active,
    Suspended,
    Deactivated,
    Deleted
}

public enum DeactivationReason
{
    UserRequested,
    Terminated,
    Suspended,
    PolicyViolation,
    Duplicate,
    Other
}
```

### Request Models
```csharp
public class CreateAccountRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class UpdateAccountRequest
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class AccountSearchCriteria
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    public AccountStatus? Status { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }

    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
}
```

### MFA Models
```csharp
public class MfaSetupResult
{
    public string Secret { get; set; } = "";
    public string QrCodeUri { get; set; } = "";
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
}
```

---

## Dependency Injection

### Service Registration
```csharp
namespace OoBDev.System.Identity.Extensions;

public static class AccountServiceExtensions
{
    /// <summary>
    /// Adds account management services to DI container.
    /// </summary>
    public static IServiceCollection AddAccountManagement(
        this IServiceCollection services,
        Action<AccountOptions>? configure = null)
    {
        // Configuration
        if (configure != null)
        {
            services.Configure(configure);
        }

        // Core services
        services.TryAddScoped<IAccountService, AccountService>();
        services.TryAddScoped<IAccountRepository, AccountRepository>();

        // Security
        services.TryAddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.TryAddSingleton<IMfaService, MfaService>();
        services.TryAddSingleton<ITokenGenerator, CryptographicTokenGenerator>();

        // Azure AD B2C (optional)
        services.TryAddScoped<IManageGraphUser, AzureAdB2CGraphService>();

        return services;
    }

    /// <summary>
    /// Adds Azure AD B2C integration.
    /// </summary>
    public static IServiceCollection AddAzureAdB2CIntegration(
        this IServiceCollection services,
        Action<AzureAdB2COptions> configure)
    {
        services.Configure(configure);
        services.TryAddScoped<IManageGraphUser, AzureAdB2CGraphService>();

        return services;
    }
}
```

---

## Usage Examples

### Example 1: Create Account

```csharp
using OoBDev.System.Identity;

public class RegistrationController : ControllerBase
{
    private readonly IAccountService _accountService;

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        try
        {
            var account = await _accountService.CreateAccountAsync(new CreateAccountRequest
            {
                Email = request.Email,
                Password = request.Password,
                DisplayName = request.DisplayName
            });

            return Ok(new
            {
                accountId = account.Id,
                email = account.Email,
                status = account.Status.ToString(),
                message = "Verification email sent. Please check your inbox."
            });
        }
        catch (DuplicateAccountException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (PasswordComplexityException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

---

### Example 2: Change Password

```csharp
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    [HttpPost("{accountId}/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePasswordAsync(
        Guid accountId,
        [FromBody] ChangePasswordRequest request)
    {
        // Verify user is changing their own password
        var currentUserId = GetCurrentUserId();
        if (currentUserId != accountId)
            return Forbid();

        try
        {
            await _accountService.ChangePasswordAsync(
                accountId,
                request.CurrentPassword,
                request.NewPassword);

            return Ok(new { message = "Password changed successfully" });
        }
        catch (InvalidPasswordException)
        {
            return BadRequest(new { error = "Current password is incorrect" });
        }
        catch (PasswordComplexityException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (PasswordReuseException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
```

---

### Example 3: Password Reset Workflow

```csharp
// Step 1: Initiate reset
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
{
    try
    {
        var resetToken = await _accountService.InitiatePasswordResetAsync(request.Email);

        // Email sent asynchronously - don't reveal if account exists
        return Ok(new
        {
            message = "If an account exists with this email, a password reset link has been sent."
        });
    }
    catch
    {
        // Return success even if account not found (security best practice)
        return Ok(new
        {
            message = "If an account exists with this email, a password reset link has been sent."
        });
    }
}

// Step 2: Complete reset
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
{
    try
    {
        await _accountService.CompletePasswordResetAsync(request.Token, request.NewPassword);

        return Ok(new { message = "Password reset successfully" });
    }
    catch (InvalidTokenException)
    {
        return BadRequest(new { error = "Reset token is invalid or expired" });
    }
    catch (PasswordComplexityException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

---

### Example 4: MFA Setup

```csharp
// Step 1: Initiate MFA setup
[HttpPost("{accountId}/mfa/setup")]
[Authorize]
public async Task<IActionResult> SetupMfaAsync(Guid accountId)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId != accountId)
        return Forbid();

    var setup = await _accountService.InitiateMfaSetupAsync(accountId);

    return Ok(new
    {
        qrCodeUri = setup.QrCodeUri,
        backupCodes = setup.BackupCodes,
        instructions = "Scan QR code with authenticator app and enter verification code"
    });
}

// Step 2: Complete MFA setup
[HttpPost("{accountId}/mfa/verify")]
[Authorize]
public async Task<IActionResult> VerifyMfaAsync(
    Guid accountId,
    [FromBody] VerifyMfaRequest request)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId != accountId)
        return Forbid();

    try
    {
        await _accountService.CompleteMfaSetupAsync(accountId, request.Code);

        return Ok(new { message = "MFA enabled successfully" });
    }
    catch (MfaException)
    {
        return BadRequest(new { error = "Verification code is invalid" });
    }
}

// Disable MFA
[HttpDelete("{accountId}/mfa")]
[Authorize]
public async Task<IActionResult> DisableMfaAsync(
    Guid accountId,
    [FromBody] DisableMfaRequest request)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId != accountId)
        return Forbid();

    try
    {
        await _accountService.DisableMfaAsync(accountId, request.Password);

        return Ok(new { message = "MFA disabled successfully" });
    }
    catch (InvalidPasswordException)
    {
        return BadRequest(new { error = "Password is incorrect" });
    }
}
```

---

### Example 5: Link OAuth Provider

```csharp
[HttpPost("{accountId}/link/{provider}")]
[Authorize]
public async Task<IActionResult> LinkProviderAsync(
    Guid accountId,
    string provider,
    [FromBody] LinkProviderRequest request)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId != accountId)
        return Forbid();

    try
    {
        await _accountService.LinkExternalProviderAsync(
            accountId,
            provider,
            request.ExternalUserId);

        return Ok(new { message = $"{provider} linked successfully" });
    }
    catch (DuplicateLinkException ex)
    {
        return Conflict(new { error = ex.Message });
    }
}

[HttpGet("{accountId}/linked-providers")]
[Authorize]
public async Task<IActionResult> GetLinkedProvidersAsync(Guid accountId)
{
    var currentUserId = GetCurrentUserId();
    if (currentUserId != accountId)
        return Forbid();

    var providers = await _accountService.GetLinkedProvidersAsync(accountId);

    return Ok(providers.Select(p => new
    {
        provider = p.Provider,
        linkedAt = p.LinkedAt
    }));
}
```

---

### Example 6: Search Accounts (Admin)

```csharp
[HttpGet("accounts/search")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> SearchAccountsAsync([FromQuery] AccountSearchRequest request)
{
    var criteria = new AccountSearchCriteria
    {
        Email = request.Email,
        Username = request.Username,
        Status = request.Status,
        CreatedAfter = request.CreatedAfter,
        CreatedBefore = request.CreatedBefore,
        Skip = request.Page * request.PageSize,
        Take = request.PageSize
    };

    var result = await _accountService.SearchAccountsAsync(criteria);

    return Ok(new
    {
        accounts = result.Items.Select(a => new
        {
            id = a.Id,
            email = a.Email,
            username = a.Username,
            status = a.Status.ToString(),
            createdAt = a.CreatedAt,
            lastLoginAt = a.LastLoginAt
        }),
        totalCount = result.TotalCount,
        pageNumber = result.PageNumber,
        pageSize = result.PageSize,
        totalPages = (int)Math.Ceiling((double)result.TotalCount / result.PageSize)
    });
}
```

---

### Example 7: Deactivate Account (Admin)

```csharp
[HttpPost("{accountId}/deactivate")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeactivateAccountAsync(
    Guid accountId,
    [FromBody] DeactivateAccountRequest request)
{
    try
    {
        await _accountService.DeactivateAccountAsync(accountId, request.Reason);

        return Ok(new { message = "Account deactivated successfully" });
    }
    catch (AccountNotFoundException)
    {
        return NotFound(new { error = "Account not found" });
    }
}

[HttpPost("{accountId}/reactivate")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ReactivateAccountAsync(Guid accountId)
{
    try
    {
        await _accountService.ReactivateAccountAsync(accountId);

        return Ok(new { message = "Account reactivated successfully" });
    }
    catch (AccountNotFoundException)
    {
        return NotFound(new { error = "Account not found" });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

---

### Example 8: Azure AD B2C Sync

```csharp
public class AzureAdSyncService
{
    private readonly IAccountService _accountService;
    private readonly IManageGraphUser _graphUser;
    private readonly ILogger<AzureAdSyncService> _logger;

    public async Task SyncAccountToAzureAsync(Guid accountId)
    {
        var account = await _accountService.GetAccountAsync(accountId);
        if (account == null)
            throw new AccountNotFoundException($"Account {accountId} not found");

        // Create or update in Azure AD B2C
        if (string.IsNullOrEmpty(account.AzureAdB2CId))
        {
            // Create new user
            var azureId = await _graphUser.CreateUserAsync(new GraphUserCreate
            {
                Email = account.Email,
                DisplayName = account.DisplayName,
                AccountEnabled = account.Status == AccountStatus.Active
            });

            _logger.LogInformation("Created Azure AD user {AzureId} for account {AccountId}", azureId, accountId);
        }
        else
        {
            // Update existing user
            await _graphUser.UpdateUserAsync(account.AzureAdB2CId, new GraphUserUpdate
            {
                DisplayName = account.DisplayName,
                AccountEnabled = account.Status == AccountStatus.Active
            });

            _logger.LogInformation("Updated Azure AD user {AzureId} for account {AccountId}", account.AzureAdB2CId, accountId);
        }
    }
}
```

---

### Example 9: Configuration

```csharp
// Startup.cs or Program.cs
services.AddAccountManagement(options =>
{
    // Password policy
    options.MinPasswordLength = 12;
    options.MaxPasswordLength = 128;
    options.RequiredComplexityTypes = 3; // Uppercase, lowercase, digit, special
    options.PasswordHistorySize = 5;

    // Token expiration
    options.VerificationTokenExpiration = TimeSpan.FromHours(24);
    options.PasswordResetTokenExpiration = TimeSpan.FromHours(24);

    // Rate limiting
    options.MaxPasswordAttemptsPerHour = 5;
    options.MaxResetRequestsPerHour = 3;

    // Azure AD B2C
    options.AzureAdB2CEnabled = true;
    options.AzureAdB2CTenantId = "contoso.onmicrosoft.com";
});

// Azure AD B2C configuration
services.AddAzureAdB2CIntegration(options =>
{
    options.ClientId = configuration["AzureAdB2C:ClientId"];
    options.ClientSecret = configuration["AzureAdB2C:ClientSecret"];
    options.TenantId = configuration["AzureAdB2C:TenantId"];
});
```

---

## Error Handling

### Exception Types
```csharp
namespace OoBDev.System.Identity;

public class AccountException : Exception
{
    public AccountException(string message) : base(message) { }
    public AccountException(string message, Exception inner) : base(message, inner) { }
}

public class DuplicateAccountException : AccountException
{
    public DuplicateAccountException(string message) : base(message) { }
}

public class AccountNotFoundException : AccountException
{
    public AccountNotFoundException(string message) : base(message) { }
}

public class InvalidPasswordException : AccountException
{
    public InvalidPasswordException(string message) : base(message) { }
}

public class PasswordComplexityException : AccountException
{
    public PasswordComplexityException(string message) : base(message) { }
}

public class PasswordReuseException : AccountException
{
    public PasswordReuseException(string message) : base(message) { }
}

public class InvalidTokenException : AccountException
{
    public InvalidTokenException(string message) : base(message) { }
}

public class MfaException : AccountException
{
    public MfaException(string message) : base(message) { }
}

public class DuplicateLinkException : AccountException
{
    public DuplicateLinkException(string message) : base(message) { }
}
```

---

## Best Practices

### 1. Always Use CancellationToken
```csharp
// ✅ GOOD
await _accountService.CreateAccountAsync(request, cancellationToken);

// ❌ BAD
await _accountService.CreateAccountAsync(request);
```

### 2. Validate Input Before Service Call
```csharp
// ✅ GOOD
if (string.IsNullOrWhiteSpace(request.Email))
    return BadRequest("Email is required");

await _accountService.CreateAccountAsync(request);

// ❌ BAD
await _accountService.CreateAccountAsync(request); // Service throws exception
```

### 3. Use Strongly-Typed Exceptions
```csharp
// ✅ GOOD
catch (DuplicateAccountException ex)
{
    return Conflict(ex.Message);
}
catch (PasswordComplexityException ex)
{
    return BadRequest(ex.Message);
}

// ❌ BAD
catch (Exception ex)
{
    return BadRequest(ex.Message); // Too generic
}
```

### 4. Don't Reveal Account Existence in Public APIs
```csharp
// ✅ GOOD (password reset)
return Ok("If an account exists, a reset email has been sent");

// ❌ BAD
if (account == null)
    return NotFound("Account not found"); // Reveals existence
```

---

## Performance Considerations

### Caching Strategy
```csharp
// Account lookup by ID cached (15 minutes)
var account = await _accountService.GetAccountAsync(accountId);

// Account lookup by email NOT cached (freshness required)
var account = await _accountService.GetAccountByEmailAsync(email);
```

### Async Email Sending
```csharp
// Email sent asynchronously (non-blocking)
await _accountService.CreateAccountAsync(request);
// Returns immediately, email queued

// NOT waiting for email delivery
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
