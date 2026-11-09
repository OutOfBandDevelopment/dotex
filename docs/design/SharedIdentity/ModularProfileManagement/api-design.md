# Modular Profile Management - API Design

**Epic:** 07 - Identity & Session Management
**Feature:** 04 - Modular Profile Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Overview

This document defines the complete API design for the Modular Profile Management system, including all interfaces, implementations, extension methods, and usage examples.

---

## Core Interfaces

### IProfileModule<TData>

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement;

/// <summary>
/// Defines a profile module with typed data, validation, and metadata.
/// </summary>
/// <typeparam name="TData">The data type for this profile module</typeparam>
public interface IProfileModule<TData> where TData : class, new()
{
    /// <summary>
    /// Gets the unique name of this profile module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the category this module belongs to.
    /// </summary>
    /// <seealso cref="ProfileModuleCategories"/>
    string Category { get; }

    /// <summary>
    /// Gets the version of this module.
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// Gets the list of module names this module depends on.
    /// </summary>
    IReadOnlyList<string> Dependencies { get; }

    /// <summary>
    /// Validates the module data.
    /// </summary>
    /// <param name="data">The data to validate</param>
    /// <param name="context">Validation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with errors and warnings</returns>
    Task<ValidationResult> ValidateAsync(
        TData data,
        ValidationContext context,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets default data for this module (used for new profiles).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Default module data</returns>
    Task<TData> GetDefaultDataAsync(CancellationToken cancellationToken = default);
}
```

---

### IProfileService

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement;

/// <summary>
/// Service for managing profile modules.
/// </summary>
public interface IProfileService
{
    // Module Operations

    /// <summary>
    /// Gets data for a specific profile module.
    /// </summary>
    /// <typeparam name="TData">The module data type</typeparam>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Module data, or null if not found</returns>
    Task<TData?> GetModuleAsync<TData>(
        string profileId,
        string moduleName,
        CancellationToken cancellationToken = default
    ) where TData : class;

    /// <summary>
    /// Saves data for a specific profile module.
    /// </summary>
    /// <typeparam name="TData">The module data type</typeparam>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="data">The module data to save</param>
    /// <param name="changeReason">Optional reason for the change (for audit trail)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveModuleAsync<TData>(
        string profileId,
        string moduleName,
        TData data,
        string? changeReason = null,
        CancellationToken cancellationToken = default
    ) where TData : class;

    /// <summary>
    /// Deletes a specific profile module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteModuleAsync(
        string profileId,
        string moduleName,
        CancellationToken cancellationToken = default
    );

    // Profile Operations

    /// <summary>
    /// Gets a complete snapshot of all active modules for a profile.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Profile snapshot with all module data</returns>
    Task<ProfileSnapshot> GetProfileSnapshotAsync(
        string profileId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Calculates the completeness score for a profile.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completeness score (0-100)</returns>
    Task<ProfileCompletenessScore> GetCompletenessScoreAsync(
        string profileId,
        CancellationToken cancellationToken = default
    );

    // Discovery

    /// <summary>
    /// Gets all available profile modules in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available module metadata</returns>
    Task<IReadOnlyList<ProfileModuleMetadata>> GetAvailableModulesAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the list of active modules for a profile.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active module names</returns>
    Task<IReadOnlyList<string>> GetActiveModulesAsync(
        string profileId,
        CancellationToken cancellationToken = default
    );
}
```

---

### IProfileModuleStorageProvider

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Storage;

/// <summary>
/// Provides storage abstraction for profile module data.
/// </summary>
public interface IProfileModuleStorageProvider
{
    /// <summary>
    /// Gets module data from storage.
    /// </summary>
    /// <typeparam name="TData">The module data type</typeparam>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Module data, or null if not found</returns>
    Task<TData?> GetAsync<TData>(
        string profileId,
        string moduleName,
        CancellationToken cancellationToken = default
    ) where TData : class;

    /// <summary>
    /// Saves module data to storage.
    /// </summary>
    /// <typeparam name="TData">The module data type</typeparam>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="data">The module data to save</param>
    /// <param name="metadata">Module metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveAsync<TData>(
        string profileId,
        string moduleName,
        TData data,
        ProfileModuleMetadata metadata,
        CancellationToken cancellationToken = default
    ) where TData : class;

