# Modular Profile Management - Architecture Design

**Epic:** 07 - Identity & Session Management
**Feature:** 04 - Modular Profile Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Architecture Overview

The Modular Profile Management system provides an extensible framework for managing user and organizational profiles through composable, independently validated, and versioned profile modules. The architecture emphasizes flexibility, type safety, and storage abstraction while maintaining comprehensive audit trails.

### Design Principles

1. **Modularity**: Profile data organized into independent, composable modules
2. **Extensibility**: New modules added via dependency injection without core changes
3. **Type Safety**: Strongly-typed module data with compile-time validation
4. **Storage Flexibility**: Multiple storage backends (SQL, NoSQL, blob) per module
5. **Version Everything**: Complete audit trail of all profile changes
6. **Privacy by Design**: Granular visibility controls on modules and fields
7. **Validation Independence**: Each module validates its own data independently
8. **Performance**: Lazy loading, caching, and optimized storage queries

---

## System Context

```
┌─────────────────────────────────────────────────────────────────┐
│                      Client Applications                         │
│  (Web UI, Mobile Apps, Admin Portals, Third-party Integrations) │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ HTTPS / REST / GraphQL
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Profile Service Layer                         │
│  ┌──────────────┬──────────────┬──────────────┬──────────────┐  │
│  │   Profile    │   Versioning │  Visibility  │  Import/     │  │
│  │   Service    │   Service    │  Service     │  Export      │  │
│  └──────┬───────┴──────┬───────┴──────┬───────┴──────┬───────┘  │
└─────────┼──────────────┼──────────────┼──────────────┼──────────┘
          │              │              │              │
          ▼              ▼              ▼              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Profile Module Registry                       │
│  ┌──────────┬──────────┬──────────┬──────────┬──────────────┐   │
│  │   User   │   Org    │  Social  │ Contact  │   Custom     │   │
│  │   Core   │   Core   │  Links   │  Prefs   │   Modules    │   │
│  └────┬─────┴────┬─────┴────┬─────┴────┬─────┴──────┬───────┘   │
└───────┼──────────┼──────────┼──────────┼────────────┼───────────┘
        │          │          │          │            │
        ▼          ▼          ▼          ▼            ▼
┌─────────────────────────────────────────────────────────────────┐
│                  Storage Provider Abstraction                    │
│  ┌──────────────┬──────────────┬──────────────┬──────────────┐  │
│  │   SQL        │   NoSQL      │   Blob       │   Hybrid     │  │
│  │   Provider   │   Provider   │   Storage    │   Provider   │  │
│  └──────┬───────┴──────┬───────┴──────┬───────┴──────┬───────┘  │
└─────────┼──────────────┼──────────────┼──────────────┼──────────┘
          │              │              │              │
          ▼              ▼              ▼              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Physical Storage                            │
│  ┌──────────────┬──────────────┬──────────────┬──────────────┐  │
│  │  SQL Server/ │   MongoDB/   │  Azure Blob/ │   Redis      │  │
│  │  PostgreSQL  │   CosmosDB   │  AWS S3      │   Cache      │  │
│  └──────────────┴──────────────┴──────────────┴──────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### External Dependencies

- **Identity System**: User/organization IDs, authentication context
- **Caching Layer**: Module data caching, completeness score caching
- **Blob Storage**: Avatars, documents, large binary attachments
- **Search Engine**: Profile discovery and full-text search (optional)
- **Analytics Service**: Profile metrics and insights (optional)

---

## Component Architecture

### Core Components

```
┌────────────────────────────────────────────────────────────────────┐
│                         Profile Service                             │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileService                                               │  │
│  │                                                               │  │
│  │  - GetModuleAsync<TData>(profileId, moduleName)              │  │
│  │  - SaveModuleAsync<TData>(profileId, moduleName, data)       │  │
│  │  - DeleteModuleAsync(profileId, moduleName)                  │  │
│  │  - GetProfileSnapshotAsync(profileId)                        │  │
│  │  - GetCompletenessScoreAsync(profileId)                      │  │
│  │  - GetAvailableModulesAsync()                                │  │
│  │  - GetActiveModulesAsync(profileId)                          │  │
│  └───────────┬──────────────────────────────────────────────────┘  │
│              │                                                      │
│              │ Uses                                                 │
│              ▼                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileModuleRegistry                                        │  │
│  │                                                               │  │
│  │  - GetModule(moduleName)                                      │  │
│  │  - GetAllModules()                                            │  │
│  │  - IsModuleActive(profileId, moduleName)                     │  │
│  │  - ActivateModule(profileId, moduleName)                     │  │
│  │  - DeactivateModule(profileId, moduleName)                   │  │
│  └───────────┬──────────────────────────────────────────────────┘  │
└──────────────┼─────────────────────────────────────────────────────┘
               │
               │ Coordinates
               ▼
