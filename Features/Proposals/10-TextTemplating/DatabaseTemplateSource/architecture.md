# Database Template Source - Architecture

**Epic:** 10 - Text Templating Extensions
**Feature:** Database Template Source
**Last Updated:** 2026-01-22

---

## Architectural Overview

Database Template Source provides persistent, versioned, multi-tenant template storage using relational databases. It implements the existing `ITemplateSource` interface for seamless integration with the OoBDev template engine.

```
┌──────────────────────────────────────────────────────────────┐
│                    Template Engine                            │
│  - GetAll() → discovers all template sources                  │
└────────────────┬──────────────┬──────────────────────────────┘
                 ↓              ↓
      ┌──────────────┐ ┌──────────────┐
      │     File     │ │   Database   │
      │   Source     │ │   Source     │
      │  (Existing)  │ │    (NEW)     │
      └──────────────┘ └──────┬───────┘
                              ↓
                   ┌────────────────────┐
                   │ ITemplateRepository│
                   │ - GetActive()      │
                   │ - GetByName()      │
                   │ - GetById()        │
                   └────────┬───────────┘
                            ↓
         ┌─────────────────────────────────────┐
         │        Database Layer                │
         │  SQL Server | PostgreSQL | MySQL     │
         │  - Templates table                   │
         │  - TemplateAudit table               │
         │  - Indexes for fast lookup           │
         └──────────────────────────────────────┘
```

**Key Principle:** Leverage existing `ITemplateSource` abstraction, add database persistence with versioning and multi-tenancy.

---

## Core Components

### 1. DatabaseTemplateSource (Main Component)

**Responsibilities:**
- Implement `ITemplateSource` interface
- Query active templates from database
- Filter by tenant, culture, category
- Return `ITemplateContext` instances

**Design Pattern:** Repository Pattern

**Implementation:**

```csharp
namespace OoBDev.System.Text.Templating.Sources;

public class DatabaseTemplateSource : ITemplateSource
{
    private readonly ITemplateRepository _repository;
    private readonly IOptions<DatabaseTemplateSourceOptions> _options;
    private readonly ILogger<DatabaseTemplateSource> _logger;

    public DatabaseTemplateSource(
        ITemplateRepository repository,
        IOptions<DatabaseTemplateSourceOptions> options,
        ILogger<DatabaseTemplateSource> logger)
    {
        _repository = repository;
        _options = options;
        _logger = logger;
    }

    public IEnumerable<ITemplateContext> GetTemplates()
    {
        try
        {
            var tenantId = _options.Value.TenantId;
            var culture = _options.Value.Culture;

            _logger.LogDebug("Querying templates for tenant {TenantId}, culture {Culture}",
                tenantId, culture);

            var templates = _repository
                .GetActiveTemplatesAsync(tenantId, culture)
                .GetAwaiter()
                .GetResult();

            return templates.Select(t => new TemplateContext
            {
                Name = t.Name,
                ContentType = t.ContentType,
                Version = t.Version.ToString(),
                Culture = t.Culture,
                Category = t.Category,
                Metadata = new Dictionary<string, object?>
                {
                    ["TemplateId"] = t.Id,
                    ["TenantId"] = t.TenantId,
                    ["CreatedAt"] = t.CreatedAt,
                    ["UpdatedAt"] = t.UpdatedAt,
                    ["Description"] = t.Description,
                    ["Tags"] = t.Tags
                },
                Source = new DatabaseTemplateContentSource(t.Id, _repository, _logger)
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query templates from database");
            throw;
        }
    }

    public ITemplateContext? GetTemplate(
        string name,
        string? culture = null,
        int? version = null)
    {
        try
        {
            var tenantId = _options.Value.TenantId;
            culture ??= _options.Value.Culture;

            var template = _repository
                .GetByNameAsync(name, tenantId, culture, version)
                .GetAwaiter()
                .GetResult();

            if (template == null)
                return null;

            return new TemplateContext
            {
                Name = template.Name,
                ContentType = template.ContentType,
                Version = template.Version.ToString(),
                Culture = template.Culture,
                Category = template.Category,
                Metadata = new Dictionary<string, object?>
                {
                    ["TemplateId"] = template.Id,
                    ["TenantId"] = template.TenantId,
                    ["CreatedAt"] = template.CreatedAt,
                    ["UpdatedAt"] = template.UpdatedAt,
                    ["Description"] = template.Description,
                    ["Tags"] = template.Tags
                },
                Source = new DatabaseTemplateContentSource(template.Id, _repository, _logger)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template {Name} from database", name);
            throw;
        }
    }
}
```

