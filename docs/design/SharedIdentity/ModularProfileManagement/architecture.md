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

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Client Applications\n(Web UI, Mobile Apps, Admin Portals, Third-party Integrations)" as Clients <<Context>>
rectangle "Profile Service Layer" as ServiceLayer <<Container>> {
  rectangle "Profile\nService" as ProfileSvc <<Component>>
  rectangle "Versioning\nService" as VersionSvc <<Component>>
  rectangle "Visibility\nService" as VisSvc <<Component>>
  rectangle "Import/\nExport" as ImpExp <<Component>>
}
rectangle "Profile Module Registry" as Registry <<Container>> {
  rectangle "User\nCore" as UserCore <<Module>>
  rectangle "Org\nCore" as OrgCore <<Module>>
  rectangle "Social\nLinks" as Social <<Module>>
  rectangle "Contact\nPrefs" as Contact <<Module>>
  rectangle "Custom\nModules" as Custom <<Module>>
}
rectangle "Storage Provider Abstraction" as Abstraction <<Container>> {
  rectangle "SQL\nProvider" as SqlP <<Component>>
  rectangle "NoSQL\nProvider" as NoSqlP <<Component>>
  rectangle "Blob\nStorage" as BlobP <<Component>>
  rectangle "Hybrid\nProvider" as HybridP <<Component>>
}
rectangle "Physical Storage" as Physical <<Container>> {
  rectangle "SQL Server/\nPostgreSQL" as Sql <<Store>>
  rectangle "MongoDB/\nCosmosDB" as Mongo <<Store>>
  rectangle "Azure Blob/\nAWS S3" as Blob <<Store>>
  rectangle "Redis\nCache" as Redis <<Store>>
}
Clients --> ServiceLayer : HTTPS / REST / GraphQL
ProfileSvc --> Registry
VersionSvc --> Registry
VisSvc --> Registry
ImpExp --> Registry
UserCore --> Abstraction
OrgCore --> Abstraction
Social --> Abstraction
Contact --> Abstraction
Custom --> Abstraction
SqlP --> Sql
NoSqlP --> Mongo
BlobP --> Blob
HybridP --> Redis
@enduml
```

*Figure 1 — System context*

### External Dependencies

- **Identity System**: User/organization IDs, authentication context
- **Caching Layer**: Module data caching, completeness score caching
- **Blob Storage**: Avatars, documents, large binary attachments
- **Search Engine**: Profile discovery and full-text search (optional)
- **Analytics Service**: Profile metrics and insights (optional)

---

## Component Architecture

### Core Components

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Profile Service" as ProfileService <<Container>> {
  rectangle "IProfileService\n\n- GetModuleAsync<TData>(profileId, moduleName)\n- SaveModuleAsync<TData>(profileId, moduleName, data)\n- DeleteModuleAsync(profileId, moduleName)\n- GetProfileSnapshotAsync(profileId)\n- GetCompletenessScoreAsync(profileId)\n- GetAvailableModulesAsync()\n- GetActiveModulesAsync(profileId)" as IService <<Component>>
  rectangle "IProfileModuleRegistry\n\n- GetModule(moduleName)\n- GetAllModules()\n- IsModuleActive(profileId, moduleName)\n- ActivateModule(profileId, moduleName)\n- DeactivateModule(profileId, moduleName)" as IRegistry <<Component>>
}
rectangle "Profile Module Framework" as Framework <<Container>> {
  rectangle "IProfileModule<TData>\n\n+ Name: string\n+ Category: string\n+ Version: Version\n+ Dependencies: IReadOnlyList<string>\n\n- ValidateAsync(data, ct): ValidationResult\n- GetDefaultDataAsync(ct): TData" as IModule <<Component>>
  rectangle "Built-In Modules" as BuiltIn <<Group>> {
    rectangle "UserProfile\nCore" as M1 <<Module>>
    rectangle "OrgCore\nModule" as M2 <<Module>>
    rectangle "Social\nLinks" as M3 <<Module>>
    rectangle "Contact\nPrefs" as M4 <<Module>>
  }
}
IService --> IRegistry : Uses
IRegistry --> Framework : Coordinates
@enduml
```

*Figure 2 — Core components*

