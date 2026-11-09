# Modular Profile Management - Testing Strategy

**Epic:** 07 - Identity & Session Management
**Feature:** 04 - Modular Profile Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Overview

This document defines the comprehensive testing strategy for the Modular Profile Management system, covering unit tests, integration tests, performance tests, and security tests.

---

## Testing Principles

1. **Comprehensive Coverage**: 80%+ code coverage for Framework layer
2. **Test Pyramid**: Heavy unit tests, moderate integration tests, light E2E tests
3. **Isolation**: Tests are independent and can run in parallel
4. **Repeatability**: Tests produce consistent results across environments
5. **Fast Feedback**: Unit tests complete in <5 seconds, integration tests in <30 seconds
6. **Real-World Scenarios**: Tests cover actual usage patterns
7. **Security First**: Security and privacy tests are mandatory

---

## Test Categories

### Unit Tests (80+ tests)

Test individual components in isolation with mocked dependencies.

**Scope:**
- Profile modules (validation, default data)
- Validation framework (validators, rules)
- Visibility service (permission checks, filtering)
- Versioning service (version creation, rollback)
- Export/import services (serialization, deserialization)
- Storage providers (CRUD operations)

**Characteristics:**
- Fast (<100ms each)
- No external dependencies
- In-memory implementations
- High code coverage

**Category:** `Unit`

---

### Integration Tests (30+ tests)

Test component interactions with real dependencies (database, blob storage).

**Scope:**
- End-to-end profile workflows
- Storage provider integration
- Cache behavior
- Event publishing
- Multi-module scenarios

**Characteristics:**
- Medium speed (<2 seconds each)
- Docker-based dependencies
- Database transactions (rollback)
- Realistic data

**Category:** `Integration`

---

### Performance Tests (10+ tests)

Validate performance requirements and identify bottlenecks.

**Scope:**
- Profile load times
- Profile save times
- Completeness calculation
- Snapshot retrieval
- Bulk operations

**Characteristics:**
- Benchmarking (BenchmarkDotNet)
- Load testing (NBomber)
- Memory profiling
- Database query analysis

**Category:** `DevLocal`

---

### Security Tests (15+ tests)

Validate security controls and privacy features.

**Scope:**
- Visibility enforcement
- Authorization checks
- Field-level filtering
- Audit logging
- Export security

**Characteristics:**
- Negative testing (unauthorized access)
- Privacy validation
- Audit trail verification
- Data masking

**Category:** `Unit` (most), `Integration` (some)

---

## Test Organization

### Project Structure

```
OoBDev.Framework.Identity.ProfileManagement.Tests/
├── Unit/
│   ├── Modules/
│   │   ├── UserProfileCoreModuleTests.cs
│   │   ├── OrganizationProfileCoreModuleTests.cs
│   │   ├── ContactPreferencesModuleTests.cs
│   │   └── SocialLinksModuleTests.cs
│   ├── Validation/
│   │   ├── ValidationResultTests.cs
│   │   ├── RequiredFieldValidatorTests.cs
│   │   ├── EmailValidatorTests.cs
│   │   └── CompositeValidatorTests.cs
│   ├── Visibility/
│   │   ├── ProfileVisibilityServiceTests.cs
│   │   ├── VisibilityFilterTests.cs
│   │   └── PermissionCheckTests.cs
│   ├── Versioning/
│   │   ├── ProfileVersioningServiceTests.cs
│   │   ├── VersionCreationTests.cs
│   │   └── RollbackTests.cs
│   ├── Export/
│   │   ├── ProfileExportServiceTests.cs
│   │   └── JsonExportTests.cs
│   └── Import/
│       ├── ProfileImportServiceTests.cs
│       └── ValidationTests.cs
├── Integration/
│   ├── ProfileServiceIntegrationTests.cs
│   ├── StorageProviderTests.cs
│   ├── BlobStorageTests.cs
│   ├── CachingTests.cs
│   └── EndToEndWorkflowTests.cs
├── Performance/
│   ├── ProfileLoadBenchmarks.cs
│   ├── ProfileSaveBenchmarks.cs
│   └── CompletenessCalculationBenchmarks.cs
├── Security/
│   ├── VisibilityEnforcementTests.cs
│   ├── AuthorizationTests.cs
│   └── AuditLoggingTests.cs
└── Helpers/
    ├── TestDataBuilders.cs
    ├── InMemoryStorageProvider.cs
    └── MockProfileModule.cs
```