---

### 2. DatabaseTemplateContentSource (Content Loader)

**Responsibilities:**
- Lazy-load template content from database
- Implement `ITemplateContentSource`
- Cache content after first load

**Implementation:**

```csharp
namespace OoBDev.System.Text.Templating.Sources;

internal class DatabaseTemplateContentSource : ITemplateContentSource
{
    private readonly int _templateId;
    private readonly ITemplateRepository _repository;
    private readonly ILogger _logger;
    private string? _cachedContent;

    public DatabaseTemplateContentSource(
        int templateId,
        ITemplateRepository repository,
        ILogger logger)
    {
        _templateId = templateId;
        _repository = repository;
        _logger = logger;
    }

    public async Task<string> GetContentAsync(CancellationToken cancellationToken = default)
    {
        // Return cached if available
        if (_cachedContent != null)
            return _cachedContent;

        try
        {
            var template = await _repository.GetByIdAsync(_templateId, cancellationToken);

            if (template == null)
            {
                throw new TemplateNotFoundException($"Template {_templateId} not found");
            }

            _cachedContent = template.Content;
            return _cachedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load content for template {TemplateId}", _templateId);
            throw;
        }
    }

    public async Task<Stream> GetContentStreamAsync(CancellationToken cancellationToken = default)
    {
        var content = await GetContentAsync(cancellationToken);
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }
}
```

---

### 3. SqlServerTemplateRepository (SQL Server Implementation)

**Responsibilities:**
- Implement `ITemplateRepository`
- Execute SQL queries with parameters
- Handle transactions and connection management
- Support async operations

**Implementation:**