┌────────────────────────────────────────────────────────────────────┐
│                    Profile Module Framework                         │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileModule<TData>                                         │  │
│  │                                                               │  │
│  │  + Name: string                                               │  │
│  │  + Category: string                                           │  │
│  │  + Version: Version                                           │  │
│  │  + Dependencies: IReadOnlyList<string>                        │  │
│  │                                                               │  │
│  │  - ValidateAsync(data, ct): ValidationResult                  │  │
│  │  - GetDefaultDataAsync(ct): TData                             │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  Built-In Modules:                                                  │
│  ┌────────────┬────────────┬────────────┬────────────┐             │
│  │ UserProfile│   OrgCore  │  Social    │  Contact   │             │
│  │    Core    │   Module   │  Links     │  Prefs     │             │
│  └────────────┴────────────┴────────────┴────────────┘             │
└────────────────────────────────────────────────────────────────────┘
```

### Storage Layer

```
┌────────────────────────────────────────────────────────────────────┐
│                    Storage Provider Layer                           │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileModuleStorageProvider                                 │  │
│  │                                                               │  │
│  │  - GetAsync<TData>(profileId, moduleName)                    │  │
│  │  - SaveAsync<TData>(profileId, moduleName, data, metadata)   │  │
│  │  - DeleteAsync(profileId, moduleName)                        │  │
│  │  - GetVersionHistoryAsync(profileId, moduleName)             │  │
│  └───────────┬──────────────────────────────────────────────────┘  │
│              │                                                      │
│              │ Implemented By                                       │
│              ▼                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Concrete Providers:                                           │  │
│  │                                                               │  │
│  │  SqlProfileStorageProvider       (EF Core, relational)       │  │
│  │  MongoProfileStorageProvider     (MongoDB, flexible schema)  │  │
│  │  CosmosProfileStorageProvider    (CosmosDB, global)          │  │
│  │  HybridProfileStorageProvider    (metadata+blob split)       │  │
│  │  InMemoryProfileStorageProvider  (testing only)              │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileBlobStorageProvider                                   │  │
│  │                                                               │  │
│  │  - GetBlobAsync(profileId, moduleName, blobKey)              │  │
│  │  - SaveBlobAsync(profileId, moduleName, blobKey, stream)     │  │
│  │  - DeleteBlobAsync(profileId, moduleName, blobKey)           │  │
│  └───────────┬──────────────────────────────────────────────────┘  │
│              │                                                      │
│              │ Implemented By                                       │
│              ▼                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Concrete Blob Providers:                                      │  │
│  │                                                               │  │
│  │  AzureBlobProfileStorageProvider    (Azure Blob Storage)     │  │
│  │  S3ProfileBlobStorageProvider       (AWS S3)                 │  │
│  │  FileSystemBlobStorageProvider      (local files, dev only)  │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
```

### Versioning and Audit

```
┌────────────────────────────────────────────────────────────────────┐
│                     Versioning System                               │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileVersioningService                                     │  │
│  │                                                               │  │
│  │  - CreateVersionAsync<TData>(profileId, moduleName, data)    │  │
│  │  - GetVersionHistoryAsync(profileId, moduleName)             │  │
│  │  - GetVersionDataAsync<TData>(versionId)                     │  │
│  │  - RollbackToVersionAsync(versionId, reason)                 │  │
│  │  - CompareVersionsAsync(version1Id, version2Id)              │  │
│  └───────────┬──────────────────────────────────────────────────┘  │
│              │                                                      │
│              │ Creates                                              │
│              ▼                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ ProfileModuleVersion (Record)                                 │  │
│  │                                                               │  │
│  │  + Id: string                                                 │  │
│  │  + ProfileId: string                                          │  │
│  │  + ModuleName: string                                         │  │
│  │  + VersionNumber: int                                         │  │
│  │  + CreatedAt: DateTime                                        │  │
│  │  + CreatedBy: string                                          │  │
│  │  + CreatedByIpAddress: string?                                │  │
│  │  + CreatedByUserAgent: string?                                │  │
│  │  + ChangeReason: string?                                      │  │
│  │  + DataJson: string (complete snapshot)                       │  │
│  │  + Metadata: ProfileModuleMetadata                            │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  Storage Strategy:                                                  │
│  - Versions stored in separate partition/table                     │
│  - Indexed by (ProfileId, ModuleName, VersionNumber)               │
│  - Data stored as JSON for flexibility                             │
│  - Retention policy: configurable per tenant                       │
│  - Archival: move old versions to cold storage                     │
└────────────────────────────────────────────────────────────────────┘
```

### Validation Framework

```
┌────────────────────────────────────────────────────────────────────┐
│                    Validation Framework                             │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileModuleValidator<TData>                                │  │
│  │                                                               │  │
│  │  - ValidateAsync(data, context, ct): ValidationResult         │  │
│  └───────────┬──────────────────────────────────────────────────┘  │
│              │                                                      │
│              │ Implemented By                                       │
│              ▼                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Built-In Validators:                                          │  │
│  │                                                               │  │
│  │  RequiredFieldValidator<TData>                                │  │
│  │  RegexValidator<TData>                                        │  │
│  │  RangeValidator<TData>                                        │  │
│  │  LengthValidator<TData>                                       │  │
│  │  EmailValidator<TData>                                        │  │
│  │  PhoneNumberValidator<TData>                                  │  │
│  │  UrlValidator<TData>                                          │  │
│  │  UniqueValueValidator<TData>    (async, database query)      │  │
│  │  CompositeValidator<TData>      (combines validators)        │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ ValidationResult (Record)                                     │  │
│  │                                                               │  │
│  │  + IsValid: bool                                              │  │
│  │  + Errors: IReadOnlyList<ValidationError>                     │  │
│  │  + Warnings: IReadOnlyList<ValidationWarning>                 │  │
│  │                                                               │  │
│  │  Static Methods:                                              │  │
│  │  - Success(): ValidationResult                                │  │
│  │  - Failure(errors): ValidationResult                          │  │
│  │  - Combine(results): ValidationResult                         │  │
│  └──────────────────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────────────────┘
```

### Visibility and Privacy

```
┌────────────────────────────────────────────────────────────────────┐
│                   Visibility Service                                │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ IProfileVisibilityService                                     │  │
│  │                                                               │  │
│  │  - CanViewModuleAsync(profileId, moduleName, viewerId)       │  │
│  │  - ApplyVisibilityFilterAsync<TData>(data, viewerId)         │  │
│  │  - SetModuleVisibilityAsync(profileId, moduleName, level)    │  │
│  │  - SetFieldVisibilityAsync(profileId, field, level)          │  │
│  │  - GetVisibilitySettingsAsync(profileId, moduleName)         │  │
│  └───────────┬──────────────────────────────────────────────────┘  │
│              │                                                      │
│              │ Uses                                                 │
│              ▼                                                      │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ ProfileModuleVisibility (Enum)                                │  │
│  │                                                               │  │
│  │  Private        - Only profile owner                          │  │
│  │  Team           - Profile owner + team members                │  │
│  │  Organization   - All organization members                    │  │
│  │  Authenticated  - All authenticated users                     │  │
│  │  Public         - Anyone (including anonymous)                │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ ProfileFieldVisibility (Record)                               │  │
│  │                                                               │  │
│  │  + FieldPath: string                                          │  │
│  │  + Visibility: ProfileModuleVisibility                        │  │
│  │  + ExplicitUsers: IReadOnlyList<string>                       │  │
│  │  + ExplicitRoles: IReadOnlyList<string>                       │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  Visibility Resolution Algorithm:                                   │
│  1. Check module-level visibility                                  │
│  2. For fields, check field-level overrides                        │
│  3. Evaluate explicit user grants                                  │
│  4. Evaluate explicit role grants                                  │
│  5. Filter data based on viewer context                            │
└────────────────────────────────────────────────────────────────────┘
```

---

## Data Flow

### Profile Module Save Operation

```
Client Request (Save Profile Module)
         │
         │ 1. POST /api/profiles/{profileId}/modules/{moduleName}
         ▼