---

## Unit Test Cases

### Module Validation Tests (15 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Framework.Identity.ProfileManagement;
using OoBDev.Framework.Identity.ProfileManagement.Modules;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Unit.Modules;

[TestClass]
public class UserProfileCoreModuleTests
{
    private UserProfileCoreModule _module = null!;

    [TestInitialize]
    public void Setup()
    {
        _module = new UserProfileCoreModule();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1-555-0123"
        };

        var context = new ValidationContext
        {
            ProfileId = "user-123",
            ModuleName = "UserCore",
            IsCreate = true,
            AdditionalData = new Dictionary<string, object>()
        };

        // Act
        var result = await _module.ValidateAsync(data, context);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateAsync_MissingFirstName_ReturnsError()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "", // Missing
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var context = new ValidationContext
        {
            ProfileId = "user-123",
            ModuleName = "UserCore",
            IsCreate = true,
            AdditionalData = new Dictionary<string, object>()
        };

        // Act
        var result = await _module.ValidateAsync(data, context);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(nameof(data.FirstName), result.Errors[0].PropertyPath);
        Assert.AreEqual("REQUIRED", result.Errors[0].ErrorCode);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateAsync_InvalidEmail_ReturnsError()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "invalid-email" // Invalid format
        };

        var context = new ValidationContext
        {
            ProfileId = "user-123",
            ModuleName = "UserCore",
            IsCreate = true,
            AdditionalData = new Dictionary<string, object>()
        };

        // Act
        var result = await _module.ValidateAsync(data, context);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(nameof(data.Email), result.Errors[0].PropertyPath);
        Assert.AreEqual("INVALID_EMAIL", result.Errors[0].ErrorCode);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "", // Missing
            LastName = "", // Missing
            Email = "invalid-email" // Invalid format
        };

        var context = new ValidationContext
        {
            ProfileId = "user-123",
            ModuleName = "UserCore",
            IsCreate = true,
            AdditionalData = new Dictionary<string, object>()
        };

        // Act
        var result = await _module.ValidateAsync(data, context);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(3, result.Errors.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetDefaultDataAsync_ReturnsEmptyData()
    {
        // Act
        var data = await _module.GetDefaultDataAsync();

        // Assert
        Assert.IsNotNull(data);
        Assert.IsNull(data.FirstName);
        Assert.IsNull(data.LastName);
        Assert.IsNull(data.Email);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Name_ReturnsUserCore()
    {
        // Assert
        Assert.AreEqual("UserCore", _module.Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Category_ReturnsPersonal()
    {
        // Assert
        Assert.AreEqual(ProfileModuleCategories.Personal, _module.Category);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Version_ReturnsValidVersion()
    {
        // Assert
        Assert.IsNotNull(_module.Version);
        Assert.IsTrue(_module.Version >= new Version(1, 0, 0));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Dependencies_ReturnsEmptyList()
    {
        // Assert
        Assert.IsNotNull(_module.Dependencies);
        Assert.AreEqual(0, _module.Dependencies.Count);
    }
}
```

---

### Validation Framework Tests (12 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Framework.Identity.ProfileManagement.Validation;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Unit.Validation;

[TestClass]
public class ValidationResultTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Success_CreatesValidResult()
    {
        // Act
        var result = ValidationResult.Success();

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
        Assert.AreEqual(0, result.Warnings.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Failure_CreatesInvalidResult()
    {
        // Arrange
        var error = new ValidationError("Field1", "ERROR_CODE", "Error message", null);

        // Act
        var result = ValidationResult.Failure(error);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual(error, result.Errors[0]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Combine_MultipleResults_CombinesErrorsAndWarnings()
    {
        // Arrange
        var result1 = ValidationResult.Failure(
            new ValidationError("Field1", "ERROR1", "Error 1", null)
        );

        var result2 = ValidationResult.Failure(
            new ValidationError("Field2", "ERROR2", "Error 2", null)
        );

        var result3 = ValidationResult.Success();

        // Act
        var combined = ValidationResult.Combine(result1, result2, result3);

        // Assert
        Assert.IsFalse(combined.IsValid);
        Assert.AreEqual(2, combined.Errors.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        // Arrange
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        // Act
        var combined = ValidationResult.Combine(result1, result2);

        // Assert
        Assert.IsTrue(combined.IsValid);
        Assert.AreEqual(0, combined.Errors.Count);
    }
}

[TestClass]
public class RequiredFieldValidatorTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateAsync_ValuePresent_ReturnsSuccess()
    {
        // Arrange
        var validator = new RequiredFieldValidator<TestData>(d => d.Name);
        var data = new TestData { Name = "John" };

        // Act
        var result = await validator.ValidateAsync(data, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateAsync_ValueNull_ReturnsError()
    {
        // Arrange
        var validator = new RequiredFieldValidator<TestData>(d => d.Name);
        var data = new TestData { Name = null };

        // Act
        var result = await validator.ValidateAsync(data, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
        Assert.AreEqual("REQUIRED", result.Errors[0].ErrorCode);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ValidateAsync_ValueEmptyString_ReturnsError()
    {
        // Arrange
        var validator = new RequiredFieldValidator<TestData>(d => d.Name);
        var data = new TestData { Name = "" };

        // Act
        var result = await validator.ValidateAsync(data, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(1, result.Errors.Count);
    }

    private class TestData
    {
        public string? Name { get; set; }
    }
}

[TestClass]
public class EmailValidatorTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("user@example.com")]
    [DataRow("user.name@example.com")]
    [DataRow("user+tag@example.co.uk")]
    public async Task ValidateAsync_ValidEmail_ReturnsSuccess(string email)
    {
        // Arrange
        var validator = new EmailValidator<TestData>(d => d.Email);
        var data = new TestData { Email = email };

        // Act
        var result = await validator.ValidateAsync(data, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("invalid")]
    [DataRow("@example.com")]
    [DataRow("user@")]
    [DataRow("user name@example.com")]
    public async Task ValidateAsync_InvalidEmail_ReturnsError(string email)
    {
        // Arrange
        var validator = new EmailValidator<TestData>(d => d.Email);
        var data = new TestData { Email = email };

        // Act
        var result = await validator.ValidateAsync(data, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("INVALID_EMAIL", result.Errors[0].ErrorCode);
    }

    private class TestData
    {
        public string? Email { get; set; }
    }
}
```

---

### Profile Service Tests (18 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Framework.Identity.ProfileManagement;
using OoBDev.Framework.Identity.ProfileManagement.Storage;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Unit;

[TestClass]
public class ProfileServiceTests
{
    private Mock<IProfileModuleRegistry> _mockRegistry = null!;
    private Mock<IProfileModuleStorageProvider> _mockStorageProvider = null!;
    private Mock<IProfileVersioningService> _mockVersioningService = null!;
    private Mock<IProfileVisibilityService> _mockVisibilityService = null!;
    private ProfileService _profileService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRegistry = new Mock<IProfileModuleRegistry>();
        _mockStorageProvider = new Mock<IProfileModuleStorageProvider>();
        _mockVersioningService = new Mock<IProfileVersioningService>();
        _mockVisibilityService = new Mock<IProfileVisibilityService>();

        _profileService = new ProfileService(
            _mockRegistry.Object,
            _mockStorageProvider.Object,
            _mockVersioningService.Object,
            _mockVisibilityService.Object
        );
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetModuleAsync_ModuleExists_ReturnsData()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";
        var expectedData = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockStorageProvider
            .Setup(sp => sp.GetAsync<UserProfileCoreData>(profileId, moduleName, default))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _profileService.GetModuleAsync<UserProfileCoreData>(
            profileId,
            moduleName
        );

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedData.FirstName, result.FirstName);
        Assert.AreEqual(expectedData.Email, result.Email);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetModuleAsync_ModuleNotFound_ReturnsNull()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";

        _mockStorageProvider
            .Setup(sp => sp.GetAsync<UserProfileCoreData>(profileId, moduleName, default))
            .ReturnsAsync((UserProfileCoreData?)null);

        // Act
        var result = await _profileService.GetModuleAsync<UserProfileCoreData>(
            profileId,
            moduleName
        );

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveModuleAsync_ValidData_SavesSuccessfully()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var mockModule = new Mock<IProfileModule<UserProfileCoreData>>();
        mockModule.Setup(m => m.ValidateAsync(data, It.IsAny<ValidationContext>(), default))
            .ReturnsAsync(ValidationResult.Success());

        _mockRegistry
            .Setup(r => r.GetModuleAsync(moduleName, default))
            .ReturnsAsync(mockModule.Object);

        _mockVisibilityService
            .Setup(vs => vs.CanModifyModuleAsync(profileId, moduleName, It.IsAny<string>(), default))
            .ReturnsAsync(true);

        // Act
        await _profileService.SaveModuleAsync(profileId, moduleName, data);

        // Assert
        _mockStorageProvider.Verify(
            sp => sp.SaveAsync(profileId, moduleName, data, It.IsAny<ProfileModuleMetadata>(), default),
            Times.Once
        );

        _mockVersioningService.Verify(
            vs => vs.CreateVersionAsync(profileId, moduleName, data, It.IsAny<string>(), It.IsAny<string>(), default),
            Times.Once
        );
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(ProfileModuleValidationException))]
    public async Task SaveModuleAsync_InvalidData_ThrowsValidationException()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";
        var data = new UserProfileCoreData
        {
            FirstName = "", // Invalid
            LastName = "Doe",
            Email = "invalid-email"
        };

        var mockModule = new Mock<IProfileModule<UserProfileCoreData>>();
        var validationError = new ValidationError("FirstName", "REQUIRED", "Required", "");
        mockModule.Setup(m => m.ValidateAsync(data, It.IsAny<ValidationContext>(), default))
            .ReturnsAsync(ValidationResult.Failure(validationError));

        _mockRegistry
            .Setup(r => r.GetModuleAsync(moduleName, default))
            .ReturnsAsync(mockModule.Object);

        // Act
        await _profileService.SaveModuleAsync(profileId, moduleName, data);

        // Assert - ExpectedException
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(UnauthorizedProfileAccessException))]
    public async Task SaveModuleAsync_Unauthorized_ThrowsException()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var mockModule = new Mock<IProfileModule<UserProfileCoreData>>();
        mockModule.Setup(m => m.ValidateAsync(data, It.IsAny<ValidationContext>(), default))
            .ReturnsAsync(ValidationResult.Success());

        _mockRegistry
            .Setup(r => r.GetModuleAsync(moduleName, default))
            .ReturnsAsync(mockModule.Object);

        _mockVisibilityService
            .Setup(vs => vs.CanModifyModuleAsync(profileId, moduleName, It.IsAny<string>(), default))
            .ReturnsAsync(false); // Unauthorized

        // Act
        await _profileService.SaveModuleAsync(profileId, moduleName, data);

        // Assert - ExpectedException
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task DeleteModuleAsync_ModuleExists_DeletesSuccessfully()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";

        _mockVisibilityService
            .Setup(vs => vs.CanModifyModuleAsync(profileId, moduleName, It.IsAny<string>(), default))
            .ReturnsAsync(true);

        // Act
        await _profileService.DeleteModuleAsync(profileId, moduleName);

        // Assert
        _mockStorageProvider.Verify(
            sp => sp.DeleteAsync(profileId, moduleName, default),
            Times.Once
        );
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetCompletenessScoreAsync_CalculatesCorrectScore()
    {
        // Arrange
        var profileId = "user-123";

        var userCoreData = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
            // Avatar missing (optional)
        };

        _mockStorageProvider
            .Setup(sp => sp.GetAsync<UserProfileCoreData>(profileId, "UserCore", default))
            .ReturnsAsync(userCoreData);

        _mockRegistry
            .Setup(r => r.GetRequiredModulesAsync(default))
            .ReturnsAsync(new[] { "UserCore" });

        // Act
        var score = await _profileService.GetCompletenessScoreAsync(profileId);

        // Assert
        Assert.IsNotNull(score);
        Assert.IsTrue(score.Score > 0 && score.Score <= 100);
        Assert.AreEqual(profileId, score.ProfileId);
    }
}
```

---

### Visibility Service Tests (10 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Framework.Identity.ProfileManagement.Visibility;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Unit.Visibility;

[TestClass]
public class ProfileVisibilityServiceTests
{
    private ProfileVisibilityService _visibilityService = null!;

    [TestInitialize]
    public void Setup()
    {
        _visibilityService = new ProfileVisibilityService();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CanViewModuleAsync_OwnerViewingOwnProfile_ReturnsTrue()
    {
        // Arrange
        var profileId = "user-123";
        var viewerId = "user-123"; // Same user
        var moduleName = "UserCore";

        // Act
        var canView = await _visibilityService.CanViewModuleAsync(
            profileId,
            moduleName,
            viewerId
        );

        // Assert
        Assert.IsTrue(canView);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CanViewModuleAsync_PublicModule_ReturnsTrue()
    {
        // Arrange
        var profileId = "user-123";
        var viewerId = "user-456"; // Different user
        var moduleName = "UserCore";

        await _visibilityService.SetModuleVisibilityAsync(
            profileId,
            moduleName,
            ProfileModuleVisibility.Public
        );

        // Act
        var canView = await _visibilityService.CanViewModuleAsync(
            profileId,
            moduleName,
            viewerId
        );

        // Assert
        Assert.IsTrue(canView);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CanViewModuleAsync_PrivateModule_ReturnsFalse()
    {
        // Arrange
        var profileId = "user-123";
        var viewerId = "user-456"; // Different user
        var moduleName = "UserCore";

        await _visibilityService.SetModuleVisibilityAsync(
            profileId,
            moduleName,
            ProfileModuleVisibility.Private
        );

        // Act
        var canView = await _visibilityService.CanViewModuleAsync(
            profileId,
            moduleName,
            viewerId
        );

        // Assert
        Assert.IsFalse(canView);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyVisibilityFilterAsync_RemovesPrivateFields()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "+1-555-0123"
        };

        var profileId = "user-123";
        var viewerId = "user-456";
        var moduleName = "UserCore";

        // Set phone number as private
        await _visibilityService.SetFieldVisibilityAsync(
            profileId,
            moduleName,
            nameof(data.PhoneNumber),
            ProfileModuleVisibility.Private
        );

        // Act
        var filtered = await _visibilityService.ApplyVisibilityFilterAsync(
            data,
            moduleName,
            viewerId
        );

        // Assert
        Assert.IsNotNull(filtered);
        Assert.AreEqual("John", filtered.FirstName);
        Assert.AreEqual("Doe", filtered.LastName);
        Assert.AreEqual("john@example.com", filtered.Email);
        Assert.IsNull(filtered.PhoneNumber); // Removed
    }
}
```