```csharp
namespace OoBDev.System.Text.Templating.Data;

public class SqlServerTemplateRepository : ITemplateRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerTemplateRepository> _logger;

    public SqlServerTemplateRepository(
        IOptions<DatabaseTemplateSourceOptions> options,
        ILogger<SqlServerTemplateRepository> logger)
    {
        _connectionString = options.Value.ConnectionString
            ?? throw new ArgumentNullException(nameof(options.Value.ConnectionString));
        _logger = logger;
    }

    public async Task<IEnumerable<TemplateEntity>> GetActiveTemplatesAsync(
        Guid? tenantId = null,
        string? culture = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, ContentType, Content, Version, Culture, Category,
                   TenantId, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
                   Description, Tags
            FROM Templates
            WHERE IsActive = 1
              AND (@TenantId IS NULL OR TenantId = @TenantId OR TenantId IS NULL)
              AND (@Culture IS NULL OR Culture = @Culture OR Culture IS NULL)
            ORDER BY Name, Version DESC";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var templates = await connection.QueryAsync<TemplateEntity>(
            new CommandDefinition(
                sql,
                new { TenantId = tenantId, Culture = culture },
                cancellationToken: cancellationToken));

        return templates;
    }

    public async Task<TemplateEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, ContentType, Content, Version, Culture, Category,
                   TenantId, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
                   Description, Tags
            FROM Templates
            WHERE Id = @Id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TemplateEntity>(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));
    }

    public async Task<TemplateEntity?> GetByNameAsync(
        string name,
        Guid? tenantId = null,
        string? culture = null,
        int? version = null,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT TOP 1 Id, Name, ContentType, Content, Version, Culture, Category,
                   TenantId, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
                   Description, Tags
            FROM Templates
            WHERE Name = @Name
              AND IsActive = 1
              AND (@TenantId IS NULL OR TenantId = @TenantId OR TenantId IS NULL)
              AND (@Culture IS NULL OR Culture = @Culture OR Culture IS NULL)
              AND (@Version IS NULL OR Version = @Version)
            ORDER BY Version DESC";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<TemplateEntity>(
            new CommandDefinition(
                sql,
                new { Name = name, TenantId = tenantId, Culture = culture, Version = version },
                cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<TemplateEntity>> GetByCategoryAsync(
        string category,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, ContentType, Content, Version, Culture, Category,
                   TenantId, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
                   Description, Tags
            FROM Templates
            WHERE Category = @Category
              AND IsActive = 1
              AND (@TenantId IS NULL OR TenantId = @TenantId OR TenantId IS NULL)
            ORDER BY Name, Version DESC";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QueryAsync<TemplateEntity>(
            new CommandDefinition(
                sql,
                new { Category = category, TenantId = tenantId },
                cancellationToken: cancellationToken));
    }

    public async Task<TemplateEntity> CreateAsync(
        TemplateEntity template,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO Templates (Name, ContentType, Content, Version, Culture, Category,
                                 TenantId, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
                                 Description, Tags)
            VALUES (@Name, @ContentType, @Content, @Version, @Culture, @Category,
                    @TenantId, @IsActive, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
                    @Description, @Tags);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        template.Id = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                template,
                cancellationToken: cancellationToken));

        return template;
    }

    public async Task<TemplateEntity> UpdateAsync(
        TemplateEntity template,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Templates
            SET Content = @Content,
                ContentType = @ContentType,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy,
                Description = @Description,
                Tags = @Tags
            WHERE Id = @Id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        template.UpdatedAt = DateTime.UtcNow;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                template,
                cancellationToken: cancellationToken));

        return template;
    }

    public async Task<TemplateEntity> CreateNewVersionAsync(
        int templateId,
        string content,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get existing template
            const string selectSql = @"
                SELECT TOP 1 Name, ContentType, Culture, Category, TenantId, Version
                FROM Templates
                WHERE Id = @Id";

            var existing = await connection.QuerySingleOrDefaultAsync<TemplateEntity>(
                new CommandDefinition(
                    selectSql,
                    new { Id = templateId },
                    transaction,
                    cancellationToken: cancellationToken));

            if (existing == null)
            {
                throw new TemplateNotFoundException($"Template {templateId} not found");
            }

            // Create new version
            var newTemplate = new TemplateEntity
            {
                Name = existing.Name,
                ContentType = existing.ContentType,
                Content = content,
                Version = existing.Version + 1,
                Culture = existing.Culture,
                Category = existing.Category,
                TenantId = existing.TenantId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy
            };

            const string insertSql = @"
                INSERT INTO Templates (Name, ContentType, Content, Version, Culture, Category,
                                     TenantId, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
                VALUES (@Name, @ContentType, @Content, @Version, @Culture, @Category,
                        @TenantId, @IsActive, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            newTemplate.Id = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    insertSql,
                    newTemplate,
                    transaction,
                    cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);

            return newTemplate;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IEnumerable<TemplateEntity>> GetVersionHistoryAsync(
        string name,
        Guid? tenantId = null,
        string? culture = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT Id, Name, ContentType, Content, Version, Culture, Category,
                   TenantId, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy,
                   Description, Tags
            FROM Templates
            WHERE Name = @Name
              AND (@TenantId IS NULL OR TenantId = @TenantId OR TenantId IS NULL)
              AND (@Culture IS NULL OR Culture = @Culture OR Culture IS NULL)
            ORDER BY Version DESC";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QueryAsync<TemplateEntity>(
            new CommandDefinition(
                sql,
                new { Name = name, TenantId = tenantId, Culture = culture },
                cancellationToken: cancellationToken));
    }

    public async Task<bool> ActivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Templates
            SET IsActive = 1,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE Templates
            SET IsActive = 0,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));

        return affected > 0;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            DELETE FROM Templates
            WHERE Id = @Id";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var affected = await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Id = id },
                cancellationToken: cancellationToken));

        return affected > 0;
    }
}
```

