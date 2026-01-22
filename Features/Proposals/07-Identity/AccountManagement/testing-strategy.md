# Account Management - Testing Strategy

**Epic:** 07 - Identity & Session Management
**Feature:** Account Management
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and security tests.

**Test Categories:**
- **Unit Tests** - Service logic with mocked dependencies
- **Integration Tests** - End-to-end workflows with real database
- **Security Tests** - Password hashing, token validation, rate limiting
- **Azure AD Tests** - Graph API integration (with test tenant)

---

## Test Pyramid

```
                ┌─────────────┐
                │  Security   │  (10 tests)
                │   Tests     │
                └─────────────┘
              ┌───────────────────┐
              │ Azure AD Tests    │  (8 tests)
              │                   │
              └───────────────────┘
            ┌─────────────────────────┐
            │  Integration Tests      │  (15 tests)
            │                         │
            └─────────────────────────┘
      ┌───────────────────────────────────┐
      │        Unit Tests                 │  (50+ tests)
      │                                   │
      └───────────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. AccountService Tests

**File:** `AccountServiceTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Identity;

namespace OoBDev.System.Identity.Tests;

[TestClass]
public class AccountServiceTests
{
    private Mock<IAccountRepository> _mockRepository;
    private Mock<IPasswordHasher> _mockPasswordHasher;
    private Mock<IMfaService> _mockMfaService;
    private Mock<IEmailService> _mockEmailService;
    private Mock<IManageGraphUser> _mockGraphUser;
    private Mock<IEventPublisher> _mockEventPublisher;
    private AccountService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IAccountRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockMfaService = new Mock<IMfaService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockGraphUser = new Mock<IManageGraphUser>();
        _mockEventPublisher = new Mock<IEventPublisher>();

