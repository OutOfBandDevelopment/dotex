# Account Management - Requirements

**Epic:** 07 - Identity & Session Management
**Feature:** Account Management
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~400

---

## Overview

Comprehensive account management supporting CRUD operations, password management, multi-factor authentication, account linking, and integration with Azure AD B2C via IManageGraphUser.

---

## Business Requirements

### BR-1: Account Creation
**As a** system administrator
**I want** to create user accounts with email/username authentication
**So that** users can access the system

**Acceptance Criteria:**
- Create account with email and password
- Create account with username and password
- Email verification workflow
- Validate email uniqueness
- Validate username uniqueness
- Password complexity requirements enforced
- Account status set to "PendingVerification" initially

---

### BR-2: Account Updates
**As a** user
**I want** to update my account information
**So that** my profile stays current

**Acceptance Criteria:**
- Update email (requires re-verification)
- Update username (if allowed by policy)
- Update profile information (name, phone, etc.)
- Audit trail for account changes
- Prevent duplicate email/username

---

### BR-3: Account Deactivation & Reactivation
**As a** system administrator
**I want** to deactivate and reactivate accounts
**So that** I can manage account lifecycle

**Acceptance Criteria:**
- Deactivate account with reason (terminated, suspended, deleted)
- Reactivate previously deactivated account
- Deactivated accounts cannot sign in
- Deactivation reason tracked
- Deactivation timestamp recorded

---

### BR-4: Password Management
**As a** user
**I want** to change and reset my password securely
**So that** I can maintain account security

**Acceptance Criteria:**
- Change password (requires current password)
- Initiate password reset via email
- Complete password reset with token
- Token expires after 24 hours
- Password history (prevent reuse of last 5 passwords)
- Password complexity validation
- Rate limiting on reset requests

---

### BR-5: Multi-Factor Authentication (MFA)
**As a** user
**I want** to enable multi-factor authentication
**So that** my account is more secure

**Acceptance Criteria:**
- Enable MFA with TOTP (Time-based One-Time Password)
- Generate QR code for authenticator apps
- Verify MFA setup before enabling
- Disable MFA (requires current password)
- Backup codes generated on MFA enable
- MFA status visible in account details

---

### BR-6: Account Linking (OAuth Providers)
**As a** user
**I want** to link my account with external OAuth providers
**So that** I can sign in with Google, Microsoft, etc.

**Acceptance Criteria:**
- Link account to Google OAuth
- Link account to Microsoft OAuth
- Link account to Azure AD B2C
- Unlink external provider
- Multiple providers can be linked
- Account email matches provider email (or explicitly confirmed)

---

### BR-7: Account Search & Retrieval
**As a** system administrator
**I want** to search and retrieve user accounts
**So that** I can manage users efficiently

**Acceptance Criteria:**
- Get account by ID
- Get account by email
- Get account by username
- Search accounts by criteria (status, creation date, last login)
- Paginated search results
- Filter by account status

---

### BR-8: Azure AD B2C Integration
**As a** system
**I want** to integrate with Azure AD B2C via IManageGraphUser
**So that** accounts sync with Azure AD

**Acceptance Criteria:**
- Create Azure AD B2C user via IManageGraphUser
- Update Azure AD B2C user on account changes
- Sync account status to Azure AD B2C
- Map custom claims to Azure AD attributes
- Handle Azure AD errors gracefully

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IAccountService
{
    // Create & Update
    Task<Account> CreateAccountAsync(CreateAccountRequest request, CancellationToken ct = default);
    Task<Account> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request, CancellationToken ct = default);

    // Deactivation
    Task DeactivateAccountAsync(Guid accountId, DeactivationReason reason, CancellationToken ct = default);
    Task ReactivateAccountAsync(Guid accountId, CancellationToken ct = default);

    // Retrieval
    Task<Account?> GetAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<Account?> GetAccountByEmailAsync(string email, CancellationToken ct = default);
    Task<Account?> GetAccountByUsernameAsync(string username, CancellationToken ct = default);
    Task<PagedResult<Account>> SearchAccountsAsync(AccountSearchCriteria criteria, CancellationToken ct = default);

    // Password Management
    Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task<PasswordResetToken> InitiatePasswordResetAsync(string email, CancellationToken ct = default);
    Task CompletePasswordResetAsync(string token, string newPassword, CancellationToken ct = default);

    // MFA
    Task<MfaSetupResult> InitiateMfaSetupAsync(Guid accountId, CancellationToken ct = default);
    Task CompleteMfaSetupAsync(Guid accountId, string verificationCode, CancellationToken ct = default);
    Task DisableMfaAsync(Guid accountId, string password, CancellationToken ct = default);

    // Account Linking
    Task LinkExternalProviderAsync(Guid accountId, string provider, string externalUserId, CancellationToken ct = default);
    Task UnlinkExternalProviderAsync(Guid accountId, string provider, CancellationToken ct = default);
    Task<IEnumerable<ExternalProvider>> GetLinkedProvidersAsync(Guid accountId, CancellationToken ct = default);
}