---

### Versioning Service Tests (8 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Framework.Identity.ProfileManagement.Versioning;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Unit.Versioning;

[TestClass]
public class ProfileVersioningServiceTests
{
    private Mock<IProfileModuleStorageProvider> _mockStorageProvider = null!;
    private ProfileVersioningService _versioningService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockStorageProvider = new Mock<IProfileModuleStorageProvider>();
        _versioningService = new ProfileVersioningService(_mockStorageProvider.Object);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CreateVersionAsync_CreatesVersionRecord()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };
        var createdBy = "admin-456";
        var changeReason = "Initial profile creation";

        // Act
        var version = await _versioningService.CreateVersionAsync(
            profileId,
            moduleName,
            data,
            createdBy,
            changeReason
        );

        // Assert
        Assert.IsNotNull(version);
        Assert.AreEqual(profileId, version.ProfileId);
        Assert.AreEqual(moduleName, version.ModuleName);
        Assert.AreEqual(createdBy, version.CreatedBy);
        Assert.AreEqual(changeReason, version.ChangeReason);
        Assert.IsTrue(version.VersionNumber > 0);
        Assert.IsNotNull(version.DataJson);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetVersionHistoryAsync_ReturnsVersionsNewestFirst()
    {
        // Arrange
        var profileId = "user-123";
        var moduleName = "UserCore";

        var versions = new List<ProfileModuleVersion>
        {
            CreateVersion(profileId, moduleName, 1, DateTime.UtcNow.AddDays(-2)),
            CreateVersion(profileId, moduleName, 2, DateTime.UtcNow.AddDays(-1)),
            CreateVersion(profileId, moduleName, 3, DateTime.UtcNow)
        };

        _mockStorageProvider
            .Setup(sp => sp.GetVersionHistoryAsync(profileId, moduleName, default))
            .ReturnsAsync(versions.OrderByDescending(v => v.VersionNumber).ToList());

        // Act
        var history = await _versioningService.GetVersionHistoryAsync(profileId, moduleName);

        // Assert
        Assert.AreEqual(3, history.Count);
        Assert.AreEqual(3, history[0].VersionNumber); // Newest first
        Assert.AreEqual(2, history[1].VersionNumber);
        Assert.AreEqual(1, history[2].VersionNumber);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RollbackToVersionAsync_CreatesNewVersion()
    {
        // Arrange
        var versionId = "version-123";
        var profileId = "user-123";
        var moduleName = "UserCore";
        var rolledBackBy = "admin-456";
        var reason = "Restore previous version";

        var originalVersion = CreateVersion(profileId, moduleName, 5, DateTime.UtcNow.AddDays(-1));

        _mockStorageProvider
            .Setup(sp => sp.GetVersionAsync(versionId, default))
            .ReturnsAsync(originalVersion);

        // Act
        var newVersion = await _versioningService.RollbackToVersionAsync(
            versionId,
            rolledBackBy,
            reason
        );

        // Assert
        Assert.IsNotNull(newVersion);
        Assert.AreEqual(profileId, newVersion.ProfileId);
        Assert.AreEqual(moduleName, newVersion.ModuleName);
        Assert.AreEqual(rolledBackBy, newVersion.CreatedBy);
        Assert.IsTrue(newVersion.VersionNumber > originalVersion.VersionNumber);
        Assert.AreEqual(originalVersion.DataJson, newVersion.DataJson); // Same data
    }

    private ProfileModuleVersion CreateVersion(
        string profileId,
        string moduleName,
        int versionNumber,
        DateTime createdAt)
    {
        return new ProfileModuleVersion
        {
            Id = $"version-{versionNumber}",
            ProfileId = profileId,
            ModuleName = moduleName,
            VersionNumber = versionNumber,
            CreatedAt = createdAt,
            CreatedBy = "user-123",
            DataJson = "{}",
            Metadata = new ProfileModuleMetadata
            {
                Name = moduleName,
                DisplayName = moduleName,
                Category = "Personal",
                Version = new Version(1, 0, 0),
                Description = "Test module",
                Dependencies = Array.Empty<string>(),
                DefaultVisibility = ProfileModuleVisibility.Private,
                DataType = typeof(object),
                StorageProviderType = typeof(InMemoryStorageProvider)
            }
        };
    }
}
```

---

## Integration Test Cases

### End-to-End Workflow Tests (8 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Framework.Identity.ProfileManagement;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Integration;

[TestClass]
public class EndToEndWorkflowTests
{
    private IProfileService _profileService = null!;
    private string _testProfileId = null!;

    [TestInitialize]
    public void Setup()
    {
        // Setup with real dependencies (Docker-based database)
        _testProfileId = $"test-profile-{Guid.NewGuid():N}";
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CompleteProfileWorkflow_CreateUpdateDeleteProfile_Success()
    {
        // Stage 1: Create profile
        var createData = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        await _profileService.SaveModuleAsync(
            _testProfileId,
            "UserCore",
            createData
        );

        // Stage 2: Read profile
        var readData = await _profileService.GetModuleAsync<UserProfileCoreData>(
            _testProfileId,
            "UserCore"
        );

        Assert.IsNotNull(readData);
        Assert.AreEqual("John", readData.FirstName);
        Assert.AreEqual("john.doe@example.com", readData.Email);

        // Stage 3: Update profile
        var updateData = readData with { FirstName = "Jane" };

        await _profileService.SaveModuleAsync(
            _testProfileId,
            "UserCore",
            updateData,
            changeReason: "Updated first name"
        );

        // Stage 4: Verify update
        var updatedData = await _profileService.GetModuleAsync<UserProfileCoreData>(
            _testProfileId,
            "UserCore"
        );

        Assert.AreEqual("Jane", updatedData?.FirstName);

        // Stage 5: Delete profile
        await _profileService.DeleteModuleAsync(_testProfileId, "UserCore");

        // Stage 6: Verify deletion
        var deletedData = await _profileService.GetModuleAsync<UserProfileCoreData>(
            _testProfileId,
            "UserCore"
        );

        Assert.IsNull(deletedData);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task MultiModuleProfile_SaveAndRetrieveSnapshot_Success()
    {
        // Create user core module
        await _profileService.SaveModuleAsync(
            _testProfileId,
            "UserCore",
            new UserProfileCoreData
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            }
        );

        // Create social links module
        await _profileService.SaveModuleAsync(
            _testProfileId,
            "SocialLinks",
            new SocialLinksData
            {
                LinkedIn = "https://linkedin.com/in/johndoe",
                Twitter = "https://twitter.com/johndoe"
            }
        );

        // Create contact preferences module
        await _profileService.SaveModuleAsync(
            _testProfileId,
            "ContactPreferences",
            new ContactPreferencesData
            {
                AllowEmail = true,
                AllowSms = false,
                EmailFrequency = EmailFrequency.Weekly
            }
        );

        // Get complete snapshot
        var snapshot = await _profileService.GetProfileSnapshotAsync(_testProfileId);

        // Assert
        Assert.IsNotNull(snapshot);
        Assert.AreEqual(3, snapshot.Modules.Count);
        Assert.IsTrue(snapshot.Modules.ContainsKey("UserCore"));
        Assert.IsTrue(snapshot.Modules.ContainsKey("SocialLinks"));
        Assert.IsTrue(snapshot.Modules.ContainsKey("ContactPreferences"));
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Delete test profile
        try
        {
            await _profileService.DeleteModuleAsync(_testProfileId, "UserCore");
            await _profileService.DeleteModuleAsync(_testProfileId, "SocialLinks");
            await _profileService.DeleteModuleAsync(_testProfileId, "ContactPreferences");
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
```