┌─────────────────────┐
│  Profile Controller │
│  (API Layer)        │
└──────────┬──────────┘
           │ 2. Deserialize TData, get user context
           ▼
┌─────────────────────┐
│  Profile Service    │
│  SaveModuleAsync    │
└──────────┬──────────┘
           │ 3. Get module from registry
           ▼
┌─────────────────────┐
│  Module Registry    │
│  GetModule()        │
└──────────┬──────────┘
           │ 4. Return IProfileModule<TData>
           ▼
┌─────────────────────┐
│  Profile Module     │
│  ValidateAsync()    │
└──────────┬──────────┘
           │ 5. Validation rules execute
           │    ├─ Required fields
           │    ├─ Format validation
           │    ├─ Cross-field rules
           │    └─ Async validators (DB checks)
           ▼
┌─────────────────────┐
│  Validation Result  │
│  IsValid: true/false│
└──────────┬──────────┘
           │ 6. If invalid, return errors to client
           │    If valid, continue
           ▼
┌─────────────────────┐
│  Visibility Service │
│  Check permissions  │
└──────────┬──────────┘
           │ 7. Verify user can modify module
           ▼
┌─────────────────────┐
│  Versioning Service │
│  CreateVersionAsync │
└──────────┬──────────┘
           │ 8. Create version record
           │    - Capture audit fields
           │    - Serialize current data
           │    - Store version metadata
           ▼