        _service = new AccountService(
            _mockRepository.Object,
            _mockPasswordHasher.Object,
            _mockMfaService.Object,
            _mockEmailService.Object,
            _mockGraphUser.Object,
            _mockEventPublisher.Object,
            Options.Create(new AccountOptions()));
    }

    [TestMethod]
    public async Task CreateAccountAsync_ValidRequest_CreatesAccount()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123",
            DisplayName = "Test User"
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        _mockRepository.Setup(r => r.SaveAsync(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var account = await _service.CreateAccountAsync(request);

        // Assert
        Assert.IsNotNull(account);
        Assert.AreEqual("test@example.com", account.Email);
        Assert.AreEqual("Test User", account.DisplayName);
        Assert.AreEqual(AccountStatus.PendingVerification, account.Status);

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Account>(), "hashed_password", It.IsAny<CancellationToken>()), Times.Once);
        _mockEmailService.Verify(e => e.SendVerificationEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(p => p.PublishAsync(It.IsAny<AccountCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAccountAsync_DuplicateEmail_ThrowsDuplicateAccountException()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Email = "duplicate@example.com",
            Password = "P@ssw0rd123"
        };

        _mockRepository.Setup(r => r.ExistsByEmailAsync("duplicate@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<DuplicateAccountException>(
            () => _service.CreateAccountAsync(request));

        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Account>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_ValidCurrentPassword_ChangesPassword()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var currentPassword = "OldP@ssw0rd123";
        var newPassword = "NewP@ssw0rd456";

        var account = new Account { Id = accountId, Email = "test@example.com", Status = AccountStatus.Active };

        _mockRepository.Setup(r => r.GetWithPasswordAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((account, "old_hash"));

        _mockPasswordHasher.Setup(h => h.VerifyPassword(currentPassword, "old_hash"))
            .Returns(true);

        _mockPasswordHasher.Setup(h => h.HashPassword(newPassword))
            .Returns("new_hash");

        _mockRepository.Setup(r => r.GetPasswordHistoryAsync(accountId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        // Act
        await _service.ChangePasswordAsync(accountId, currentPassword, newPassword);

        // Assert
        _mockRepository.Verify(r => r.UpdatePasswordAsync(accountId, "new_hash", It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.AddPasswordHistoryAsync(accountId, "new_hash", It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(p => p.PublishAsync(It.IsAny<PasswordChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_InvalidCurrentPassword_ThrowsInvalidPasswordException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new Account { Id = accountId };

        _mockRepository.Setup(r => r.GetWithPasswordAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((account, "current_hash"));

        _mockPasswordHasher.Setup(h => h.VerifyPassword("WrongPassword", "current_hash"))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidPasswordException>(
            () => _service.ChangePasswordAsync(accountId, "WrongPassword", "NewP@ssw0rd"));

        _mockRepository.Verify(r => r.UpdatePasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task ChangePasswordAsync_PasswordInHistory_ThrowsPasswordReuseException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var newPassword = "P@ssw0rd123";
        var account = new Account { Id = accountId };

        _mockRepository.Setup(r => r.GetWithPasswordAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((account, "current_hash"));

        _mockPasswordHasher.Setup(h => h.VerifyPassword("CurrentPassword", "current_hash"))
            .Returns(true);

        _mockRepository.Setup(r => r.GetPasswordHistoryAsync(accountId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "hash1", "hash2", "hash3" });

        _mockPasswordHasher.Setup(h => h.VerifyPassword(newPassword, "hash2"))
            .Returns(true); // Password matches historical hash

        // Act & Assert
        await Assert.ThrowsExceptionAsync<PasswordReuseException>(
            () => _service.ChangePasswordAsync(accountId, "CurrentPassword", newPassword));
    }

    [TestMethod]
    public async Task InitiateMfaSetupAsync_ValidAccount_ReturnsSetupResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new Account { Id = accountId, Email = "test@example.com" };

        _mockRepository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var setupResult = new MfaSetupResult
        {
            Secret = "SECRET123",
            QrCodeUri = "otpauth://totp/...",
            BackupCodes = new[] { "CODE1", "CODE2" }
        };

        _mockMfaService.Setup(m => m.GenerateSetup(account.Email))
            .Returns(setupResult);

        // Act
        var result = await _service.InitiateMfaSetupAsync(accountId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("SECRET123", result.Secret);
        Assert.AreEqual(2, result.BackupCodes.Length);

        _mockRepository.Verify(r => r.StoreMfaSecretAsync(accountId, "SECRET123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task CompleteMfaSetupAsync_ValidCode_EnablesMfa()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var secret = "SECRET123";
        var code = "123456";

        _mockRepository.Setup(r => r.GetMfaSecretAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secret);

        _mockMfaService.Setup(m => m.VerifyTotp(secret, code))
            .Returns(true);

        // Act
        await _service.CompleteMfaSetupAsync(accountId, code);

        // Assert
        _mockRepository.Verify(r => r.EnableMfaAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(p => p.PublishAsync(It.IsAny<MfaEnabledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task CompleteMfaSetupAsync_InvalidCode_ThrowsMfaException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var secret = "SECRET123";
        var code = "999999";

        _mockRepository.Setup(r => r.GetMfaSecretAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secret);

        _mockMfaService.Setup(m => m.VerifyTotp(secret, code))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<MfaException>(
            () => _service.CompleteMfaSetupAsync(accountId, code));

        _mockRepository.Verify(r => r.EnableMfaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task LinkExternalProviderAsync_NewProvider_LinksSuccessfully()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var provider = "google";
        var externalUserId = "google_123";

        _mockRepository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = accountId });

        _mockRepository.Setup(r => r.IsProviderLinkedAsync(provider, externalUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _service.LinkExternalProviderAsync(accountId, provider, externalUserId);

        // Assert
        _mockRepository.Verify(r => r.LinkProviderAsync(
            accountId,
            It.Is<ExternalProvider>(p => p.Provider == provider && p.ExternalUserId == externalUserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task LinkExternalProviderAsync_AlreadyLinked_ThrowsDuplicateLinkException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var provider = "google";
        var externalUserId = "google_123";

        _mockRepository.Setup(r => r.IsProviderLinkedAsync(provider, externalUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<DuplicateLinkException>(
            () => _service.LinkExternalProviderAsync(accountId, provider, externalUserId));
    }

    [TestMethod]
    public async Task SearchAccountsAsync_WithCriteria_ReturnsPagedResults()
    {
        // Arrange
        var criteria = new AccountSearchCriteria
        {
            Email = "test",
            Status = AccountStatus.Active,
            Skip = 0,
            Take = 10
        };

        var accounts = new List<Account>
        {
            new Account { Id = Guid.NewGuid(), Email = "test1@example.com", Status = AccountStatus.Active },
            new Account { Id = Guid.NewGuid(), Email = "test2@example.com", Status = AccountStatus.Active }
        };

        var pagedResult = new PagedResult<Account>
        {
            Items = accounts,
            TotalCount = 2,
            PageSize = 10,
            PageNumber = 1
        };

        _mockRepository.Setup(r => r.SearchAsync(criteria, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _service.SearchAccountsAsync(criteria);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Items.Count);
        Assert.AreEqual(2, result.TotalCount);
    }

    [TestMethod]
    public async Task DeactivateAccountAsync_ActiveAccount_Deactivates()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var reason = DeactivationReason.UserRequested;

        _mockRepository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = accountId, Status = AccountStatus.Active });

        // Act
        await _service.DeactivateAccountAsync(accountId, reason);

        // Assert
        _mockRepository.Verify(r => r.DeactivateAsync(accountId, reason, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(p => p.PublishAsync(It.IsAny<AccountDeactivatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task ReactivateAccountAsync_DeactivatedAccount_Reactivates()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _mockRepository.Setup(r => r.GetByIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = accountId, Status = AccountStatus.Deactivated });

        // Act
        await _service.ReactivateAccountAsync(accountId);

        // Assert
        _mockRepository.Verify(r => r.ReactivateAsync(accountId, It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(p => p.PublishAsync(It.IsAny<AccountReactivatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

#### 2. Password Validation Tests

**File:** `PasswordValidationTests.cs`

```csharp
[TestClass]
public class PasswordValidationTests
{
    [TestMethod]
    [DataRow("P@ssw0rd", true)]
    [DataRow("P@ssw0rd123", true)]
    [DataRow("Passw0rd!", true)]
    [DataRow("password", false)]        // No uppercase, no special
    [DataRow("PASSWORD", false)]        // No lowercase, no digit
    [DataRow("Pass123", false)]         // Too short
    [DataRow("P@ss", false)]            // Too short
    public void ValidatePasswordComplexity_VariousPasswords_ReturnsExpected(string password, bool expected)
    {
        // Arrange
        var options = new AccountOptions
        {
            MinPasswordLength = 8,
            RequiredComplexityTypes = 3
        };

        // Act
        var isValid = PasswordValidator.ValidateComplexity(password, options);

        // Assert
        Assert.AreEqual(expected, isValid);
    }

    [TestMethod]
    public void HashPassword_SamePassword_DifferentHashes()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(Options.Create(new PasswordHasherOptions { WorkFactor = 10 }));
        var password = "P@ssw0rd123";

        // Act
        var hash1 = hasher.HashPassword(password);
        var hash2 = hasher.HashPassword(password);

        // Assert
        Assert.AreNotEqual(hash1, hash2); // bcrypt uses random salt
        Assert.IsTrue(hasher.VerifyPassword(password, hash1));
        Assert.IsTrue(hasher.VerifyPassword(password, hash2));
    }

    [TestMethod]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(Options.Create(new PasswordHasherOptions { WorkFactor = 10 }));
        var password = "P@ssw0rd123";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword(password, hash);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(Options.Create(new PasswordHasherOptions { WorkFactor = 10 }));
        var password = "P@ssw0rd123";
        var hash = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword("WrongPassword", hash);

        // Assert
        Assert.IsFalse(result);
    }
}
```

---

#### 3. MFA Service Tests

**File:** `MfaServiceTests.cs`

```csharp
[TestClass]
public class MfaServiceTests
{
    private MfaService _service;

    [TestInitialize]
    public void Setup()
    {
        _service = new MfaService();
    }

    [TestMethod]
    public void GenerateSetup_ValidEmail_ReturnsSetupResult()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result = _service.GenerateSetup(email);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(string.IsNullOrEmpty(result.Secret));
        Assert.IsTrue(result.QrCodeUri.Contains("otpauth://totp/"));
        Assert.IsTrue(result.QrCodeUri.Contains(email));
        Assert.AreEqual(10, result.BackupCodes.Length);
    }

    [TestMethod]
    public void GenerateSetup_MultipleCalls_DifferentSecrets()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result1 = _service.GenerateSetup(email);
        var result2 = _service.GenerateSetup(email);

        // Assert
        Assert.AreNotEqual(result1.Secret, result2.Secret);
    }

    [TestMethod]
    public void VerifyTotp_ValidCode_ReturnsTrue()
    {
        // Arrange
        var setup = _service.GenerateSetup("test@example.com");
        var totp = new Totp(Base32Encoding.ToBytes(setup.Secret));
        var code = totp.ComputeTotp(DateTime.UtcNow);

        // Act
        var result = _service.VerifyTotp(setup.Secret, code);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyTotp_InvalidCode_ReturnsFalse()
    {
        // Arrange
        var setup = _service.GenerateSetup("test@example.com");
        var invalidCode = "000000";

        // Act
        var result = _service.VerifyTotp(setup.Secret, invalidCode);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void VerifyTotp_CodeFromPreviousTimeStep_ReturnsTrue()
    {
        // Arrange
        var setup = _service.GenerateSetup("test@example.com");
        var totp = new Totp(Base32Encoding.ToBytes(setup.Secret));
        var previousCode = totp.ComputeTotp(DateTime.UtcNow.AddSeconds(-30));

        // Act
        var result = _service.VerifyTotp(setup.Secret, previousCode);

        // Assert
        Assert.IsTrue(result); // Should accept previous time step
    }

    [TestMethod]
    public void GenerateBackupCodes_ReturnsCorrectFormat()
    {
        // Arrange & Act
        var setup = _service.GenerateSetup("test@example.com");

        // Assert
        Assert.AreEqual(10, setup.BackupCodes.Length);
        foreach (var code in setup.BackupCodes)
        {
            Assert.AreEqual(8, code.Length);
            Assert.IsTrue(code.All(c => char.IsLetterOrDigit(c)));
        }
    }
}
```

---

## Integration Tests

### Test Scenarios

**File:** `AccountIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class AccountIntegrationTests
{
    private IAccountService _accountService;
    private IAccountRepository _repository;
    private TestContext _testContext;

    [TestInitialize]
    public void Setup()
    {
        // Use test database
        var connectionString = TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING");
        var services = new ServiceCollection();

        services.AddDbContext<AccountDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddAccountManagement();
        services.AddLogging();

        var provider = services.BuildServiceProvider();
        _accountService = provider.GetRequiredService<IAccountService>();
        _repository = provider.GetRequiredService<IAccountRepository>();
    }

    [TestMethod]
    public async Task EndToEnd_CreateAccountVerifyAndLogin_Success()
    {
        // Stage 1: Create account
        var createRequest = new CreateAccountRequest
        {
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "P@ssw0rd123",
            DisplayName = "Integration Test User"
        };

        var account = await _accountService.CreateAccountAsync(createRequest);

        Assert.IsNotNull(account);
        Assert.AreEqual(AccountStatus.PendingVerification, account.Status);

        // Stage 2: Verify email (simulate)
        // In real scenario, user clicks link with token
        await _accountService.VerifyEmailAsync(account.Id.ToString());

        var verifiedAccount = await _accountService.GetAccountAsync(account.Id);
        Assert.AreEqual(AccountStatus.Active, verifiedAccount.Status);
        Assert.IsTrue(verifiedAccount.EmailVerified);

        // Stage 3: Login simulation would happen here
        // (handled by authentication service)
    }

    [TestMethod]
    public async Task EndToEnd_PasswordResetWorkflow_Success()
    {
        // Stage 1: Create account
        var account = await CreateTestAccountAsync();

        // Stage 2: Initiate password reset
        var resetToken = await _accountService.InitiatePasswordResetAsync(account.Email);
        Assert.IsNotNull(resetToken);

        // Stage 3: Complete password reset
        var newPassword = "NewP@ssw0rd456";
        await _accountService.CompletePasswordResetAsync(resetToken.Token, newPassword);

        // Stage 4: Verify password changed
        // (verify by attempting login - handled by auth service)
    }

    [TestMethod]
    public async Task EndToEnd_MfaSetupWorkflow_Success()
    {
        // Stage 1: Create active account
        var account = await CreateActiveTestAccountAsync();

        // Stage 2: Initiate MFA setup
        var setup = await _accountService.InitiateMfaSetupAsync(account.Id);
        Assert.IsNotNull(setup);
        Assert.IsFalse(string.IsNullOrEmpty(setup.Secret));
        Assert.AreEqual(10, setup.BackupCodes.Length);

        // Stage 3: Generate valid TOTP code
        var totp = new Totp(Base32Encoding.ToBytes(setup.Secret));
        var code = totp.ComputeTotp(DateTime.UtcNow);

        // Stage 4: Complete MFA setup
        await _accountService.CompleteMfaSetupAsync(account.Id, code);

        // Stage 5: Verify MFA enabled
        var updatedAccount = await _accountService.GetAccountAsync(account.Id);
        Assert.IsTrue(updatedAccount.MfaEnabled);
    }

    [TestMethod]
    public async Task Search_WithMultipleAccounts_ReturnsCorrectResults()
    {
        // Arrange - Create test accounts
        await CreateTestAccountAsync("user1@example.com", AccountStatus.Active);
        await CreateTestAccountAsync("user2@example.com", AccountStatus.Active);
        await CreateTestAccountAsync("user3@example.com", AccountStatus.Deactivated);

        // Act
        var criteria = new AccountSearchCriteria
        {
            Status = AccountStatus.Active,
            Skip = 0,
            Take = 10
        };

        var result = await _accountService.SearchAccountsAsync(criteria);

        // Assert
        Assert.IsTrue(result.Items.Count >= 2);
        Assert.IsTrue(result.Items.All(a => a.Status == AccountStatus.Active));
    }

    private async Task<Account> CreateTestAccountAsync(
        string email = null,
        AccountStatus status = AccountStatus.Active)
    {
        var request = new CreateAccountRequest
        {
            Email = email ?? $"test_{Guid.NewGuid():N}@example.com",
            Password = "P@ssw0rd123"
        };

        var account = await _accountService.CreateAccountAsync(request);

        if (status == AccountStatus.Active)
        {
            await _accountService.VerifyEmailAsync(account.Id.ToString());
        }

        return account;
    }
}
```

---

## Azure AD B2C Integration Tests

**File:** `AzureAdB2CTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.LiveIntegration)] // Requires Azure test tenant
public class AzureAdB2CTests
{
    private IManageGraphUser _graphUser;
    private string _testTenantId;

    [TestInitialize]
    public void Setup()
    {
        _testTenantId = TestContext.GetRequiredProperty<string>("AZURE_B2C_TENANT_ID");
        var clientId = TestContext.GetRequiredProperty<string>("AZURE_B2C_CLIENT_ID");
        var clientSecret = TestContext.GetRequiredProperty<string>("AZURE_B2C_CLIENT_SECRET");

        var services = new ServiceCollection();
        services.AddAzureAdB2CIntegration(options =>
        {
            options.TenantId = _testTenantId;
            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
        });

        var provider = services.BuildServiceProvider();
        _graphUser = provider.GetRequiredService<IManageGraphUser>();
    }

    [TestMethod]
    public async Task CreateUser_ValidRequest_CreatesInAzureAd()
    {
        // Arrange
        var email = $"test_{Guid.NewGuid():N}@contoso.onmicrosoft.com";
        var request = new GraphUserCreate
        {
            Email = email,
            DisplayName = "Test User",
            AccountEnabled = true
        };

        // Act
        var azureId = await _graphUser.CreateUserAsync(request);

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(azureId));

        // Cleanup
        await _graphUser.DeleteUserAsync(azureId);
    }

    [TestMethod]
    public async Task UpdateUser_ExistingUser_Updates()
    {
        // Arrange - Create user first
        var azureId = await CreateTestUserAsync();

        // Act
        var updateRequest = new GraphUserUpdate
        {
            DisplayName = "Updated Name",
            AccountEnabled = false
        };

        await _graphUser.UpdateUserAsync(azureId, updateRequest);

        // Assert
        var user = await _graphUser.GetUserAsync(azureId);
        Assert.AreEqual("Updated Name", user.DisplayName);
        Assert.IsFalse(user.AccountEnabled);

        // Cleanup
        await _graphUser.DeleteUserAsync(azureId);
    }

    private async Task<string> CreateTestUserAsync()
    {
        return await _graphUser.CreateUserAsync(new GraphUserCreate
        {
            Email = $"test_{Guid.NewGuid():N}@contoso.onmicrosoft.com",
            DisplayName = "Test User",
            AccountEnabled = true
        });
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Cleanup any remaining test users
    }
}
```

---

## Security Tests

**File:** `SecurityTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Unit)]
public class SecurityTests
{
    [TestMethod]
    public void PasswordHash_BcryptWorkFactor12_TakesExpectedTime()
    {
        // Arrange
        var hasher = new BcryptPasswordHasher(Options.Create(new PasswordHasherOptions { WorkFactor = 12 }));
        var password = "P@ssw0rd123";

        // Act
        var stopwatch = Stopwatch.StartNew();
        var hash = hasher.HashPassword(password);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 200, "bcrypt work factor 12 should take ~200-300ms");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds <= 500, "Should not exceed 500ms");
    }

    [TestMethod]
    public void TokenGeneration_CryptographicallySecure_IsRandom()
    {
        // Arrange
        var generator = new CryptographicTokenGenerator();

        // Act
        var token1 = generator.GenerateToken();
        var token2 = generator.GenerateToken();

        // Assert
        Assert.AreNotEqual(token1, token2);
        Assert.AreEqual(64, token1.Length); // 256 bits hex = 64 chars
        Assert.IsTrue(token1.All(c => "0123456789abcdef".Contains(c)));
    }

    [TestMethod]
    public void MfaSecret_Base32Encoded_ValidLength()
    {
        // Arrange
        var service = new MfaService();

        // Act
        var setup = service.GenerateSetup("test@example.com");

        // Assert
        Assert.IsTrue(setup.Secret.Length >= 16); // At least 80 bits
        Assert.IsTrue(setup.Secret.All(c => "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".Contains(c))); // Base32 alphabet
    }

    [TestMethod]
    public void PasswordComplexity_Entropy_MeetsMinimum()
    {
        // Arrange
        var passwords = new[]
        {
            "P@ssw0rd123",     // High entropy
            "password",        // Low entropy
            "P@ss",            // Very low entropy
        };

        // Act & Assert
        var entropy1 = CalculatePasswordEntropy("P@ssw0rd123");
        var entropy2 = CalculatePasswordEntropy("password");

        Assert.IsTrue(entropy1 > 50, "Complex password should have >50 bits entropy");
        Assert.IsTrue(entropy2 < 40, "Simple password should have <40 bits entropy");
    }

    private double CalculatePasswordEntropy(string password)
    {
        var charsetSize = 0;
        if (password.Any(char.IsLower)) charsetSize += 26;
        if (password.Any(char.IsUpper)) charsetSize += 26;
        if (password.Any(char.IsDigit)) charsetSize += 10;
        if (password.Any(c => !char.IsLetterOrDigit(c))) charsetSize += 32;

        return password.Length * Math.Log2(charsetSize);
    }
}
```

---

## Performance Tests

**File:** `PerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class PerformanceTests
{
    [TestMethod]
    public async Task AccountLookupById_Cached_UnderTenMilliseconds()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var mockCache = new Mock<IDistributedCache>();
        mockCache.Setup(c => c.GetAsync<Account>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Account { Id = accountId });

        var repository = new AccountRepository(null, mockCache.Object);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var account = await repository.GetByIdAsync(accountId);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10, $"Cached lookup took {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task BulkAccountCreation_1000Accounts_CompletesInReasonableTime()
    {
        // Arrange
        var service = CreateTestAccountService();

        // Act
        var stopwatch = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, 1000).Select(i =>
            service.CreateAccountAsync(new CreateAccountRequest
            {
                Email = $"test{i}@example.com",
                Password = "P@ssw0rd123"
            }));

        await Task.WhenAll(tasks);

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000, "1000 accounts should be created in <30 seconds");
        Console.WriteLine($"Created 1000 accounts in {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| AccountService | 90% | CreateAccount, ChangePassword, MFA setup |
| PasswordHasher | 100% | HashPassword, VerifyPassword |
| MfaService | 95% | GenerateSetup, VerifyTotp |
| AccountRepository | 85% | CRUD operations, Search |
| Azure AD Integration | 70% | CreateUser, UpdateUser |

---

## Test Data Builders

```csharp
public static class AccountTestData
{
    public static CreateAccountRequest ValidCreateRequest()
    {
        return new CreateAccountRequest
        {
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Password = "P@ssw0rd123",
            DisplayName = "Test User"
        };
    }

    public static Account ActiveAccount()
    {
        return new Account
        {
            Id = Guid.NewGuid(),
            Email = $"test_{Guid.NewGuid():N}@example.com",
            Status = AccountStatus.Active,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 07 Overview](../README.md)