---

### Storage Provider Tests (6 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Framework.Identity.ProfileManagement.Storage;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Integration;

[TestClass]
public class SqlStorageProviderTests
{
    private SqlProfileStorageProvider _storageProvider = null!;
    private string _testProfileId = null!;

    [TestInitialize]
    public void Setup()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING");
        _storageProvider = new SqlProfileStorageProvider(connectionString);
        _testProfileId = $"test-{Guid.NewGuid():N}";
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SaveAndGetAsync_ValidData_Success()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act - Save
        await _storageProvider.SaveAsync(
            _testProfileId,
            "UserCore",
            data,
            CreateMetadata()
        );

        // Act - Get
        var retrieved = await _storageProvider.GetAsync<UserProfileCoreData>(
            _testProfileId,
            "UserCore"
        );

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(data.FirstName, retrieved.FirstName);
        Assert.AreEqual(data.Email, retrieved.Email);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task DeleteAsync_ExistingData_RemovesData()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        await _storageProvider.SaveAsync(_testProfileId, "UserCore", data, CreateMetadata());

        // Act
        await _storageProvider.DeleteAsync(_testProfileId, "UserCore");

        // Assert
        var retrieved = await _storageProvider.GetAsync<UserProfileCoreData>(
            _testProfileId,
            "UserCore"
        );