┌─────────────────────┐
│  Storage Provider   │
│  SaveAsync()        │
└──────────┬──────────┘
           │ 9. Persist to appropriate storage
           │    - SQL for structured data
           │    - NoSQL for flexible schema
           │    - Blob for binary data
           ▼
┌─────────────────────┐
│  Cache Invalidation │
└──────────┬──────────┘
           │ 10. Invalidate cached module data
           │     Invalidate completeness score
           ▼
┌─────────────────────┐
│  Event Publishing   │
│  (Optional)         │
└──────────┬──────────┘
           │ 11. Publish ProfileModuleUpdated event
           ▼
     Success Response
     (HTTP 200/204)
```

### Profile Snapshot Retrieval

```
Client Request (Get Full Profile)
         │
         │ 1. GET /api/profiles/{profileId}/snapshot
         ▼
┌─────────────────────┐
│  Profile Controller │
└──────────┬──────────┘
           │ 2. Get viewer context
           ▼
┌─────────────────────┐
│  Profile Service    │
│  GetSnapshotAsync   │
└──────────┬──────────┘
           │ 3. Get active modules for profile
           ▼
┌─────────────────────┐
│  Module Registry    │
│  GetActiveModules() │
└──────────┬──────────┘
           │ 4. Return list of module names
           │    [UserCore, ContactPrefs, SocialLinks, ...]
           ▼
┌─────────────────────────────────────┐
│  Parallel Module Loading             │
│                                     │
│  For each module:                   │
│  ┌───────────────────────────────┐  │
│  │ 1. Check cache               │  │
│  │    ├─ Hit: return cached     │  │
│  │    └─ Miss: load from storage│  │
│  └───────────────────────────────┘  │
│                                     │
│  ┌───────────────────────────────┐  │
│  │ 2. Check visibility           │  │
│  │    ├─ Can view: continue      │  │
│  │    └─ Cannot: skip module     │  │
│  └───────────────────────────────┘  │
│                                     │
│  ┌───────────────────────────────┐  │
│  │ 3. Apply field filtering      │  │
│  │    - Remove private fields    │  │
│  │    - Mask sensitive data      │  │
│  └───────────────────────────────┘  │
└─────────────┬───────────────────────┘
              │ 5. Aggregate results
              ▼
┌─────────────────────┐
│  Profile Snapshot   │
│  {                  │
│    ProfileId: "...",│
│    ProfileType: User│
│    LastModified: ...│
│    Modules: {       │
│      UserCore: {...}│
│      ContactPrefs:..│
│      SocialLinks:.. │
│    }                │
│  }                  │
└──────────┬──────────┘
           │ 6. Cache snapshot (short TTL)
           ▼
     JSON Response
     (HTTP 200)
```

### Profile Completeness Calculation

```
Completeness Score Request
         │
         │ 1. Calculate score for profile
         ▼
┌─────────────────────┐
│  Profile Service    │
│  GetCompleteness    │
└──────────┬──────────┘
           │ 2. Check cache
           │    Key: "completeness:{profileId}"
           ▼
     Cache Hit?
      ╱      ╲
    Yes       No
     │         │
     │         ▼
     │   ┌─────────────────────┐
     │   │  Module Registry    │
     │   │  GetRequiredModules │
     │   └──────────┬──────────┘
     │              │ 3. Get list of required modules
     │              │    and their required fields
     │              ▼
     │   ┌─────────────────────┐
     │   │  Load Modules       │
     │   │  (parallel)         │
     │   └──────────┬──────────┘
     │              │ 4. Load all required modules
     │              ▼
     │   ┌─────────────────────────────┐
     │   │  Calculate Score            │
     │   │                             │
     │   │  For each required module:  │
     │   │  ├─ Module exists? +weight  │
     │   │  ├─ Required fields:        │
     │   │  │  ├─ Populated? +points   │
     │   │  │  └─ Empty? +0            │
     │   │  └─ Optional fields:        │
     │   │     ├─ Populated? +bonus    │
     │   │     └─ Empty? +0            │
     │   │                             │
     │   │  Score = (points / total)   │
     │   │         * 100               │
     │   └──────────┬──────────────────┘
     │              │ 5. Cache result (10 min TTL)
     │              ▼
     └───────────>  Return Score
                    (0-100)