    /// <summary>
    /// Deletes module data from storage.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(
        string profileId,
        string moduleName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets version history for a module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of version records</returns>
    Task<IReadOnlyList<ProfileModuleVersion>> GetVersionHistoryAsync(
        string profileId,
        string moduleName,
        CancellationToken cancellationToken = default
    );
}
```

---

### IProfileBlobStorageProvider

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Storage;

/// <summary>
/// Provides blob storage abstraction for profile binary data (avatars, documents).
/// </summary>
public interface IProfileBlobStorageProvider
{
    /// <summary>
    /// Gets a blob stream.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="blobKey">The blob key within the module</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Blob content stream</returns>
    Task<Stream> GetBlobAsync(
        string profileId,
        string moduleName,
        string blobKey,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Saves a blob.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="blobKey">The blob key within the module</param>
    /// <param name="content">The blob content stream</param>
    /// <param name="contentType">The content type (MIME type)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The blob URL</returns>
    Task<string> SaveBlobAsync(
        string profileId,
        string moduleName,
        string blobKey,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a blob.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="blobKey">The blob key within the module</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteBlobAsync(
        string profileId,
        string moduleName,
        string blobKey,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generates a SAS URL for direct browser access to a blob.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="blobKey">The blob key within the module</param>
    /// <param name="expiresIn">Time until URL expires</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SAS URL</returns>
    Task<string> GenerateSasUrlAsync(
        string profileId,
        string moduleName,
        string blobKey,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default
    );
}
```

---

### IProfileVersioningService

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Versioning;

/// <summary>
/// Service for managing profile module versioning and audit trail.
/// </summary>
public interface IProfileVersioningService
{
    /// <summary>
    /// Creates a new version record for a profile module.
    /// </summary>
    /// <typeparam name="TData">The module data type</typeparam>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="data">The module data</param>
    /// <param name="createdBy">The user creating this version</param>
    /// <param name="changeReason">Optional reason for the change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created version record</returns>
    Task<ProfileModuleVersion> CreateVersionAsync<TData>(
        string profileId,
        string moduleName,
        TData data,
        string createdBy,
        string? changeReason = null,
        CancellationToken cancellationToken = default
    ) where TData : class;