### Storage Layer

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Storage Provider Layer" as Layer <<Container>> {
  rectangle "IProfileModuleStorageProvider\n\n- GetAsync<TData>(profileId, moduleName)\n- SaveAsync<TData>(profileId, moduleName, data, metadata)\n- DeleteAsync(profileId, moduleName)\n- GetVersionHistoryAsync(profileId, moduleName)" as IStorage <<Component>>
  rectangle "Concrete Providers:\n\nSqlProfileStorageProvider       (EF Core, relational)\nMongoProfileStorageProvider     (MongoDB, flexible schema)\nCosmosProfileStorageProvider    (CosmosDB, global)\nHybridProfileStorageProvider    (metadata+blob split)\nInMemoryProfileStorageProvider  (testing only)" as Concrete <<Component>>
  rectangle "IProfileBlobStorageProvider\n\n- GetBlobAsync(profileId, moduleName, blobKey)\n- SaveBlobAsync(profileId, moduleName, blobKey, stream)\n- DeleteBlobAsync(profileId, moduleName, blobKey)" as IBlob <<Component>>
  rectangle "Concrete Blob Providers:\n\nAzureBlobProfileStorageProvider    (Azure Blob Storage)\nS3ProfileBlobStorageProvider       (AWS S3)\nFileSystemBlobStorageProvider      (local files, dev only)" as ConcreteBlob <<Component>>
}
IStorage --> Concrete : Implemented By
Concrete -[hidden]down- IBlob
IBlob --> ConcreteBlob : Implemented By
@enduml
```

*Figure 3 — Storage layer*

### Versioning and Audit

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Versioning System" as System <<Container>> {
  rectangle "IProfileVersioningService\n\n- CreateVersionAsync<TData>(profileId, moduleName, data)\n- GetVersionHistoryAsync(profileId, moduleName)\n- GetVersionDataAsync<TData>(versionId)\n- RollbackToVersionAsync(versionId, reason)\n- CompareVersionsAsync(version1Id, version2Id)" as IVersioning <<Component>>
  rectangle "ProfileModuleVersion (Record)\n\n+ Id: string\n+ ProfileId: string\n+ ModuleName: string\n+ VersionNumber: int\n+ CreatedAt: DateTime\n+ CreatedBy: string\n+ CreatedByIpAddress: string?\n+ CreatedByUserAgent: string?\n+ ChangeReason: string?\n+ DataJson: string (complete snapshot)\n+ Metadata: ProfileModuleMetadata" as Version <<Component>>
  rectangle "Storage Strategy:\n- Versions stored in separate partition/table\n- Indexed by (ProfileId, ModuleName, VersionNumber)\n- Data stored as JSON for flexibility\n- Retention policy: configurable per tenant\n- Archival: move old versions to cold storage" as Strategy <<Note>>
}
IVersioning --> Version : Creates
Version -[hidden]down- Strategy
@enduml
```

*Figure 4 — Versioning and audit*