```

---

## Module Registration and Discovery

### Module Registration Flow

```csharp
// Startup.cs / Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // 1. Register core profile services
    services.AddProfileManagement(options =>
    {
        options.DefaultCacheTtl = TimeSpan.FromMinutes(5);
        options.EnableVersioning = true;
        options.EnableAuditLog = true;
    });

    // 2. Register storage providers
    services.AddProfileStorageProvider<SqlProfileStorageProvider>(options =>
    {
        options.ConnectionString = configuration.GetConnectionString("ProfileDb");
    });

    services.AddProfileBlobStorage<AzureBlobProfileStorageProvider>(options =>
    {
        options.ConnectionString = configuration["Azure:BlobStorage"];
        options.ContainerName = "profile-blobs";
    });

    // 3. Register built-in modules (auto-discovered)
    services.AddBuiltInProfileModules();

    // 4. Register custom modules
    services.AddProfileModule<UserSkillsModule, UserSkillsData>(options =>
    {
        options.StorageProvider = typeof(NoSqlProfileStorageProvider);
        options.DefaultVisibility = ProfileModuleVisibility.Organization;
    });

    services.AddProfileModule<CompanyBrandingModule, CompanyBrandingData>(options =>
    {
        options.StorageProvider = typeof(HybridProfileStorageProvider);
        options.BlobStorageProvider = typeof(AzureBlobProfileStorageProvider);
    });
}
```

### Module Discovery Process

```
Application Startup
         │
         │ 1. DI container initialization
         ▼
┌─────────────────────┐
│  Profile Module     │
│  Registry Builder   │
└──────────┬──────────┘
           │ 2. Scan for IProfileModule<TData> registrations
           ▼
┌─────────────────────────────────────┐
│  Module Discovery                   │
│                                     │
│  For each IProfileModule<TData>:    │
│  ┌───────────────────────────────┐  │
│  │ 1. Extract metadata           │  │
│  │    - Name                     │  │
│  │    - Category                 │  │
│  │    - Version                  │  │
│  │    - Dependencies             │  │
│  │    - Data type                │  │
│  │    - Storage provider         │  │
│  └───────────────────────────────┘  │
│                                     │
│  ┌───────────────────────────────┐  │
│  │ 2. Validate module            │  │
│  │    - No circular dependencies │  │
│  │    - Dependencies exist       │  │
│  │    - Storage provider valid   │  │
│  └───────────────────────────────┘  │
│                                     │
│  ┌───────────────────────────────┐  │
│  │ 3. Build dependency graph     │  │
│  │    - Topological sort         │  │
│  │    - Detect cycles            │  │
│  └───────────────────────────────┘  │
└─────────────┬───────────────────────┘
              │ 4. Register in registry
              ▼
┌─────────────────────┐
│  Module Registry    │
│  (Ready for use)    │
│                     │
│  Modules:           │
│  - UserCore         │
│  - OrgCore          │
│  - ContactPrefs     │
│  - SocialLinks      │
│  - UserSkills       │
│  - CompanyBranding  │
└─────────────────────┘
```

---

## Storage Provider Selection

### Storage Strategy Decision Tree

```
Profile Module Data
         │
         ▼
┌─────────────────────────────────┐
│ What type of data?              │
├─────────────────────────────────┤
│                                 │
│ Structured, relational?         │───Yes──> SQL Storage Provider
│  (normalize, joins, ACID)       │          (Entity Framework)
│                                 │
│ Flexible schema, high write?   │───Yes──> NoSQL Storage Provider
│  (frequent changes, scale)      │          (MongoDB, CosmosDB)
│                                 │
│ Binary data (images, docs)?    │───Yes──> Blob Storage Provider
│  (>1MB, streaming)              │          (Azure Blob, S3)
│                                 │
│ Hybrid (metadata + blobs)?     │───Yes──> Hybrid Storage Provider
│  (profile photo with metadata)  │          (SQL + Blob)
└─────────────────────────────────┘
```

### Storage Provider Configuration

```csharp
// Example: User Core Module (SQL)
public class UserProfileCoreModule : IProfileModule<UserProfileCoreData>
{
    public string Name => "UserCore";
    public string Category => ProfileModuleCategories.Personal;
    public Version Version => new(1, 0, 0);
    public IReadOnlyList<string> Dependencies => [];