        Assert.IsNull(retrieved);
    }

    private ProfileModuleMetadata CreateMetadata()
    {
        return new ProfileModuleMetadata
        {
            Name = "UserCore",
            DisplayName = "User Core",
            Category = "Personal",
            Version = new Version(1, 0, 0),
            Description = "Core user profile",
            Dependencies = Array.Empty<string>(),
            DefaultVisibility = ProfileModuleVisibility.Private,
            DataType = typeof(UserProfileCoreData),
            StorageProviderType = typeof(SqlProfileStorageProvider)
        };
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        try
        {
            await _storageProvider.DeleteAsync(_testProfileId, "UserCore");
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
```

---

## Performance Test Cases

### Profile Load Benchmarks (4 tests)

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OoBDev.Framework.Identity.ProfileManagement;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Performance;

[MemoryDiagnoser]
public class ProfileLoadBenchmarks
{
    private IProfileService _profileService = null!;
    private string _testProfileId = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        // Setup profile service with real dependencies
        _testProfileId = "benchmark-profile";

        // Create test profile with multiple modules
        await _profileService.SaveModuleAsync(
            _testProfileId,
            "UserCore",
            new UserProfileCoreData { FirstName = "John", LastName = "Doe", Email = "john@example.com" }
        );
    }

    [Benchmark]
    [TestCategory(TestCategories.DevLocal)]
    public async Task<UserProfileCoreData?> LoadSingleModule()
    {
        return await _profileService.GetModuleAsync<UserProfileCoreData>(
            _testProfileId,
            "UserCore"
        );
    }

    [Benchmark]
    [TestCategory(TestCategories.DevLocal)]
    public async Task<ProfileSnapshot> LoadCompleteSnapshot()
    {
        return await _profileService.GetProfileSnapshotAsync(_testProfileId);
    }

    [Benchmark]
    [TestCategory(TestCategories.DevLocal)]
    public async Task<ProfileCompletenessScore> CalculateCompleteness()
    {
        return await _profileService.GetCompletenessScoreAsync(_testProfileId);
    }

    // Expected results:
    // - LoadSingleModule: < 100ms (cached), < 500ms (uncached)
    // - LoadCompleteSnapshot: < 1000ms (10 modules)
    // - CalculateCompleteness: < 500ms
}
```

---

## Security Test Cases

### Visibility Enforcement Tests (8 tests)

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Framework.Identity.ProfileManagement;
using OoBDev.Framework.Identity.ProfileManagement.Visibility;

namespace OoBDev.Framework.Identity.ProfileManagement.Tests.Security;

[TestClass]
public class VisibilityEnforcementTests
{
    private IProfileService _profileService = null!;
    private IProfileVisibilityService _visibilityService = null!;
    private string _testProfileId = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testProfileId = $"test-{Guid.NewGuid():N}";

        // Create test profile
        await _profileService.SaveModuleAsync(
            _testProfileId,
            "UserCore",
            new UserProfileCoreData
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1-555-0123"
            }
        );
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(UnauthorizedProfileAccessException))]
    public async Task GetModule_PrivateModule_UnauthorizedUser_ThrowsException()
    {
        // Arrange
        await _visibilityService.SetModuleVisibilityAsync(
            _testProfileId,
            "UserCore",
            ProfileModuleVisibility.Private
        );

        var unauthorizedUserId = "unauthorized-user";

        // Act
        await _profileService.GetModuleAsync<UserProfileCoreData>(
            _testProfileId,
            "UserCore",
            userId: unauthorizedUserId // Unauthorized
        );

        // Assert - ExpectedException
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyVisibilityFilter_RemovesSensitiveFields()
    {
        // Arrange
        var data = new UserProfileCoreData
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PhoneNumber = "+1-555-0123" // Sensitive
        };

        await _visibilityService.SetFieldVisibilityAsync(
            _testProfileId,
            "UserCore",
            nameof(data.PhoneNumber),
            ProfileModuleVisibility.Private
        );

        var viewerId = "other-user";

        // Act
        var filtered = await _visibilityService.ApplyVisibilityFilterAsync(
            data,
            "UserCore",
            viewerId
        );

        // Assert
        Assert.IsNotNull(filtered);
        Assert.AreEqual("John", filtered.FirstName);
        Assert.IsNull(filtered.PhoneNumber); // Removed
    }
}
```

---

## Test Coverage Goals

### Coverage Targets

| Component | Target | Rationale |
|-----------|--------|-----------|
| Profile Modules | 95%+ | Core business logic |
| Validation Framework | 90%+ | Critical data quality |
| Visibility Service | 95%+ | Security critical |
| Versioning Service | 85%+ | Audit compliance |
| Storage Providers | 80%+ | Infrastructure layer |
| Export/Import Services | 75%+ | Edge case heavy |
| Overall Framework | 80%+ | OoBDev standard |

### Coverage Measurement

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coveragereport" \
  -reporttypes:Html

# View coverage report
open coveragereport/index.html
```