### Validation Framework

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Validation Framework" as Framework <<Container>> {
  rectangle "IProfileModuleValidator<TData>\n\n- ValidateAsync(data, context, ct): ValidationResult" as IValidator <<Component>>
  rectangle "Built-In Validators:\n\nRequiredFieldValidator<TData>\nRegexValidator<TData>\nRangeValidator<TData>\nLengthValidator<TData>\nEmailValidator<TData>\nPhoneNumberValidator<TData>\nUrlValidator<TData>\nUniqueValueValidator<TData>    (async, database query)\nCompositeValidator<TData>      (combines validators)" as BuiltIn <<Component>>
  rectangle "ValidationResult (Record)\n\n+ IsValid: bool\n+ Errors: IReadOnlyList<ValidationError>\n+ Warnings: IReadOnlyList<ValidationWarning>\n\nStatic Methods:\n- Success(): ValidationResult\n- Failure(errors): ValidationResult\n- Combine(results): ValidationResult" as Result <<Component>>
}
IValidator --> BuiltIn : Implemented By
BuiltIn -[hidden]down- Result
@enduml
```

*Figure 5 — Validation framework*

### Visibility and Privacy

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Visibility Service" as Service <<Container>> {
  rectangle "IProfileVisibilityService\n\n- CanViewModuleAsync(profileId, moduleName, viewerId)\n- ApplyVisibilityFilterAsync<TData>(data, viewerId)\n- SetModuleVisibilityAsync(profileId, moduleName, level)\n- SetFieldVisibilityAsync(profileId, field, level)\n- GetVisibilitySettingsAsync(profileId, moduleName)" as IVis <<Component>>
  rectangle "ProfileModuleVisibility (Enum)\n\nPrivate        - Only profile owner\nTeam           - Profile owner + team members\nOrganization   - All organization members\nAuthenticated  - All authenticated users\nPublic         - Anyone (including anonymous)" as Enum <<Component>>
  rectangle "ProfileFieldVisibility (Record)\n\n+ FieldPath: string\n+ Visibility: ProfileModuleVisibility\n+ ExplicitUsers: IReadOnlyList<string>\n+ ExplicitRoles: IReadOnlyList<string>" as Field <<Component>>
  rectangle "Visibility Resolution Algorithm:\n1. Check module-level visibility\n2. For fields, check field-level overrides\n3. Evaluate explicit user grants\n4. Evaluate explicit role grants\n5. Filter data based on viewer context" as Algo <<Note>>
}
IVis --> Enum : Uses
Enum -[hidden]down- Field
Field -[hidden]down- Algo
@enduml
```

*Figure 6 — Visibility and privacy*

---

## Data Flow

### Profile Module Save Operation

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Client Request (Save Profile Module)" as Client
rectangle "Profile Controller\n(API Layer)" as Controller
rectangle "Profile Service\nSaveModuleAsync" as Service
rectangle "Module Registry\nGetModule()" as Registry
rectangle "Profile Module\nValidateAsync()" as Module
rectangle "Validation Result\nIsValid: true/false" as Result
rectangle "Visibility Service\nCheck permissions" as Visibility
rectangle "Versioning Service\nCreateVersionAsync" as Versioning
rectangle "Storage Provider\nSaveAsync()" as Storage
rectangle "Cache Invalidation" as Cache
rectangle "Event Publishing\n(Optional)" as Events
rectangle "Success Response\n(HTTP 200/204)" as Response
Client --> Controller : 1. POST /api/profiles/{profileId}/modules/{moduleName}
Controller --> Service : 2. Deserialize TData, get user context
Service --> Registry : 3. Get module from registry
Registry --> Module : 4. Return IProfileModule<TData>
Module --> Result : 5. Validation rules execute\n- Required fields\n- Format validation\n- Cross-field rules\n- Async validators (DB checks)
Result --> Visibility : 6. If invalid, return errors to client\nIf valid, continue
Visibility --> Versioning : 7. Verify user can modify module
Versioning --> Storage : 8. Create version record\n- Capture audit fields\n- Serialize current data\n- Store version metadata
Storage --> Cache : 9. Persist to appropriate storage\n- SQL for structured data\n- NoSQL for flexible schema\n- Blob for binary data
Cache --> Events : 10. Invalidate cached module data\nInvalidate completeness score
Events --> Response : 11. Publish ProfileModuleUpdated event
@enduml
```

*Figure 7 — Profile module save operation*

### Profile Snapshot Retrieval

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Client Request (Get Full Profile)" as Client
rectangle "Profile Controller" as Controller
rectangle "Profile Service\nGetSnapshotAsync" as Service
rectangle "Module Registry\nGetActiveModules()" as Registry
rectangle "Parallel Module Loading\n\nFor each module:" as Loading {
  rectangle "1. Check cache\n- Hit: return cached\n- Miss: load from storage" as L1
  rectangle "2. Check visibility\n- Can view: continue\n- Cannot: skip module" as L2
  rectangle "3. Apply field filtering\n- Remove private fields\n- Mask sensitive data" as L3
}
rectangle "Profile Snapshot\n{\n  ProfileId: '...',\n  ProfileType: User\n  LastModified: ...\n  Modules: {\n    UserCore: {...}\n    ContactPrefs: ...\n    SocialLinks: ...\n  }\n}" as Snapshot
rectangle "JSON Response\n(HTTP 200)" as Response
Client --> Controller : 1. GET /api/profiles/{profileId}/snapshot
Controller --> Service : 2. Get viewer context
Service --> Registry : 3. Get active modules for profile
Registry --> Loading : 4. Return list of module names\n[UserCore, ContactPrefs, SocialLinks, ...]
L1 -[hidden]down- L2
L2 -[hidden]down- L3
Loading --> Snapshot : 5. Aggregate results
Snapshot --> Response : 6. Cache snapshot (short TTL)
@enduml
```