    // Storage provider: SQL (structured, relational)
    public Type StorageProvider => typeof(SqlProfileStorageProvider);

    public async Task<ValidationResult> ValidateAsync(
        UserProfileCoreData data,
        CancellationToken ct)
    {
        var validator = new CompositeValidator<UserProfileCoreData>(
            new RequiredFieldValidator<UserProfileCoreData>(d => d.FirstName),
            new RequiredFieldValidator<UserProfileCoreData>(d => d.LastName),
            new EmailValidator<UserProfileCoreData>(d => d.Email),
            new UniqueValueValidator<UserProfileCoreData>(d => d.Email)
        );

        return await validator.ValidateAsync(data, ct);
    }

    public Task<UserProfileCoreData> GetDefaultDataAsync(CancellationToken ct)
    {
        return Task.FromResult(new UserProfileCoreData());
    }
}

// Example: Company Branding Module (Hybrid: SQL + Blob)
public class CompanyBrandingModule : IProfileModule<CompanyBrandingData>
{
    public string Name => "CompanyBranding";
    public string Category => ProfileModuleCategories.Organization;
    public Version Version => new(1, 0, 0);
    public IReadOnlyList<string> Dependencies => ["OrgCore"];

    // Storage provider: Hybrid (metadata in SQL, logo in blob)
    public Type StorageProvider => typeof(HybridProfileStorageProvider);
    public Type BlobStorageProvider => typeof(AzureBlobProfileStorageProvider);

    // Logo, banner stored as blobs
    // Colors, fonts stored as JSON in SQL
}
```

---

## Caching Strategy

### Multi-Level Caching

```
┌─────────────────────────────────────────────────────────────────┐
│                      Cache Hierarchy                             │
│                                                                  │
│  Level 1: In-Memory Cache (L1)                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ IMemoryCache                                                │ │
│  │ - Individual module data                                    │ │
│  │ - TTL: 5 minutes (configurable)                             │ │
│  │ - Size limit: 1000 entries per node                         │ │
│  │ - Eviction: LRU                                             │ │
│  │                                                              │ │
│  │ Keys:                                                        │ │
│  │   "profile:module:{profileId}:{moduleName}"                 │ │
│  │   "profile:completeness:{profileId}"                        │ │
│  │   "profile:visibility:{profileId}:{moduleName}"             │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Level 2: Distributed Cache (L2)                                │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ IDistributedCache (Redis)                                   │ │
│  │ - Profile snapshots                                         │ │
│  │ - TTL: 10 minutes                                           │ │
│  │ - Shared across all nodes                                   │ │
│  │                                                              │ │
│  │ Keys:                                                        │ │
│  │   "profile:snapshot:{profileId}"                            │ │
│  │   "profile:modules:active:{profileId}"                      │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  Level 3: Storage Layer (L3)                                    │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Database / Blob Storage                                     │ │
│  │ - Source of truth                                           │ │
│  │ - Always consistent                                         │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### Cache Invalidation Strategy

```csharp
// On module save/update
public async Task SaveModuleAsync<TData>(
    string profileId,
    string moduleName,
    TData data,
    CancellationToken ct) where TData : class
{
    // 1. Save to storage
    await _storageProvider.SaveAsync(profileId, moduleName, data, ct);

    // 2. Invalidate L1 cache (local node)
    _memoryCache.Remove($"profile:module:{profileId}:{moduleName}");
    _memoryCache.Remove($"profile:completeness:{profileId}");

    // 3. Invalidate L2 cache (distributed)
    await _distributedCache.RemoveAsync($"profile:snapshot:{profileId}", ct);
    await _distributedCache.RemoveAsync($"profile:modules:active:{profileId}", ct);

    // 4. Publish cache invalidation event (for other nodes)
    await _eventBus.PublishAsync(new ProfileModuleCacheInvalidated
    {
        ProfileId = profileId,
        ModuleName = moduleName,
        Timestamp = DateTime.UtcNow
    }, ct);
}
```

---

## Security Architecture

### Access Control Flow