---

## Data Flow

### Template Query Flow

```
┌──────────────────┐
│  Template Engine │   GetAll()
└────────┬─────────┘
         ↓
┌─────────────────────────────────────────────────────┐
│      DatabaseTemplateSource                         │
│  1. Get tenant/culture from options                 │
│  2. Query repository                                │
└────────┬────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────┐
│      SqlServerTemplateRepository                    │
│  1. Open connection                                 │
│  2. Execute parameterized query                     │
│  3. Map rows to TemplateEntity                      │
└────────┬────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────┐
│         SQL Server                                  │
│  SELECT * FROM Templates                            │
│  WHERE IsActive = 1                                 │
│    AND TenantId = @TenantId                         │
│    AND Culture = @Culture                           │
└─────────────────────────────────────────────────────┘
```

### Template Content Loading Flow

```
┌──────────────────┐
│ Template Provider│   ApplyAsync(context, data)
└────────┬─────────┘
         ↓
┌─────────────────────────────────────────────────────┐
│      ITemplateContext                               │
│  context.Source.GetContentAsync()                   │
└────────┬────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────┐
│      DatabaseTemplateContentSource                  │
│  1. Check cache                                     │
│  2. If not cached, query repository                 │
│  3. Cache result                                    │
│  4. Return content                                  │
└────────┬────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────┐
│      SqlServerTemplateRepository                    │
│  GetByIdAsync(templateId)                           │
└────────┬────────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────────────────┐
│         SQL Server                                  │
│  SELECT Content FROM Templates WHERE Id = @Id       │
└─────────────────────────────────────────────────────┘
```

---

## Design Patterns

### 1. Repository Pattern
- `ITemplateRepository` abstracts data access
- `SqlServerTemplateRepository` implements SQL Server-specific logic
- Easy to add PostgreSQL, MySQL, etc.

### 2. Lazy Loading
- `DatabaseTemplateContentSource` loads content on-demand
- Content cached after first load
- Reduces memory footprint

### 3. Options Pattern
- `DatabaseTemplateSourceOptions` for configuration
- Injected via `IOptions<T>`
- Supports ASP.NET Core configuration

---

## Performance Optimizations

### 1. Connection Pooling
```csharp
// Connection string
"Server=localhost;Database=Templates;Trusted_Connection=True;Min Pool Size=5;Max Pool Size=100"
```

### 2. Indexed Queries
```sql
-- Fast lookups by name + tenant + culture
CREATE INDEX IX_Templates_Name_Culture_Tenant
    ON Templates(Name, Culture, TenantId)
    WHERE IsActive = 1;
```

### 3. Content Caching
```csharp
private string? _cachedContent;

public async Task<string> GetContentAsync()
{
    if (_cachedContent != null)
        return _cachedContent;

    _cachedContent = await LoadFromDatabaseAsync();
    return _cachedContent;
}
```

---

## Error Handling

### Connection Errors
```csharp
try
{
    await connection.OpenAsync(cancellationToken);
}
catch (SqlException ex) when (ex.Number == -2) // Timeout
{
    _logger.LogWarning(ex, "Database connection timeout");
    throw new TemplateRepositoryException("Database connection timeout", ex);
}
catch (SqlException ex)
{
    _logger.LogError(ex, "Database connection failed");
    throw new TemplateRepositoryException("Database connection failed", ex);
}
```

### Query Errors
```csharp
try
{
    return await connection.QueryAsync<TemplateEntity>(sql, parameters);
}
catch (SqlException ex)
{
    _logger.LogError(ex, "Failed to query templates");
    throw new TemplateRepositoryException("Failed to query templates", ex);
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