*Figure 8 — Profile snapshot retrieval*

### Profile Completeness Calculation

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Completeness Score Request" as Request
rectangle "Profile Service\nGetCompleteness" as Service
rectangle "Cache Hit?" as Decision <<Decision>>
rectangle "Module Registry\nGetRequiredModules" as Registry
rectangle "Load Modules\n(parallel)" as Load
rectangle "Calculate Score\n\nFor each required module:\n- Module exists? +weight\n- Required fields:\n  - Populated? +points\n  - Empty? +0\n- Optional fields:\n  - Populated? +bonus\n  - Empty? +0\n\nScore = (points / total) * 100" as Calc
rectangle "Return Score\n(0-100)" as Return
Request --> Service : 1. Calculate score for profile
Service --> Decision : 2. Check cache\nKey: 'completeness:{profileId}'
Decision --> Return : Yes
Decision --> Registry : No
Registry --> Load : 3. Get list of required modules\nand their required fields
Load --> Calc : 4. Load all required modules
Calc --> Return : 5. Cache result (10 min TTL)
@enduml
```

*Figure 9 — Profile completeness calculation*

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

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Application Startup" as Startup
rectangle "Profile Module\nRegistry Builder" as Builder
rectangle "Module Discovery\n\nFor each IProfileModule<TData>:" as Discovery {
  rectangle "1. Extract metadata\n- Name\n- Category\n- Version\n- Dependencies\n- Data type\n- Storage provider" as D1
  rectangle "2. Validate module\n- No circular dependencies\n- Dependencies exist\n- Storage provider valid" as D2
  rectangle "3. Build dependency graph\n- Topological sort\n- Detect cycles" as D3
}
rectangle "Module Registry\n(Ready for use)\n\nModules:\n- UserCore\n- OrgCore\n- ContactPrefs\n- SocialLinks\n- UserSkills\n- CompanyBranding" as Registry
Startup --> Builder : 1. DI container initialization
Builder --> Discovery : 2. Scan for IProfileModule<TData> registrations
D1 -[hidden]down- D2
D2 -[hidden]down- D3
Discovery --> Registry : 4. Register in registry
@enduml
```

*Figure 10 — Module discovery process*

---

## Storage Provider Selection

### Storage Strategy Decision Tree

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Profile Module Data" as Data
rectangle "What type of data?" as Question <<Decision>>
rectangle "SQL Storage Provider\n(Entity Framework)" as Sql
rectangle "NoSQL Storage Provider\n(MongoDB, CosmosDB)" as NoSql
rectangle "Blob Storage Provider\n(Azure Blob, S3)" as Blob
rectangle "Hybrid Storage Provider\n(SQL + Blob)" as Hybrid
Data --> Question
Question --> Sql : Yes - Structured, relational?\n(normalize, joins, ACID)
Question --> NoSql : Yes - Flexible schema, high write?\n(frequent changes, scale)
Question --> Blob : Yes - Binary data (images, docs)?\n(>1MB, streaming)
Question --> Hybrid : Yes - Hybrid (metadata + blobs)?\n(profile photo with metadata)
@enduml
```

*Figure 11 — Storage strategy decision tree*

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

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Cache Hierarchy" as Hierarchy <<Container>> {
  rectangle "Level 1: In-Memory Cache (L1)\n\nIMemoryCache\n- Individual module data\n- TTL: 5 minutes (configurable)\n- Size limit: 1000 entries per node\n- Eviction: LRU\n\nKeys:\n  'profile:module:{profileId}:{moduleName}'\n  'profile:completeness:{profileId}'\n  'profile:visibility:{profileId}:{moduleName}'" as L1 <<Cache>>
  rectangle "Level 2: Distributed Cache (L2)\n\nIDistributedCache (Redis)\n- Profile snapshots\n- TTL: 10 minutes\n- Shared across all nodes\n\nKeys:\n  'profile:snapshot:{profileId}'\n  'profile:modules:active:{profileId}'" as L2 <<Cache>>
  rectangle "Level 3: Storage Layer (L3)\n\nDatabase / Blob Storage\n- Source of truth\n- Always consistent" as L3 <<Store>>
}
L1 -[hidden]down- L2
L2 -[hidden]down- L3
@enduml
```