```
API Request
    │
    │ 1. Authentication (JWT, Cookie, etc.)
    ▼
┌─────────────────┐
│ Identity Context│
│ - UserId        │
│ - Roles         │
│ - Claims        │
│ - TenantId      │
└────────┬────────┘
         │ 2. Extract context
         ▼
┌─────────────────────────────┐
│ Authorization Middleware    │
│ - Verify user authenticated │
│ - Check tenant isolation    │
└────────┬────────────────────┘
         │ 3. Authorized request
         ▼
┌─────────────────────────────┐
│ Profile Service             │
│ - Receive request           │
│ - Get target profileId      │
└────────┬────────────────────┘
         │ 4. Check module visibility
         ▼
┌─────────────────────────────────────────────────┐
│ Visibility Service                              │
│                                                 │
│ Can user view module?                           │
│ ┌─────────────────────────────────────────────┐ │
│ │ 1. Get module visibility level              │ │
│ │    Private / Team / Org / Auth / Public     │ │
│ ├─────────────────────────────────────────────┤ │
│ │ 2. Check ownership                          │ │
│ │    Is user the profile owner?               │ │
│ ├─────────────────────────────────────────────┤ │
│ │ 3. Check visibility level                   │ │
│ │    Private: owner only                      │ │
│ │    Team: owner + team members               │ │
│ │    Organization: same tenant                │ │
│ │    Authenticated: any user                  │ │
│ │    Public: anyone                           │ │
│ ├─────────────────────────────────────────────┤ │
│ │ 4. Check explicit grants                    │ │
│ │    User explicitly granted access?          │ │
│ │    User has required role?                  │ │
│ └─────────────────────────────────────────────┘ │
└────────┬────────────────────────────────────────┘
         │ 5. Access decision
         │
    Granted?
    ╱      ╲
  Yes       No
   │         │
   │         └──> HTTP 403 Forbidden
   │
   ▼
┌─────────────────────────────┐
│ Load Module Data            │
└────────┬────────────────────┘
         │ 6. Apply field-level filtering
         ▼
┌─────────────────────────────┐
│ Field Visibility Filter     │
│ - Remove private fields     │
│ - Mask sensitive data       │
└────────┬────────────────────┘
         │ 7. Return filtered data
         ▼
    HTTP 200 OK
```

### Data Encryption