---

## Test Data Builders

### ProfileDataBuilder

```csharp
public class ProfileDataBuilder
{
    public static UserProfileCoreData CreateUserCore(
        string firstName = "John",
        string lastName = "Doe",
        string? email = null)
    {
        return new UserProfileCoreData
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email ?? $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
            PhoneNumber = "+1-555-0123"
        };
    }

    public static OrganizationProfileCoreData CreateOrganizationCore(
        string name = "Acme Corporation")
    {
        return new OrganizationProfileCoreData
        {
            Name = name,
            Industry = "Technology",
            Size = OrganizationSize.Medium,
            Description = $"{name} is a leading technology company.",
            Website = $"https://{name.ToLower().Replace(" ", "")}.com"
        };
    }

    public static SocialLinksData CreateSocialLinks(string username = "johndoe")
    {
        return new SocialLinksData
        {
            LinkedIn = $"https://linkedin.com/in/{username}",
            Twitter = $"https://twitter.com/{username}",
            GitHub = $"https://github.com/{username}"
        };
    }
}
```

---

## Summary

This testing strategy provides:

1. **80+ Unit Tests** - Fast, isolated, high coverage
2. **30+ Integration Tests** - Real dependencies, end-to-end workflows
3. **10+ Performance Tests** - Benchmarks and load testing
4. **15+ Security Tests** - Visibility, authorization, audit
5. **Test Data Builders** - Consistent, reusable test data
6. **80%+ Coverage Goal** - Meets OoBDev framework standards

Total: **135+ test cases** covering all aspects of the Modular Profile Management system.
