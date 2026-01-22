# Account Management - Architecture

**Epic:** 07 - Identity & Session Management
**Feature:** Account Management
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Account Management system implements a layered architecture using the **Repository Pattern**, **Service Layer Pattern**, and **Provider Pattern** for extensible authentication and Azure AD B2C integration.

```
┌─────────────────────────────────────────────────────────────────┐
│                      API / Application Layer                    │
│                  (Controllers, SignalR Hubs)                    │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────────┐
│                    IAccountService (Service Layer)              │
│  ┌────────────────┬──────────────────┬────────────────────┐    │
│  │ CreateAccount  │ ChangePassword   │ InitiateMfaSetup   │    │
│  │ UpdateAccount  │ PasswordReset    │ LinkProvider       │    │
│  │ Deactivate     │ EmailVerification│ SearchAccounts     │    │
│  └────────────────┴──────────────────┴────────────────────┘    │
└────────────────────┬────────────────────────────────────────────┘
                     │
         ┌───────────┼───────────┬─────────────┬──────────────┐
         ↓           ↓           ↓             ↓              ↓
┌─────────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐
│IAccountRepo │ │IPassword│ │  IMfa    │ │IEmail    │ │IManageGraph  │
│             │ │Hasher   │ │Service   │ │Service   │ │User (Azure)  │
│- GetById    │ │         │ │          │ │          │ │              │
│- GetByEmail │ │- Hash   │ │- Generate│ │- SendVeri│ │- CreateUser  │
│- Save       │ │- Verify │ │  Secret  │ │  fication│ │- UpdateUser  │
│- Search     │ │         │ │- VerifyOtp│ │- SendReset│ │- DeleteUser  │
└─────────────┘ └─────────┘ └──────────┘ └──────────┘ └──────────────┘
       │
       ↓
┌─────────────────────────────────────────────────────────────────┐
│                   Data Layer (Database)                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │  Accounts   │  │  Password   │  │   External  │            │
│  │    Table    │  │   History   │  │  Providers  │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Core Components

### 1. AccountService (Main Service)

**Responsibilities:**
- Account lifecycle management (CRUD)
- Password management (change, reset)
- Email verification
- MFA setup and management
- External provider linking
- Azure AD B2C synchronization

**Key Design Decisions:**
- **Async operations** - All I/O operations async
- **Transaction management** - Database operations wrapped in transactions
- **Event publishing** - Publishes domain events (AccountCreated, PasswordChanged, etc.)
- **Validation** - Business rule validation before persistence

**Implementation Pattern:**
```csharp
public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMfaService _mfaService;
    private readonly IEmailService _emailService;
    private readonly IManageGraphUser _graphUser;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AccountService> _logger;
    private readonly AccountOptions _options;

    public async Task<Account> CreateAccountAsync(
        CreateAccountRequest request,
        CancellationToken ct = default)
    {
        // 1. Validate uniqueness
        if (await _repository.ExistsByEmailAsync(request.Email, ct))
            throw new DuplicateAccountException($"Account with email '{request.Email}' already exists");

        // 2. Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // 3. Create account entity
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            Username = request.Username?.ToLowerInvariant(),
            DisplayName = request.DisplayName,
            Status = AccountStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Save to database
        await _repository.SaveAsync(account, passwordHash, ct);

        // 5. Sync to Azure AD B2C (if enabled)
        if (_options.AzureAdB2CEnabled)
        {
            try
            {
                var azureId = await _graphUser.CreateUserAsync(new GraphUserCreate
                {
                    Email = account.Email,
                    DisplayName = account.DisplayName,
                    AccountEnabled = false  // Disabled until verified
                }, ct);

                account.AzureAdB2CId = azureId;
                await _repository.UpdateAsync(account, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Azure AD B2C user for account {AccountId}", account.Id);
                // Continue - Azure sync failure should not block account creation
            }
        }

        // 6. Send verification email
        var verificationToken = await GenerateVerificationTokenAsync(account.Id, ct);
        await _emailService.SendVerificationEmailAsync(account.Email, verificationToken, ct);

        // 7. Publish event
        await _eventPublisher.PublishAsync(new AccountCreatedEvent(account.Id, account.Email), ct);

        _logger.LogInformation("Account created: {AccountId}, Email: {Email}", account.Id, account.Email);

        return account;
    }

    public async Task ChangePasswordAsync(
        Guid accountId,
        string currentPassword,
        string newPassword,
        CancellationToken ct = default)
    {
        // 1. Get account with current password hash
        var (account, currentHash) = await _repository.GetWithPasswordAsync(accountId, ct);
        if (account == null)
            throw new AccountNotFoundException($"Account {accountId} not found");

        // 2. Verify current password
        if (!_passwordHasher.VerifyPassword(currentPassword, currentHash))
            throw new InvalidPasswordException("Current password is incorrect");

        // 3. Validate new password
        ValidatePasswordComplexity(newPassword);

        // 4. Check password history
        var history = await _repository.GetPasswordHistoryAsync(accountId, _options.PasswordHistorySize, ct);
        foreach (var historicalHash in history)
        {
            if (_passwordHasher.VerifyPassword(newPassword, historicalHash))
                throw new PasswordReuseException($"Cannot reuse password from last {_options.PasswordHistorySize} passwords");
        }

        // 5. Hash new password
        var newHash = _passwordHasher.HashPassword(newPassword);

        // 6. Save new password and add to history
        await _repository.UpdatePasswordAsync(accountId, newHash, ct);
        await _repository.AddPasswordHistoryAsync(accountId, newHash, ct);

        // 7. Update Azure AD B2C (if synced)
        if (!string.IsNullOrEmpty(account.AzureAdB2CId))
        {
            try
            {
                await _graphUser.UpdateUserAsync(account.AzureAdB2CId, new GraphUserUpdate
                {
                    PasswordProfile = new PasswordProfile
                    {
                        Password = newPassword,
                        ForceChangePasswordNextSignIn = false
                    }
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Azure AD B2C password for account {AccountId}", accountId);
            }
        }

        // 8. Publish event
        await _eventPublisher.PublishAsync(new PasswordChangedEvent(accountId), ct);

        _logger.LogInformation("Password changed for account {AccountId}", accountId);
    }

    private void ValidatePasswordComplexity(string password)
    {
        if (password.Length < _options.MinPasswordLength)
            throw new PasswordComplexityException($"Password must be at least {_options.MinPasswordLength} characters");

        if (password.Length > _options.MaxPasswordLength)
            throw new PasswordComplexityException($"Password must not exceed {_options.MaxPasswordLength} characters");

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        var complexityCount = (hasUpper ? 1 : 0) + (hasLower ? 1 : 0) + (hasDigit ? 1 : 0) + (hasSpecial ? 1 : 0);

        if (complexityCount < _options.RequiredComplexityTypes)
            throw new PasswordComplexityException($"Password must contain at least {_options.RequiredComplexityTypes} of: uppercase, lowercase, digit, special character");
    }
}
```

---

### 2. AccountRepository (Data Access)

**Responsibilities:**
- Account CRUD operations
- Password storage and retrieval
- Password history management
- Account search with criteria
- Transaction management

**Key Design Decisions:**
- **Interface-based** - IAccountRepository abstraction
- **Provider-agnostic** - Works with SQL, MongoDB, etc.
- **Optimized queries** - Indexes on email, username, status
- **Soft deletes** - Deactivated accounts retained

**Implementation Pattern:**
```csharp
public class AccountRepository : IAccountRepository
{
    private readonly DbContext _dbContext;
    private readonly IDistributedCache _cache;

    public async Task<Account?> GetByIdAsync(Guid accountId, CancellationToken ct = default)
    {
        // 1. Check cache first
        var cacheKey = $"account:{accountId}";
        var cached = await _cache.GetAsync<Account>(cacheKey, ct);
        if (cached != null)
            return cached;

        // 2. Query database
        var account = await _dbContext.Accounts
            .Include(a => a.LinkedProviders)
            .FirstOrDefaultAsync(a => a.Id == accountId, ct);

        // 3. Cache result
        if (account != null)
        {
            await _cache.SetAsync(cacheKey, account, TimeSpan.FromMinutes(15), ct);
        }

        return account;
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant();

        return await _dbContext.Accounts
            .Include(a => a.LinkedProviders)
            .FirstOrDefaultAsync(a => a.Email == normalizedEmail, ct);
    }

    public async Task<PagedResult<Account>> SearchAsync(
        AccountSearchCriteria criteria,
        CancellationToken ct = default)
    {
        var query = _dbContext.Accounts.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(criteria.Email))
            query = query.Where(a => a.Email.Contains(criteria.Email.ToLowerInvariant()));

        if (!string.IsNullOrEmpty(criteria.Username))
            query = query.Where(a => a.Username != null && a.Username.Contains(criteria.Username.ToLowerInvariant()));

        if (criteria.Status.HasValue)
            query = query.Where(a => a.Status == criteria.Status.Value);

        if (criteria.CreatedAfter.HasValue)
            query = query.Where(a => a.CreatedAt >= criteria.CreatedAfter.Value);

        if (criteria.CreatedBefore.HasValue)
            query = query.Where(a => a.CreatedAt <= criteria.CreatedBefore.Value);

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync(ct);

        return new PagedResult<Account>
        {
            Items = items,
            TotalCount = totalCount,
            PageSize = criteria.Take,
            PageNumber = (criteria.Skip / criteria.Take) + 1
        };
    }

    public async Task SaveAsync(Account account, string passwordHash, CancellationToken ct = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            // Insert account
            _dbContext.Accounts.Add(account);
            await _dbContext.SaveChangesAsync(ct);

            // Insert password
            _dbContext.AccountPasswords.Add(new AccountPassword
            {
                AccountId = account.Id,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            // Invalidate cache
            await _cache.RemoveAsync($"account:{account.Id}", ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
```

---

### 3. PasswordHasher (Security)

**Responsibilities:**
- Password hashing with bcrypt
- Password verification
- Work factor configuration

**Implementation:**
```csharp
public class BcryptPasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public BcryptPasswordHasher(IOptions<PasswordHasherOptions> options)
    {
        _workFactor = options.Value.WorkFactor; // Default: 12
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            return false; // Invalid hash format
        }
    }
}
```

---

### 4. MfaService (Multi-Factor Authentication)

**Responsibilities:**
- TOTP secret generation
- QR code generation
- TOTP verification
- Backup code generation

**Implementation:**
```csharp
public class MfaService : IMfaService
{
    public MfaSetupResult GenerateSetup(string accountEmail)
    {
        // 1. Generate secret (Base32 encoded)
        var secret = GenerateSecret();

        // 2. Generate QR code URI
        var issuer = "OoBDev";
        var qrUri = $"otpauth://totp/{issuer}:{accountEmail}?secret={secret}&issuer={issuer}";

        // 3. Generate backup codes
        var backupCodes = GenerateBackupCodes(10);

        return new MfaSetupResult
        {
            Secret = secret,
            QrCodeUri = qrUri,
            BackupCodes = backupCodes
        };
    }

    public bool VerifyTotp(string secret, string code)
    {
        // TOTP verification with time window (±1 time step)
        var totp = new Totp(Base32Encoding.ToBytes(secret));

        var currentTime = DateTime.UtcNow;

        // Check current time step
        if (totp.ComputeTotp(currentTime) == code)
            return true;

        // Check previous time step (30 seconds ago)
        if (totp.ComputeTotp(currentTime.AddSeconds(-30)) == code)
            return true;

        // Check next time step (30 seconds ahead)
        if (totp.ComputeTotp(currentTime.AddSeconds(30)) == code)
            return true;

        return false;
    }

    private string GenerateSecret()
    {
        var bytes = new byte[20]; // 160 bits
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Base32Encoding.ToString(bytes);
    }

    private string[] GenerateBackupCodes(int count)
    {
        var codes = new string[count];
        for (int i = 0; i < count; i++)
        {
            codes[i] = GenerateBackupCode();
        }
        return codes;
    }

    private string GenerateBackupCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var code = new char[8];
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[8];
        rng.GetBytes(bytes);

        for (int i = 0; i < 8; i++)
        {
            code[i] = chars[bytes[i] % chars.Length];
        }

        return new string(code);
    }
}
```

---

### 5. AzureAdB2CGraphService (IManageGraphUser)

**Responsibilities:**
- Create users in Azure AD B2C
- Update user attributes
- Sync account status
- Map custom claims

**Implementation:**
```csharp
public class AzureAdB2CGraphService : IManageGraphUser
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<AzureAdB2CGraphService> _logger;

    public async Task<string> CreateUserAsync(GraphUserCreate request, CancellationToken ct = default)
    {
        var user = new User
        {
            DisplayName = request.DisplayName,
            Identities = new List<ObjectIdentity>
            {
                new ObjectIdentity
                {
                    SignInType = "emailAddress",
                    Issuer = "contoso.onmicrosoft.com",
                    IssuerAssignedId = request.Email
                }
            },
            PasswordProfile = new PasswordProfile
            {
                Password = GenerateRandomPassword(),
                ForceChangePasswordNextSignIn = false
            },
            AccountEnabled = request.AccountEnabled
        };

        var created = await _graphClient.Users
            .Request()
            .AddAsync(user, ct);

        _logger.LogInformation("Created Azure AD B2C user: {AzureId}, Email: {Email}", created.Id, request.Email);

        return created.Id;
    }

    public async Task UpdateUserAsync(string azureId, GraphUserUpdate request, CancellationToken ct = default)
    {
        var user = new User
        {
            DisplayName = request.DisplayName,
            AccountEnabled = request.AccountEnabled
        };

        if (request.PasswordProfile != null)
        {
            user.PasswordProfile = request.PasswordProfile;
        }

        await _graphClient.Users[azureId]
            .Request()
            .UpdateAsync(user, ct);

        _logger.LogInformation("Updated Azure AD B2C user: {AzureId}", azureId);
    }

    private string GenerateRandomPassword()
    {
        // Generate complex random password for Azure AD
        const int length = 16;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        var password = new char[length];
        for (int i = 0; i < length; i++)
        {
            password[i] = chars[bytes[i] % chars.Length];
        }

        return new string(password);
    }
}
```

---

## Data Flow

### Sequence: Create Account with Azure AD B2C Sync

```
┌────────┐      ┌──────────────┐      ┌──────────────┐      ┌──────────┐      ┌─────────┐
│ Client │      │AccountService│      │IAccountRepo  │      │IManageGrp│      │IEmail   │
└───┬────┘      └──────┬───────┘      └──────┬───────┘      └────┬─────┘      └────┬────┘
    │                  │                     │                   │                 │
    │ CreateAccount    │                     │                   │                 │
    ├─────────────────>│                     │                   │                 │
    │                  │                     │                   │                 │
    │                  │ ValidateUniqueness  │                   │                 │
    │                  ├────────────────────>│                   │                 │
    │                  │                     │                   │                 │
    │                  │ Email unique        │                   │                 │
    │                  │<────────────────────┤                   │                 │
    │                  │                     │                   │                 │
    │                  │ HashPassword        │                   │                 │
    │                  │ (bcrypt work=12)    │                   │                 │
    │                  │                     │                   │                 │
    │                  │ SaveAccount         │                   │                 │
    │                  ├────────────────────>│                   │                 │
    │                  │                     │                   │                 │
    │                  │ Account saved       │                   │                 │
    │                  │<────────────────────┤                   │                 │
    │                  │                     │                   │                 │
    │                  │ CreateAzureUser     │                   │                 │
    │                  ├────────────────────────────────────────>│                 │
    │                  │                     │                   │                 │
    │                  │                     │                   │ Graph API       │
    │                  │                     │                   │ (HTTP POST)     │
    │                  │                     │                   │                 │
    │                  │ AzureId             │                   │                 │
    │                  │<────────────────────────────────────────┤                 │
    │                  │                     │                   │                 │
    │                  │ UpdateAzureId       │                   │                 │
    │                  ├────────────────────>│                   │                 │
    │                  │                     │                   │                 │
    │                  │ SendVerificationEmail                   │                 │
    │                  ├─────────────────────────────────────────────────────────>│
    │                  │                     │                   │                 │
    │                  │                     │                   │                 │ Email
    │                  │                     │                   │                 │ Queued
    │                  │                     │                   │                 │
    │ Account          │                     │                   │                 │
    │<─────────────────┤                     │                   │                 │
    │                  │                     │                   │                 │
```

---

## Design Patterns

### 1. Repository Pattern
- Abstracts data access logic
- Enables testability (mock repositories)
- Supports multiple storage backends

### 2. Service Layer Pattern
- Business logic encapsulation
- Transaction management
- Event publishing

### 3. Provider Pattern
- IManageGraphUser for Azure AD B2C
- IEmailService for notifications
- IMfaService for authentication

### 4. Strategy Pattern
- IPasswordHasher (bcrypt, PBKDF2, Argon2)
- ITokenGenerator (GUID, cryptographic)

---

## Performance Optimizations

### 1. Caching
- Account lookup by ID cached (15 minutes)
- Email-to-AccountId mapping cached (5 minutes)
- Distributed cache (Redis) for multi-instance deployments

### 2. Database Indexes
```sql
CREATE INDEX IX_Accounts_Email ON Accounts(Email);
CREATE INDEX IX_Accounts_Username ON Accounts(Username);
CREATE INDEX IX_Accounts_Status ON Accounts(Status);
CREATE INDEX IX_Accounts_CreatedAt ON Accounts(CreatedAt);
```

### 3. Async Operations
- All I/O operations async
- Email sending queued (not blocking)
- Azure AD sync non-blocking (failures logged)

### 4. Connection Pooling
- Database connection pooling enabled
- HttpClient reuse for Graph API calls

---

## Security Considerations

### 1. Password Storage
- bcrypt with work factor 12 (256ms per hash)
- Password history encrypted
- No plaintext password storage

### 2. Token Generation
- Cryptographically secure random tokens (256-bit)
- Token expiration enforced
- One-time use for verification/reset tokens

### 3. Rate Limiting
- Password attempts: 5 per hour per account
- Password reset requests: 3 per hour per email
- MFA code attempts: 5 per hour per account

### 4. Audit Logging
- All account changes logged
- Failed login attempts logged
- Password changes logged (not hash)

---

## Error Handling

### Exception Hierarchy
```csharp
public class AccountException : Exception { }

public class DuplicateAccountException : AccountException { }
public class AccountNotFoundException : AccountException { }
public class InvalidPasswordException : AccountException { }
public class PasswordComplexityException : AccountException { }
public class PasswordReuseException : AccountException { }
public class EmailVerificationException : AccountException { }
public class MfaException : AccountException { }
```

---

## Testing Strategy

### Unit Tests
- Service logic with mocked repositories
- Password validation rules
- MFA TOTP verification
- Token generation and validation

### Integration Tests
- End-to-end account creation
- Password reset workflow
- Email verification workflow
- Azure AD B2C synchronization (with test tenant)

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