*Figure 12 — Multi-level cache hierarchy*

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

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "API Request" as Request
rectangle "Identity Context\n- UserId\n- Roles\n- Claims\n- TenantId" as Identity
rectangle "Authorization Middleware\n- Verify user authenticated\n- Check tenant isolation" as Auth
rectangle "Profile Service\n- Receive request\n- Get target profileId" as Service
rectangle "Visibility Service\n\nCan user view module?\n1. Get module visibility level\n   Private / Team / Org / Auth / Public\n2. Check ownership\n   Is user the profile owner?\n3. Check visibility level\n   Private: owner only\n   Team: owner + team members\n   Organization: same tenant\n   Authenticated: any user\n   Public: anyone\n4. Check explicit grants\n   User explicitly granted access?\n   User has required role?" as Visibility
rectangle "Granted?" as Decision <<Decision>>
rectangle "HTTP 403 Forbidden" as Forbidden
rectangle "Load Module Data" as Load
rectangle "Field Visibility Filter\n- Remove private fields\n- Mask sensitive data" as Filter
rectangle "HTTP 200 OK" as Ok
Request --> Identity : 1. Authentication (JWT, Cookie, etc.)
Identity --> Auth : 2. Extract context
Auth --> Service : 3. Authorized request
Service --> Visibility : 4. Check module visibility
Visibility --> Decision : 5. Access decision
Decision --> Forbidden : No
Decision --> Load : Yes
Load --> Filter : 6. Apply field-level filtering
Filter --> Ok : 7. Return filtered data
@enduml
```

*Figure 13 — Access control flow*

### Data Encryption

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Encryption Layers" as Layers <<Container>> {
  rectangle "Transport Layer (TLS 1.2+)\n- HTTPS for all API calls\n- Certificate-based authentication" as Transport <<Layer>>
  rectangle "Application Layer\n- Sensitive fields encrypted (SSN, credit cards)\n- Field-level encryption with Azure Key Vault\n- Searchable encryption for indexed fields" as App <<Layer>>
  rectangle "Storage Layer\n- Database encryption at rest (TDE)\n- Blob storage encryption (AES-256)\n- Encrypted backups" as Storage <<Layer>>
}
Transport --> App
App --> Storage
@enduml
```

*Figure 14 — Encryption layers*

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

```plantuml
@startuml
skinparam shadowing false
skinparam rectangle {
  BackgroundColor White
  BorderColor #444444
}
rectangle "Load Balancer\n(Azure Front Door / AWS ALB)" as LB <<Container>>
rectangle "Web Tier 1\n(API + UI)" as Web1 <<Container>>
rectangle "Web Tier 2\n(API + UI)" as Web2 <<Container>>
rectangle "Web Tier 3\n(API + UI)" as Web3 <<Container>>
rectangle "Redis Cache Cluster\n(Distributed cache L2)" as Redis <<Container>>
rectangle "SQL Database\n(Primary +\n Read\n Replicas)" as Sql <<Store>>
rectangle "NoSQL Store\n(MongoDB/\n CosmosDB)" as NoSql <<Store>>
rectangle "Blob Storage\n(Azure Blob/\n AWS S3)" as Blob <<Store>>
LB --> Web1
LB --> Web2
LB --> Web3
Web1 --> Redis
Web2 --> Redis
Web3 --> Redis
Redis --> Sql
Redis --> NoSql
Redis --> Blob
note bottom of Redis
  Tenant Isolation Strategy:
  - Row-level security (TenantId in all tables)
  - Separate databases per tier (optional)
  - Separate blob containers per tenant
end note
@enduml
```

*Figure 15 — Multi-tenant deployment*

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