public interface IManageGraphUser
{
    // Azure AD B2C Integration
    Task<string> CreateUserAsync(GraphUserCreate request, CancellationToken ct = default);
    Task UpdateUserAsync(string azureId, GraphUserUpdate request, CancellationToken ct = default);
    Task DeleteUserAsync(string azureId, CancellationToken ct = default);
    Task<GraphUser?> GetUserAsync(string azureId, CancellationToken ct = default);
}
```

---

### TR-2: Account Model
```csharp
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

---

### TR-3: Password Requirements
- **Minimum length:** 8 characters
- **Complexity:** At least 3 of: uppercase, lowercase, digit, special character
- **History:** Cannot reuse last 5 passwords
- **Expiration:** Optional policy-based expiration (e.g., 90 days)
- **Hashing:** bcrypt with work factor 12

---

### TR-4: Email Verification Workflow
```
1. Create account → Status = PendingVerification
2. Generate verification token (GUID, expires in 24 hours)
3. Send verification email with token link
4. User clicks link → Verify token
5. Token valid → Status = Active, EmailVerified = true
6. Token invalid/expired → Show error, allow resend
```

---

### TR-5: Password Reset Workflow
```
1. User requests reset → Validate email exists
2. Generate reset token (GUID, expires in 24 hours)
3. Send reset email with token link
4. User clicks link → Verify token
5. Token valid → Allow new password entry
6. New password validated → Hash and save
7. Token invalid/expired → Show error, allow retry
```

---

### TR-6: MFA Setup Workflow
```
1. User initiates MFA setup
2. Generate secret key (Base32 encoded)
3. Generate QR code (otpauth://totp/...)
4. Generate 10 backup codes
5. User scans QR code in authenticator app
6. User enters verification code
7. Verify code matches (TOTP algorithm)
8. Code valid → MfaEnabled = true, store secret encrypted
9. Return backup codes (display once)
```

---

### TR-7: Multi-Tenancy Support
- Accounts scoped to tenant ID
- Email uniqueness per tenant
- Username uniqueness per tenant (optional)
- Tenant-specific password policies
- Tenant-specific MFA requirements

---

### TR-8: Performance Requirements
- **Account lookup by ID:** < 10ms (cached)
- **Account lookup by email:** < 50ms (indexed)
- **Password hash verification:** < 200ms (bcrypt work factor 12)
- **Password reset email:** < 5 seconds (async)
- **Search accounts:** < 100ms for 1000 results

---

## Non-Functional Requirements

### NFR-1: Security
- Passwords hashed with bcrypt (work factor 12)
- Password reset tokens cryptographically random (256-bit)
- Email verification tokens cryptographically random (256-bit)
- MFA secrets encrypted at rest (AES-256)
- Rate limiting on password attempts (5 per hour)
- Rate limiting on password reset requests (3 per hour)

### NFR-2: Scalability
- Support 100,000+ accounts
- Distributed caching for account lookups
- Async email sending (queue-based)
- Database indexes on email, username, status

### NFR-3: Auditability
- All account changes logged
- Password changes logged (hash not logged)
- Login attempts logged (success and failure)
- Account status changes logged with reason

### NFR-4: Compatibility
- Works with .NET 10.0
- Supports Azure AD B2C via Microsoft.Graph SDK
- Compatible with ASP.NET Core Identity (but not dependent)
- Works with any IUserRepository implementation

---

## Constraints

### C-1: Email Constraints
- Maximum length: 254 characters
- Must be valid email format
- Case-insensitive uniqueness check
- Normalized storage (lowercase)

### C-2: Username Constraints
- Length: 3-50 characters
- Allowed: letters, numbers, underscore, hyphen
- Case-insensitive uniqueness check
- Cannot start with number

### C-3: Password Constraints
- Length: 8-128 characters
- Complexity enforced (configurable)
- History size: 5 passwords (configurable)
- Reset token valid for 24 hours

### C-4: MFA Constraints
- TOTP algorithm (RFC 6238)
- 30-second time step
- 6-digit codes
- Backup codes: 10 per account (8 characters each)

---

## Success Criteria

- ✅ Create, update, deactivate, reactivate accounts
- ✅ Password change with current password verification
- ✅ Password reset via email with token
- ✅ Email verification workflow
- ✅ MFA setup with QR code and backup codes
- ✅ Account linking with OAuth providers (Google, Microsoft)
- ✅ Azure AD B2C integration via IManageGraphUser
- ✅ Account search with pagination
- ✅ 85%+ test coverage
- ✅ < 50ms account lookup by email (indexed)

---

## Out of Scope

- ❌ Social login UI (use external OAuth library)
- ❌ Email template design (use template service)
- ❌ SMS-based MFA (use separate SMS service)
- ❌ Biometric authentication (use device-specific APIs)
- ❌ OAuth provider implementation (use existing libraries)

---

## Dependencies

### Internal
- **OoBDev.System.Identity.Abstractions** - IAccountService, IManageGraphUser
- **OoBDev.System.Security.Cryptography** - Password hashing, token generation
- **OoBDev.System.Data** - Repository pattern

### External
- **Microsoft.Graph SDK** - Azure AD B2C integration
- **BCrypt.Net-Next** - Password hashing
- **System.Security.Cryptography** - Token generation
- **MailKit** (optional) - Email sending

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
