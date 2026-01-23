# Database Template Source - Requirements

**Epic:** 10 - Text Templating Extensions
**Feature:** Database Template Source
**Priority:** MEDIUM
**Complexity:** MEDIUM
**Estimated LOC:** ~200

---

## Overview

Database-backed template storage implementation for the OoBDev template engine. Enables dynamic template management, versioning, multi-tenancy, and audit capabilities without requiring application redeployment.

---

## Business Requirements

### BR-1: Database Template Storage
**As a** developer
**I want** to store templates in a database instead of files
**So that** I can manage templates dynamically without redeployment

**Acceptance Criteria:**
- Templates stored in relational database
- Support SQL Server, PostgreSQL, MySQL
- CRUD operations for template management
- Template content stored as NVARCHAR(MAX) or TEXT
- Metadata (name, version, culture) indexed

---

### BR-2: Template Versioning
**As a** system administrator
**I want** templates to support versioning
**So that** I can track changes and rollback if needed

**Acceptance Criteria:**
- Each template has version number (integer)
- New versions do not delete old versions
- Can query specific version or latest version
- Audit trail of who/when template was modified

---

### BR-3: Multi-Culture Support
**As a** developer
**I want** templates to support multiple cultures
**So that** I can render localized content

**Acceptance Criteria:**
- Templates can have culture code (e.g., "en-US", "es-ES")
- Query templates by name + culture
- Fallback to default culture if specific not found
- Support culture-neutral templates (NULL culture)

---

### BR-4: Multi-Tenancy Support
**As a** SaaS provider
**I want** templates isolated by tenant
**So that** each customer has independent templates

**Acceptance Criteria:**
- Optional TenantId column
- Query templates by tenant
- Tenant isolation enforced at repository level
- Support shared templates (NULL tenant)

---

### BR-5: Template Categories
**As a** system administrator
**I want** to organize templates by category
**So that** I can manage templates logically

**Acceptance Criteria:**
- Templates have category (e.g., "email", "pdf", "sms")
- Query templates by category
- Category is optional (NULL allowed)
- Index on category for fast lookup

---

### BR-6: Template Activation Control
**As a** system administrator
**I want** to enable/disable templates
**So that** I can control which templates are active

**Acceptance Criteria:**
- Templates have IsActive flag
- Only active templates returned by default
- Can query inactive templates explicitly
- Soft delete support (set IsActive = false)

---

## Technical Requirements

### TR-1: ITemplateSource Implementation

```csharp
public class DatabaseTemplateSource : ITemplateSource
{
    private readonly ITemplateRepository _repository;
    private readonly IOptions<DatabaseTemplateSourceOptions> _options;

    public IEnumerable<ITemplateContext> GetTemplates()
    {
        var tenantId = _options.Value.TenantId;
        var culture = _options.Value.Culture;

        var templates = _repository.GetActiveTemplates(tenantId, culture);

        return templates.Select(t => new TemplateContext
        {
            Name = t.Name,
            ContentType = t.ContentType,
            Version = t.Version,
            Culture = t.Culture,
            Source = new DatabaseTemplateContentSource(t.Id, _repository)
        });
    }
}
```

---

### TR-2: Database Schema

```sql
CREATE TABLE Templates (
    Id INT PRIMARY KEY IDENTITY(1,1),

    -- Core fields
    Name NVARCHAR(200) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Version INT NOT NULL DEFAULT 1,

    -- Localization
    Culture NVARCHAR(10) NULL,

    -- Organization
    Category NVARCHAR(50) NULL,
    TenantId UNIQUEIDENTIFIER NULL,

    -- Lifecycle
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100) NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy NVARCHAR(100) NULL,

    -- Metadata
    Description NVARCHAR(500) NULL,
    Tags NVARCHAR(500) NULL,

    -- Constraints
    CONSTRAINT UQ_Templates_Name_Version_Culture_Tenant
        UNIQUE (Name, Version, Culture, TenantId)
);

-- Indexes
CREATE INDEX IX_Templates_Name_Culture_Tenant
    ON Templates(Name, Culture, TenantId)
    WHERE IsActive = 1;

CREATE INDEX IX_Templates_Category
    ON Templates(Category)
    WHERE IsActive = 1;

CREATE INDEX IX_Templates_TenantId
    ON Templates(TenantId)
    WHERE IsActive = 1;

-- Audit table (optional)
CREATE TABLE TemplateAudit (
    AuditId INT PRIMARY KEY IDENTITY(1,1),
    TemplateId INT NOT NULL,
    Action NVARCHAR(50) NOT NULL, -- CREATE, UPDATE, DELETE
    OldContent NVARCHAR(MAX) NULL,
    NewContent NVARCHAR(MAX) NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ChangedBy NVARCHAR(100) NULL,

    FOREIGN KEY (TemplateId) REFERENCES Templates(Id)
);
```