    /// <summary>
    /// Gets version history for a profile module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of version records, newest first</returns>
    Task<IReadOnlyList<ProfileModuleVersion>> GetVersionHistoryAsync(
        string profileId,
        string moduleName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the data for a specific version.
    /// </summary>
    /// <typeparam name="TData">The module data type</typeparam>
    /// <param name="versionId">The version identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The version data</returns>
    Task<TData?> GetVersionDataAsync<TData>(
        string versionId,
        CancellationToken cancellationToken = default
    ) where TData : class;

    /// <summary>
    /// Rolls back a profile module to a previous version.
    /// </summary>
    /// <param name="versionId">The version identifier to rollback to</param>
    /// <param name="rolledBackBy">The user performing the rollback</param>
    /// <param name="reason">Reason for the rollback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The new version record created by the rollback</returns>
    Task<ProfileModuleVersion> RollbackToVersionAsync(
        string versionId,
        string rolledBackBy,
        string? reason = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Compares two versions and returns the differences.
    /// </summary>
    /// <param name="version1Id">First version identifier</param>
    /// <param name="version2Id">Second version identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Version comparison result</returns>
    Task<VersionComparisonResult> CompareVersionsAsync(
        string version1Id,
        string version2Id,
        CancellationToken cancellationToken = default
    );
}
```

---

### IProfileVisibilityService

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Visibility;

/// <summary>
/// Service for managing profile visibility and privacy controls.
/// </summary>
public interface IProfileVisibilityService
{
    /// <summary>
    /// Checks if a user can view a specific profile module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="viewerUserId">The user requesting access</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user can view the module</returns>
    Task<bool> CanViewModuleAsync(
        string profileId,
        string moduleName,
        string viewerUserId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if a user can modify a specific profile module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="modifierUserId">The user requesting modification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user can modify the module</returns>
    Task<bool> CanModifyModuleAsync(
        string profileId,
        string moduleName,
        string modifierUserId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Applies visibility filtering to module data based on viewer permissions.
    /// </summary>
    /// <typeparam name="TData">The module data type</typeparam>
    /// <param name="data">The module data</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="viewerUserId">The user viewing the data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered module data with private fields removed/masked</returns>
    Task<TData?> ApplyVisibilityFilterAsync<TData>(
        TData data,
        string moduleName,
        string viewerUserId,
        CancellationToken cancellationToken = default
    ) where TData : class;

    /// <summary>
    /// Sets the visibility level for a profile module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="visibility">The visibility level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetModuleVisibilityAsync(
        string profileId,
        string moduleName,
        ProfileModuleVisibility visibility,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets the visibility level for a specific field within a module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="fieldPath">The field path (e.g., "Address.Street")</param>
    /// <param name="visibility">The visibility level</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SetFieldVisibilityAsync(
        string profileId,
        string moduleName,
        string fieldPath,
        ProfileModuleVisibility visibility,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets visibility settings for a profile module.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="moduleName">The module name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Visibility settings</returns>
    Task<ProfileModuleVisibilitySettings> GetVisibilitySettingsAsync(
        string profileId,
        string moduleName,
        CancellationToken cancellationToken = default
    );
}
```

---

### IProfileExportService

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Export;

/// <summary>
/// Service for exporting profile data in various formats.
/// </summary>
public interface IProfileExportService
{
    /// <summary>
    /// Exports a profile to JSON format.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="options">Export options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JSON string</returns>
    Task<string> ExportToJsonAsync(
        string profileId,
        ProfileExportOptions options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Exports a profile to XML format (GDPR Article 20 compliance).
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="options">Export options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>XML string</returns>
    Task<string> ExportToXmlAsync(
        string profileId,
        ProfileExportOptions options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Exports multiple profiles to CSV format (tabular data).
    /// </summary>
    /// <param name="profileIds">The profile identifiers</param>
    /// <param name="moduleNames">The module names to include</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>CSV content stream</returns>
    Task<Stream> ExportToCsvAsync(
        IReadOnlyList<string> profileIds,
        IReadOnlyList<string> moduleNames,
        CancellationToken cancellationToken = default
    );
}
```

---

### IProfileImportService

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Import;

/// <summary>
/// Service for importing profile data from various formats.
/// </summary>
public interface IProfileImportService
{
    /// <summary>
    /// Imports a profile from JSON format.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="json">The JSON content</param>
    /// <param name="options">Import options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with success/failure details</returns>
    Task<ProfileImportResult> ImportFromJsonAsync(
        string profileId,
        string json,
        ProfileImportOptions options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Imports a profile from XML format.
    /// </summary>
    /// <param name="profileId">The profile identifier</param>
    /// <param name="xml">The XML content</param>
    /// <param name="options">Import options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with success/failure details</returns>
    Task<ProfileImportResult> ImportFromXmlAsync(
        string profileId,
        string xml,
        ProfileImportOptions options,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Validates import data without actually importing.
    /// </summary>
    /// <param name="json">The JSON content to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ProfileImportResult> ValidateImportAsync(
        string json,
        CancellationToken cancellationToken = default
    );
}
```

---

## Data Models

### ProfileModuleMetadata

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement;

/// <summary>
/// Metadata for a profile module.
/// </summary>
public record ProfileModuleMetadata
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string Category { get; init; }
    public required Version Version { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<string> Dependencies { get; init; }
    public required ProfileModuleVisibility DefaultVisibility { get; init; }
    public required Type DataType { get; init; }
    public required Type StorageProviderType { get; init; }
    public Type? BlobStorageProviderType { get; init; }
}
```

---

### ProfileSnapshot

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement;

/// <summary>
/// Snapshot of a complete profile with all active modules.
/// </summary>
public record ProfileSnapshot
{
    public required string ProfileId { get; init; }
    public required ProfileType ProfileType { get; init; }
    public required DateTime LastModified { get; init; }
    public required IReadOnlyDictionary<string, object> Modules { get; init; }
}
```

---

### ProfileType

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement;

/// <summary>
/// Type of profile.
/// </summary>
public enum ProfileType
{
    User,
    Organization,
    Team,
    Custom
}
```

---

### ProfileModuleVisibility

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Visibility;

/// <summary>
/// Visibility level for profile modules and fields.
/// </summary>
public enum ProfileModuleVisibility
{
    /// <summary>
    /// Only the profile owner can view.
    /// </summary>
    Private = 0,

    /// <summary>
    /// Profile owner and team members can view.
    /// </summary>
    Team = 1,

    /// <summary>
    /// All organization members can view.
    /// </summary>
    Organization = 2,

    /// <summary>
    /// All authenticated users can view.
    /// </summary>
    Authenticated = 3,

    /// <summary>
    /// Anyone can view (including anonymous users).
    /// </summary>
    Public = 4
}
```

---

### ProfileModuleVersion

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Versioning;

/// <summary>
/// Version record for a profile module change.
/// </summary>
public record ProfileModuleVersion
{
    public required string Id { get; init; }
    public required string ProfileId { get; init; }
    public required string ModuleName { get; init; }
    public required int VersionNumber { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string CreatedBy { get; init; }
    public string? CreatedByIpAddress { get; init; }
    public string? CreatedByUserAgent { get; init; }
    public string? ChangeReason { get; init; }
    public required string DataJson { get; init; }
    public required ProfileModuleMetadata Metadata { get; init; }
}
```

---

### ProfileCompletenessScore

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement;

/// <summary>
/// Completeness score for a profile.
/// </summary>
public record ProfileCompletenessScore
{
    public required string ProfileId { get; init; }
    public required int Score { get; init; } // 0-100
    public required IReadOnlyDictionary<string, ModuleCompletenessScore> ModuleScores { get; init; }
    public required IReadOnlyList<string> MissingRequiredModules { get; init; }
    public required IReadOnlyList<string> IncompleteSuggestedModules { get; init; }
}

public record ModuleCompletenessScore
{
    public required string ModuleName { get; init; }
    public required int Score { get; init; } // 0-100
    public required IReadOnlyList<string> MissingRequiredFields { get; init; }
    public required IReadOnlyList<string> MissingOptionalFields { get; init; }
}
```

---

### ValidationResult

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Validation;

/// <summary>
/// Result of profile module validation.
/// </summary>
public record ValidationResult
{
    public required bool IsValid { get; init; }
    public required IReadOnlyList<ValidationError> Errors { get; init; }
    public required IReadOnlyList<ValidationWarning> Warnings { get; init; }

    public static ValidationResult Success() => new()
    {
        IsValid = true,
        Errors = Array.Empty<ValidationError>(),
        Warnings = Array.Empty<ValidationWarning>()
    };

    public static ValidationResult Failure(params ValidationError[] errors) => new()
    {
        IsValid = false,
        Errors = errors,
        Warnings = Array.Empty<ValidationWarning>()
    };

    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var errors = results.SelectMany(r => r.Errors).ToArray();
        var warnings = results.SelectMany(r => r.Warnings).ToArray();

        return new ValidationResult
        {
            IsValid = errors.Length == 0,
            Errors = errors,
            Warnings = warnings
        };
    }
}

public record ValidationError(
    string PropertyPath,
    string ErrorCode,
    string Message,
    object? AttemptedValue
);

public record ValidationWarning(
    string PropertyPath,
    string WarningCode,
    string Message
);
```

---

### ValidationContext

```csharp
namespace OoBDev.Framework.Identity.ProfileManagement.Validation;

/// <summary>
/// Context for validation operations.
/// </summary>
public record ValidationContext
{
    public required string ProfileId { get; init; }
    public required string ModuleName { get; init; }
    public required bool IsCreate { get; init; }
    public required IReadOnlyDictionary<string, object> AdditionalData { get; init; }
}
```

---

## Extension Methods

### IServiceCollection Extensions

```csharp
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering profile management services.
/// </summary>
public static class ProfileManagementServiceCollectionExtensions
{
    /// <summary>
    /// Adds profile management services to the DI container.
    /// </summary>
    public static IServiceCollection AddProfileManagement(
        this IServiceCollection services,
        Action<ProfileManagementOptions>? configureOptions = null)
    {
        // Register options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        // Register core services
        services.TryAddSingleton<IProfileModuleRegistry, ProfileModuleRegistry>();
        services.TryAddScoped<IProfileService, ProfileService>();
        services.TryAddScoped<IProfileVersioningService, ProfileVersioningService>();
        services.TryAddScoped<IProfileVisibilityService, ProfileVisibilityService>();
        services.TryAddScoped<IProfileExportService, ProfileExportService>();
        services.TryAddScoped<IProfileImportService, ProfileImportService>();

        return services;
    }

    /// <summary>
    /// Adds a profile storage provider.
    /// </summary>
    public static IServiceCollection AddProfileStorageProvider<TProvider>(
        this IServiceCollection services,
        Action<object>? configureOptions = null)
        where TProvider : class, IProfileModuleStorageProvider
    {
        services.TryAddScoped<IProfileModuleStorageProvider, TProvider>();

        return services;
    }

    /// <summary>
    /// Adds a blob storage provider for profile binary data.
    /// </summary>
    public static IServiceCollection AddProfileBlobStorage<TProvider>(
        this IServiceCollection services,
        Action<object>? configureOptions = null)
        where TProvider : class, IProfileBlobStorageProvider
    {
        services.TryAddScoped<IProfileBlobStorageProvider, TProvider>();

        return services;
    }

    /// <summary>
    /// Registers all built-in profile modules.
    /// </summary>
    public static IServiceCollection AddBuiltInProfileModules(
        this IServiceCollection services)
    {
        services.AddProfileModule<UserProfileCoreModule, UserProfileCoreData>();
        services.AddProfileModule<OrganizationProfileCoreModule, OrganizationProfileCoreData>();
        services.AddProfileModule<ContactPreferencesModule, ContactPreferencesData>();
        services.AddProfileModule<SocialLinksModule, SocialLinksData>();

        return services;
    }

    /// <summary>
    /// Registers a custom profile module.
    /// </summary>
    public static IServiceCollection AddProfileModule<TModule, TData>(
        this IServiceCollection services,
        Action<ProfileModuleRegistrationOptions>? configureOptions = null)
        where TModule : class, IProfileModule<TData>
        where TData : class, new()
    {
        services.TryAddTransient<IProfileModule<TData>, TModule>();

        if (configureOptions != null)
        {
            var options = new ProfileModuleRegistrationOptions();
            configureOptions(options);
            // Store options for module configuration
        }

        return services;
    }
}
```

---

## Usage Examples

### Example 1: Basic Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Framework.Identity.ProfileManagement;
using OoBDev.Framework.Identity.ProfileManagement.Storage;

// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add profile management with options
    services.AddProfileManagement(options =>
    {
        options.DefaultCacheTtl = TimeSpan.FromMinutes(5);
        options.EnableVersioning = true;
        options.EnableAuditLog = true;
    });

    // Add SQL storage provider
    services.AddProfileStorageProvider<SqlProfileStorageProvider>(options =>
    {
        // Configure SQL provider
    });

    // Add Azure Blob storage for avatars
    services.AddProfileBlobStorage<AzureBlobProfileStorageProvider>(options =>
    {
        // Configure blob storage
    });

    // Register built-in modules
    services.AddBuiltInProfileModules();
}
```

---

### Example 2: Save User Profile Core Data

```csharp
using OoBDev.Framework.Identity.ProfileManagement;

public class UserProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public UserProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpPut("api/profiles/{profileId}/core")]
    public async Task<IActionResult> UpdateUserCore(
        string profileId,
        [FromBody] UserProfileCoreData data,
        CancellationToken ct)
    {
        try
        {
            await _profileService.SaveModuleAsync(
                profileId,
                "UserCore",
                data,
                changeReason: "User updated profile",
                cancellationToken: ct
            );

            return NoContent();
        }
        catch (ProfileModuleValidationException ex)
        {
            return BadRequest(new
            {
                errors = ex.ValidationErrors.Select(e => new
                {
                    field = e.PropertyPath,
                    code = e.ErrorCode,
                    message = e.Message
                })
            });
        }
        catch (UnauthorizedProfileAccessException)
        {
            return Forbid();
        }
    }
}
```

---

### Example 3: Get Profile Snapshot

```csharp
using OoBDev.Framework.Identity.ProfileManagement;

public class ProfileSnapshotService
{
    private readonly IProfileService _profileService;

    public ProfileSnapshotService(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<ProfileDto> GetUserProfileAsync(
        string userId,
        CancellationToken ct)
    {
        // Get complete profile snapshot
        var snapshot = await _profileService.GetProfileSnapshotAsync(
            userId,
            cancellationToken: ct
        );

        // Extract modules
        var userCore = snapshot.Modules.TryGetValue("UserCore", out var coreData)
            ? (UserProfileCoreData)coreData
            : null;

        var socialLinks = snapshot.Modules.TryGetValue("SocialLinks", out var socialData)
            ? (SocialLinksData)socialData
            : null;

        var contactPrefs = snapshot.Modules.TryGetValue("ContactPreferences", out var prefsData)
            ? (ContactPreferencesData)prefsData
            : null;

        // Map to DTO
        return new ProfileDto
        {
            Id = snapshot.ProfileId,
            FirstName = userCore?.FirstName,
            LastName = userCore?.LastName,
            Email = userCore?.Email,
            Avatar = userCore?.AvatarUrl,
            LinkedIn = socialLinks?.LinkedIn,
            Twitter = socialLinks?.Twitter,
            AllowEmailNotifications = contactPrefs?.AllowEmail ?? true,
            LastModified = snapshot.LastModified
        };
    }
}
```

---

### Example 4: Calculate Profile Completeness

```csharp
using OoBDev.Framework.Identity.ProfileManagement;

public class ProfileCompletenessService
{
    private readonly IProfileService _profileService;

    public ProfileCompletenessService(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<CompletenessReportDto> GetCompletenessReportAsync(
        string profileId,
        CancellationToken ct)
    {
        var score = await _profileService.GetCompletenessScoreAsync(
            profileId,
            cancellationToken: ct
        );

        return new CompletenessReportDto
        {
            ProfileId = score.ProfileId,
            OverallScore = score.Score,
            MissingModules = score.MissingRequiredModules,
            ModuleScores = score.ModuleScores.Select(kvp => new ModuleScoreDto
            {
                ModuleName = kvp.Key,
                Score = kvp.Value.Score,
                MissingRequiredFields = kvp.Value.MissingRequiredFields,
                MissingOptionalFields = kvp.Value.MissingOptionalFields
            }).ToList(),
            Recommendations = GenerateRecommendations(score)
        };
    }

    private List<string> GenerateRecommendations(ProfileCompletenessScore score)
    {
        var recommendations = new List<string>();

        if (score.Score < 50)
        {
            recommendations.Add("Your profile is less than 50% complete. Consider adding more information.");
        }

        foreach (var missing in score.MissingRequiredModules)
        {
            recommendations.Add($"Add {missing} information to improve your profile.");
        }

        return recommendations;
    }
}
```

---

### Example 5: Create Custom Profile Module

```csharp
using OoBDev.Framework.Identity.ProfileManagement;
using OoBDev.Framework.Identity.ProfileManagement.Validation;

// Data model
public record UserSkillsData
{
    public IReadOnlyList<Skill> Skills { get; init; } = [];
    public IReadOnlyList<string> Certifications { get; init; } = [];
    public int YearsOfExperience { get; init; }
}

public record Skill
{
    public string Name { get; init; } = null!;
    public SkillLevel Level { get; init; }
    public int YearsOfExperience { get; init; }
}

public enum SkillLevel
{
    Beginner,
    Intermediate,
    Advanced,
    Expert
}

// Module implementation
public class UserSkillsModule : IProfileModule<UserSkillsData>
{
    public string Name => "UserSkills";
    public string Category => ProfileModuleCategories.Professional;
    public Version Version => new(1, 0, 0);
    public IReadOnlyList<string> Dependencies => new[] { "UserCore" };

    public async Task<ValidationResult> ValidateAsync(
        UserSkillsData data,
        ValidationContext context,
        CancellationToken cancellationToken)
    {
        var errors = new List<ValidationError>();

        // Validate years of experience
        if (data.YearsOfExperience < 0)
        {
            errors.Add(new ValidationError(
                nameof(data.YearsOfExperience),
                "INVALID_RANGE",
                "Years of experience cannot be negative",
                data.YearsOfExperience
            ));
        }

        // Validate skills
        foreach (var skill in data.Skills)
        {
            if (string.IsNullOrWhiteSpace(skill.Name))
            {
                errors.Add(new ValidationError(
                    $"{nameof(data.Skills)}.{nameof(skill.Name)}",
                    "REQUIRED",
                    "Skill name is required",
                    skill.Name
                ));
            }

            if (skill.YearsOfExperience > data.YearsOfExperience)
            {
                errors.Add(new ValidationError(
                    $"{nameof(data.Skills)}.{nameof(skill.YearsOfExperience)}",
                    "INVALID_RANGE",
                    "Skill experience cannot exceed total years of experience",
                    skill.YearsOfExperience
                ));
            }
        }

        return errors.Count > 0
            ? ValidationResult.Failure(errors.ToArray())
            : ValidationResult.Success();
    }

    public Task<UserSkillsData> GetDefaultDataAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(new UserSkillsData
        {
            Skills = Array.Empty<Skill>(),
            Certifications = Array.Empty<string>(),
            YearsOfExperience = 0
        });
    }
}

// Registration
services.AddProfileModule<UserSkillsModule, UserSkillsData>(options =>
{
    options.StorageProvider = typeof(NoSqlProfileStorageProvider);
    options.DefaultVisibility = ProfileModuleVisibility.Organization;
});
```

---

### Example 6: Profile Versioning and Rollback

```csharp
using OoBDev.Framework.Identity.ProfileManagement.Versioning;

public class ProfileHistoryService
{
    private readonly IProfileVersioningService _versioningService;

    public ProfileHistoryService(IProfileVersioningService versioningService)
    {
        _versioningService = versioningService;
    }

    public async Task<IReadOnlyList<VersionHistoryDto>> GetHistoryAsync(
        string profileId,
        string moduleName,
        CancellationToken ct)
    {
        var versions = await _versioningService.GetVersionHistoryAsync(
            profileId,
            moduleName,
            cancellationToken: ct
        );

        return versions.Select(v => new VersionHistoryDto
        {
            VersionId = v.Id,
            VersionNumber = v.VersionNumber,
            CreatedAt = v.CreatedAt,
            CreatedBy = v.CreatedBy,
            ChangeReason = v.ChangeReason
        }).ToList();
    }

    public async Task RollbackAsync(
        string versionId,
        string userId,
        string reason,
        CancellationToken ct)
    {
        await _versioningService.RollbackToVersionAsync(
            versionId,
            userId,
            reason,
            cancellationToken: ct
        );
    }

    public async Task<VersionDiffDto> CompareVersionsAsync(
        string version1Id,
        string version2Id,
        CancellationToken ct)
    {
        var comparison = await _versioningService.CompareVersionsAsync(
            version1Id,
            version2Id,
            cancellationToken: ct
        );

        // Map to DTO
        return new VersionDiffDto
        {
            Version1 = comparison.Version1,
            Version2 = comparison.Version2,
            Changes = comparison.Changes
        };
    }
}
```

---

### Example 7: Profile Visibility Management

```csharp
using OoBDev.Framework.Identity.ProfileManagement.Visibility;

public class ProfilePrivacyService
{
    private readonly IProfileVisibilityService _visibilityService;

    public ProfilePrivacyService(IProfileVisibilityService visibilityService)
    {
        _visibilityService = visibilityService;
    }

    public async Task SetModulePrivacyAsync(
        string profileId,
        string moduleName,
        ProfileModuleVisibility visibility,
        CancellationToken ct)
    {
        await _visibilityService.SetModuleVisibilityAsync(
            profileId,
            moduleName,
            visibility,
            cancellationToken: ct
        );
    }

    public async Task SetFieldPrivacyAsync(
        string profileId,
        string moduleName,
        string fieldPath,
        ProfileModuleVisibility visibility,
        CancellationToken ct)
    {
        await _visibilityService.SetFieldVisibilityAsync(
            profileId,
            moduleName,
            fieldPath,
            visibility,
            cancellationToken: ct
        );
    }

    public async Task<TData?> GetModuleForViewerAsync<TData>(
        string profileId,
        string moduleName,
        string viewerId,
        CancellationToken ct)
        where TData : class
    {
        // Check if viewer can see module
        var canView = await _visibilityService.CanViewModuleAsync(
            profileId,
            moduleName,
            viewerId,
            cancellationToken: ct
        );

        if (!canView)
        {
            return null;
        }

        // Get module data (from profile service)
        var data = await GetModuleDataAsync<TData>(profileId, moduleName, ct);

        // Apply field-level filtering
        var filtered = await _visibilityService.ApplyVisibilityFilterAsync(
            data,
            moduleName,
            viewerId,
            cancellationToken: ct
        );

        return filtered;
    }

    private async Task<TData> GetModuleDataAsync<TData>(
        string profileId,
        string moduleName,
        CancellationToken ct)
        where TData : class
    {
        // Implementation to get module data
        throw new NotImplementedException();
    }
}
```

---

### Example 8: Profile Export (GDPR Compliance)

```csharp
using OoBDev.Framework.Identity.ProfileManagement.Export;

public class GdprComplianceService
{
    private readonly IProfileExportService _exportService;

    public GdprComplianceService(IProfileExportService exportService)
    {
        _exportService = exportService;
    }

    public async Task<string> ExportUserDataAsync(
        string userId,
        CancellationToken ct)
    {
        // Export to JSON (GDPR Article 20 - Right to Data Portability)
        var json = await _exportService.ExportToJsonAsync(
            userId,
            new ProfileExportOptions(
                ModuleNames: null, // All modules
                IncludeVersionHistory: true,
                IncludeBlobs: true,
                Format: ProfileExportFormat.Json
            ),
            cancellationToken: ct
        );

        return json;
    }

    public async Task<string> ExportUserDataXmlAsync(
        string userId,
        CancellationToken ct)
    {
        // Export to XML (machine-readable format)
        var xml = await _exportService.ExportToXmlAsync(
            userId,
            new ProfileExportOptions(
                ModuleNames: null,
                IncludeVersionHistory: false,
                IncludeBlobs: false,
                Format: ProfileExportFormat.Xml
            ),
            cancellationToken: ct
        );

        return xml;
    }

    public async Task<Stream> ExportBulkProfilesAsync(
        IReadOnlyList<string> userIds,
        CancellationToken ct)
    {
        // Export multiple profiles to CSV (for data migration)
        var csv = await _exportService.ExportToCsvAsync(
            userIds,
            new[] { "UserCore", "ContactPreferences", "SocialLinks" },
            cancellationToken: ct
        );

        return csv;
    }
}
```

---

### Example 9: Profile Import and Migration

```csharp
using OoBDev.Framework.Identity.ProfileManagement.Import;

public class ProfileMigrationService
{
    private readonly IProfileImportService _importService;

    public ProfileMigrationService(IProfileImportService importService)
    {
        _importService = importService;
    }

    public async Task<ProfileImportResult> ImportUserProfileAsync(
        string userId,
        string jsonData,
        CancellationToken ct)
    {
        // Validate import data first
        var validationResult = await _importService.ValidateImportAsync(
            jsonData,
            cancellationToken: ct
        );

        if (!validationResult.Success)
        {
            return validationResult;
        }

        // Import profile
        var result = await _importService.ImportFromJsonAsync(
            userId,
            jsonData,
            new ProfileImportOptions(
                ConflictResolution: ProfileImportConflictResolution.Overwrite,
                ValidateOnly: false,
                CreateMissingModules: true
            ),
            cancellationToken: ct
        );

        return result;
    }

    public async Task<BulkImportResult> BulkImportProfilesAsync(
        IReadOnlyList<(string UserId, string JsonData)> profiles,
        CancellationToken ct)
    {
        var results = new List<ProfileImportResult>();

        foreach (var (userId, jsonData) in profiles)
        {
            try
            {
                var result = await ImportUserProfileAsync(userId, jsonData, ct);
                results.Add(result);
            }
            catch (Exception ex)
            {
                results.Add(new ProfileImportResult
                {
                    Success = false,
                    Errors = new[] { $"Import failed: {ex.Message}" }
                });
            }
        }

        return new BulkImportResult
        {
            TotalProfiles = profiles.Count,
            SuccessCount = results.Count(r => r.Success),
            FailureCount = results.Count(r => !r.Success),
            Results = results
        };
    }
}
```

---

## Controller Examples

### ProfilesController

```csharp
using Microsoft.AspNetCore.Mvc;
using OoBDev.Framework.Identity.ProfileManagement;

[ApiController]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IProfileVisibilityService _visibilityService;

    public ProfilesController(
        IProfileService profileService,
        IProfileVisibilityService visibilityService)
    {
        _profileService = profileService;
        _visibilityService = visibilityService;
    }

    [HttpGet("{profileId}")]
    public async Task<ActionResult<ProfileSnapshot>> GetProfile(
        string profileId,
        CancellationToken ct)
    {
        var snapshot = await _profileService.GetProfileSnapshotAsync(profileId, ct);
        return Ok(snapshot);
    }

    [HttpGet("{profileId}/modules/{moduleName}")]
    public async Task<ActionResult<object>> GetModule(
        string profileId,
        string moduleName,
        CancellationToken ct)
    {
        var data = await _profileService.GetModuleAsync<object>(
            profileId,
            moduleName,
            ct
        );

        if (data == null)
        {
            return NotFound();
        }

        return Ok(data);
    }

    [HttpPut("{profileId}/modules/{moduleName}")]
    public async Task<IActionResult> SaveModule(
        string profileId,
        string moduleName,
        [FromBody] object data,
        [FromQuery] string? changeReason,
        CancellationToken ct)
    {
        await _profileService.SaveModuleAsync(
            profileId,
            moduleName,
            data,
            changeReason,
            ct
        );

        return NoContent();
    }

    [HttpDelete("{profileId}/modules/{moduleName}")]
    public async Task<IActionResult> DeleteModule(
        string profileId,
        string moduleName,
        CancellationToken ct)
    {
        await _profileService.DeleteModuleAsync(profileId, moduleName, ct);
        return NoContent();
    }

    [HttpGet("{profileId}/completeness")]
    public async Task<ActionResult<ProfileCompletenessScore>> GetCompleteness(
        string profileId,
        CancellationToken ct)
    {
        var score = await _profileService.GetCompletenessScoreAsync(profileId, ct);
        return Ok(score);
    }

    [HttpGet("modules")]
    public async Task<ActionResult<IReadOnlyList<ProfileModuleMetadata>>> GetAvailableModules(
        CancellationToken ct)
    {
        var modules = await _profileService.GetAvailableModulesAsync(ct);
        return Ok(modules);
    }
}
```

---

## Summary

This API design provides:

1. **Clean Abstractions**: Well-defined interfaces for all core services
2. **Type Safety**: Generic types ensure compile-time validation
3. **Extensibility**: Easy to add custom modules via DI
4. **Flexibility**: Multiple storage providers, visibility controls, versioning
5. **Completeness**: Export/import, validation, audit trail
6. **Best Practices**: Async/await, cancellation tokens, error handling
7. **Real-World Examples**: 9 comprehensive usage examples covering all scenarios

The API follows OoBDev architectural standards and integrates seamlessly with ASP.NET Core dependency injection.