```
┌─────────────────────────────────────────────────────────────────┐
│                    Encryption Layers                             │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Transport Layer (TLS 1.2+)                                  │ │
│  │ - HTTPS for all API calls                                   │ │
│  │ - Certificate-based authentication                          │ │
│  └────────────────────────────────────────────────────────────┘ │
│                           ▼                                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Application Layer                                           │ │
│  │ - Sensitive fields encrypted (SSN, credit cards)            │ │
│  │ - Field-level encryption with Azure Key Vault              │ │
│  │ - Searchable encryption for indexed fields                  │ │
│  └────────────────────────────────────────────────────────────┘ │
│                           ▼                                      │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │ Storage Layer                                               │ │
│  │ - Database encryption at rest (TDE)                         │ │
│  │ - Blob storage encryption (AES-256)                         │ │
│  │ - Encrypted backups                                         │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

## Error Handling

### Error Handling Strategy

```csharp
// Profile service with comprehensive error handling
public class ProfileService : IProfileService
{
    public async Task SaveModuleAsync<TData>(
        string profileId,
        string moduleName,
        TData data,
        string? changeReason,
        CancellationToken ct) where TData : class
    {
        try
        {
            // 1. Validate input
            if (string.IsNullOrEmpty(profileId))
                throw new ArgumentException("Profile ID is required", nameof(profileId));

            if (string.IsNullOrEmpty(moduleName))
                throw new ArgumentException("Module name is required", nameof(moduleName));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // 2. Get module
            var module = await _moduleRegistry.GetModuleAsync(moduleName, ct);
            if (module == null)
                throw new ProfileModuleNotFoundException(moduleName);

            // 3. Validate data
            var validationResult = await module.ValidateAsync(data, ct);
            if (!validationResult.IsValid)
                throw new ProfileModuleValidationException(
                    moduleName,
                    validationResult.Errors
                );

            // 4. Check permissions
            var canModify = await _visibilityService.CanModifyModuleAsync(
                profileId,
                moduleName,
                _currentUser.UserId,
                ct
            );
            if (!canModify)
                throw new UnauthorizedProfileAccessException(
                    profileId,
                    moduleName,
                    "modify"
                );

            // 5. Save with retry logic
            await _retryPolicy.ExecuteAsync(async () =>
            {
                // Create version
                await _versioningService.CreateVersionAsync(
                    profileId,
                    moduleName,
                    data,
                    _currentUser.UserId,
                    changeReason,
                    ct
                );

                // Save to storage
                await _storageProvider.SaveAsync(
                    profileId,
                    moduleName,
                    data,
                    module.Metadata,
                    ct
                );
            });

            // 6. Invalidate cache
            await InvalidateCacheAsync(profileId, moduleName, ct);

            // 7. Publish event
            await _eventBus.PublishAsync(new ProfileModuleUpdated
            {
                ProfileId = profileId,
                ModuleName = moduleName,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = _currentUser.UserId
            }, ct);
        }
        catch (ProfileModuleNotFoundException ex)
        {
            _logger.LogWarning(ex, "Module not found: {ModuleName}", moduleName);
            throw;
        }
        catch (ProfileModuleValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for module: {ModuleName}", moduleName);
            throw;
        }
        catch (UnauthorizedProfileAccessException ex)
        {
            _logger.LogWarning(ex,
                "Unauthorized access to profile {ProfileId}, module {ModuleName}",
                profileId, moduleName);
            throw;
        }
        catch (StorageException ex)
        {
            _logger.LogError(ex,
                "Storage error saving profile {ProfileId}, module {ModuleName}",
                profileId, moduleName);
            throw new ProfileServiceException(
                "Failed to save profile module",
                ex
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error saving profile {ProfileId}, module {ModuleName}",
                profileId, moduleName);
            throw new ProfileServiceException(
                "An unexpected error occurred",
                ex
            );
        }
    }
}
```

---

## Performance Optimization

### Optimization Strategies

1. **Lazy Loading**
   - Load modules only when requested
   - Don't load entire profile if only one module needed
   - Use projection queries for metadata-only requests

2. **Caching**
   - Two-level cache (memory + distributed)
   - Cache module data with short TTL (5 min)
   - Cache completeness scores with longer TTL (10 min)
   - Cache visibility settings (rarely change)

3. **Parallel Loading**
   - Load multiple modules in parallel for snapshots
   - Use `Task.WhenAll()` for concurrent operations
   - Limit parallelism to avoid overwhelming storage

4. **Database Optimization**
   - Index on (ProfileId, ModuleName) for fast lookups
   - Index on (ProfileId, ModuleName, VersionNumber) for version queries
   - Partition large tables by ProfileType or TenantId
   - Use read replicas for query operations

5. **Blob Storage Optimization**
   - Use CDN for frequently accessed blobs (avatars)
   - Generate SAS tokens for direct browser upload/download
   - Resize images on upload (multiple sizes)
   - Lazy-load blob URLs (don't fetch blob data on profile load)

6. **Serialization**
   - Use System.Text.Json (faster than Newtonsoft.Json)
   - Configure serializer for performance (ignore nulls, camel case)
   - Cache serialization descriptors

---

## Deployment Architecture

### Multi-Tenant Deployment

```
┌─────────────────────────────────────────────────────────────────┐
│                          Load Balancer                           │
│                     (Azure Front Door / AWS ALB)                 │
└──────────────────────────┬──────────────────────────────────────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Web Tier 1   │  │ Web Tier 2   │  │ Web Tier 3   │
│ (API + UI)   │  │ (API + UI)   │  │ (API + UI)   │
└──────┬───────┘  └──────┬───────┘  └──────┬───────┘
       │                 │                 │
       └─────────────────┼─────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Redis Cache Cluster                          │
│                  (Distributed cache L2)                          │
└──────────────────────────┬──────────────────────────────────────┘
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ SQL Database │  │ NoSQL Store  │  │ Blob Storage │
│ (Primary +   │  │ (MongoDB/    │  │ (Azure Blob/ │
│  Read        │  │  CosmosDB)   │  │  AWS S3)     │
│  Replicas)   │  │              │  │              │
└──────────────┘  └──────────────┘  └──────────────┘

Tenant Isolation Strategy:
- Row-level security (TenantId in all tables)
- Separate databases per tier (optional)
- Separate blob containers per tenant
```

---

## Future Enhancements

1. **GraphQL Support**
   - GraphQL schema for profile queries
   - Field-level resolvers with visibility checks
   - Batching and caching

2. **Real-Time Sync**
   - SignalR for live profile updates
   - Optimistic UI updates
   - Conflict resolution

3. **AI-Powered Features**
   - Profile completeness suggestions
   - Automatic data enrichment
   - Duplicate detection

4. **Advanced Search**
   - ElasticSearch integration
   - Faceted search
   - Relevance ranking

5. **Mobile Offline Support**
   - Local storage with sync
   - Conflict resolution
   - Delta updates

---

## References

- Entity Framework Core 10.0 Documentation
- Azure Blob Storage Best Practices
- Redis Caching Strategies
- GDPR Data Portability Requirements
- JSON Schema Specification