---

### TR-3: Repository Pattern

```csharp
public interface ITemplateRepository
{
    // Query
    Task<IEnumerable<TemplateEntity>> GetActiveTemplatesAsync(
        Guid? tenantId = null,
        string? culture = null,
        CancellationToken cancellationToken = default);

    Task<TemplateEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<TemplateEntity?> GetByNameAsync(
        string name,
        Guid? tenantId = null,
        string? culture = null,
        int? version = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TemplateEntity>> GetByCategoryAsync(
        string category,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    // Create/Update
    Task<TemplateEntity> CreateAsync(
        TemplateEntity template,
        CancellationToken cancellationToken = default);

    Task<TemplateEntity> UpdateAsync(
        TemplateEntity template,
        CancellationToken cancellationToken = default);

    // Versioning
    Task<TemplateEntity> CreateNewVersionAsync(
        int templateId,
        string content,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TemplateEntity>> GetVersionHistoryAsync(
        string name,
        Guid? tenantId = null,
        string? culture = null,
        CancellationToken cancellationToken = default);

    // Lifecycle
    Task<bool> ActivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
```

---

### TR-4: Entity Model

```csharp
public class TemplateEntity
{
    public int Id { get; set; }

    // Core
    public string Name { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string Content { get; set; } = "";
    public int Version { get; set; } = 1;

    // Localization
    public string? Culture { get; set; }

    // Organization
    public string? Category { get; set; }
    public Guid? TenantId { get; set; }

    // Lifecycle
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }

    // Metadata
    public string? Description { get; set; }
    public string? Tags { get; set; }
}
```

---

## Non-Functional Requirements

### NFR-1: Performance
- Query templates: < 100ms for 1000+ templates
- Content retrieval: < 50ms per template
- Index lookups: < 10ms
- Connection pooling enabled

### NFR-2: Scalability
- Support 10,000+ templates per tenant
- Efficient pagination for large result sets
- Minimal memory footprint (streaming content)

### NFR-3: Reliability
- Transaction support for multi-step operations
- Retry logic for transient failures
- Connection resilience

### NFR-4: Security
- SQL injection prevention (parameterized queries)
- Tenant isolation enforced
- Content sanitization (prevent XSS)

---

## Constraints

### C-1: Database Support
- **Phase 1:** SQL Server only
- **Phase 2:** PostgreSQL, MySQL (future)
- **Phase 3:** SQLite for development/testing (future)

### C-2: Content Size
- Max template size: 1 MB (NVARCHAR(MAX) limit: 2GB)
- Typical template: 10-50 KB
- Large templates: store in Azure Blob, reference in DB

### C-3: Versioning Strategy
- Version numbers only (no semantic versioning)
- Linear versioning (no branches)
- Old versions never deleted (audit trail)

---

## Success Criteria

- ✅ DatabaseTemplateSource implements ITemplateSource
- ✅ SQL Server repository with full CRUD
- ✅ Template versioning working
- ✅ Multi-culture support
- ✅ Multi-tenancy support
- ✅ 80%+ test coverage
- ✅ Migration guide from FileTemplateSource

---

## Out of Scope

- ❌ Template compilation in database (compile on-demand only)
- ❌ Template dependency tracking
- ❌ Template inheritance/composition (use Handlebars partials)
- ❌ Full-text search (use external search service)
- ❌ Template permissions/RBAC (handle at application level)

---

## Dependencies

### Internal
- ITemplateSource (existing)
- ITemplateContext (existing)
- OoBDev.Data.Abstractions (for repository pattern)

### External
- Microsoft.Data.SqlClient (SQL Server)
- Dapper or EF Core (data access)
- Microsoft.Extensions.Options (configuration)
- Microsoft.Extensions.Logging (logging)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Handlebars Provider](../HandlebarsProvider/requirements.md)
