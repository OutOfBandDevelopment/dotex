# Database Template Source - API Design

**Epic:** 10 - Text Templating Extensions
**Feature:** Database Template Source
**Last Updated:** 2026-01-22

---

## API Overview

The Database Template Source API provides three primary components:
1. **DatabaseTemplateSource** - ITemplateSource implementation for database-backed templates
2. **ITemplateRepository** - Repository abstraction for database operations
3. **SqlServerTemplateRepository** - SQL Server implementation with Dapper

---

## Core Interfaces

### DatabaseTemplateSource

**Purpose:** Database-backed implementation of ITemplateSource.

```csharp
namespace OoBDev.System.Text.Templating.Sources;

/// <summary>
/// Database template source with versioning and multi-tenancy support.
/// </summary>
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

    /// <summary>
    /// Gets all active templates for current tenant and culture.
    /// </summary>
    /// <returns>Collection of template contexts</returns>
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

            return templates.Select(MapToContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query templates from database");
            throw new TemplateRepositoryException("Failed to query templates", ex);
        }
    }

    /// <summary>
    /// Gets specific template by name, culture, and version.
    /// </summary>
    /// <param name="name">Template name</param>
    /// <param name="culture">Optional culture (defaults to configured culture)</param>
    /// <param name="version">Optional version (defaults to latest)</param>
    /// <returns>Template context or null if not found</returns>
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

            return template == null ? null : MapToContext(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template {Name} from database", name);
            throw new TemplateRepositoryException($"Failed to get template {name}", ex);
        }
    }

    private ITemplateContext MapToContext(TemplateEntity entity)
    {
        return new TemplateContext
        {
            Name = entity.Name,
            ContentType = entity.ContentType,
            Version = entity.Version.ToString(),
            Culture = entity.Culture,
            Category = entity.Category,
            Metadata = new Dictionary<string, object?>
            {
                ["TemplateId"] = entity.Id,
                ["TenantId"] = entity.TenantId,
                ["CreatedAt"] = entity.CreatedAt,
                ["UpdatedAt"] = entity.UpdatedAt,
                ["CreatedBy"] = entity.CreatedBy,
                ["UpdatedBy"] = entity.UpdatedBy,
                ["Description"] = entity.Description,
                ["Tags"] = entity.Tags
            },
            Source = new DatabaseTemplateContentSource(entity.Id, _repository, _logger)
        };
    }
}
```

---

### ITemplateRepository

**Purpose:** Repository interface abstracting database operations.

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Repository for template database operations.
/// </summary>
public interface ITemplateRepository
{
    /// <summary>
    /// Gets all active templates for tenant and culture.
    /// </summary>
    Task<IEnumerable<TemplateEntity>> GetActiveTemplatesAsync(
        Guid? tenantId = null,
        string? culture = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets template by ID.
    /// </summary>
    Task<TemplateEntity?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets template by name, optionally filtering by tenant, culture, and version.
    /// </summary>
    Task<TemplateEntity?> GetByNameAsync(
        string name,
        Guid? tenantId = null,
        string? culture = null,
        int? version = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets templates by category.
    /// </summary>
    Task<IEnumerable<TemplateEntity>> GetByCategoryAsync(
        string category,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates new template.
    /// </summary>
    Task<TemplateEntity> CreateAsync(
        TemplateEntity template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing template (in-place).
    /// </summary>
    Task<TemplateEntity> UpdateAsync(
        TemplateEntity template,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates new version of template (preserves old version).
    /// </summary>
    Task<TemplateEntity> CreateNewVersionAsync(
        int templateId,
        string content,
        string updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all versions of template.
    /// </summary>
    Task<IEnumerable<TemplateEntity>> GetVersionHistoryAsync(
        string name,
        Guid? tenantId = null,
        string? culture = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates template (sets IsActive = true).
    /// </summary>
    Task<bool> ActivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates template (soft delete).
    /// </summary>
    Task<bool> DeactivateAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes template (hard delete).
    /// </summary>
    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
```

---

### DatabaseTemplateContentSource

**Purpose:** Lazy content loader for database templates.

```csharp
namespace OoBDev.System.Text.Templating.Sources;

/// <summary>
/// Lazy-loading content source for database templates.
/// </summary>
internal class DatabaseTemplateContentSource : ITemplateContentSource
{
    private readonly int _templateId;
    private readonly ITemplateRepository _repository;
    private readonly ILogger _logger;
    private string? _cachedContent;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public DatabaseTemplateContentSource(
        int templateId,
        ITemplateRepository repository,
        ILogger logger)
    {
        _templateId = templateId;
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Gets template content (cached after first load).
    /// </summary>
    public async Task<string> GetContentAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedContent != null)
            return _cachedContent;

        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_cachedContent != null)
                return _cachedContent;

            _logger.LogDebug("Loading content for template {TemplateId}", _templateId);

            var template = await _repository.GetByIdAsync(_templateId, cancellationToken);

            if (template == null)
            {
                throw new TemplateNotFoundException($"Template {_templateId} not found");
            }

            _cachedContent = template.Content;
            return _cachedContent;
        }
        catch (Exception ex) when (ex is not TemplateNotFoundException)
        {
            _logger.LogError(ex, "Failed to load content for template {TemplateId}", _templateId);
            throw new TemplateRepositoryException($"Failed to load template {_templateId}", ex);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    /// <summary>
    /// Gets template content as stream.
    /// </summary>
    public async Task<Stream> GetContentStreamAsync(CancellationToken cancellationToken = default)
    {
        var content = await GetContentAsync(cancellationToken);
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }
}
```

---

## Entity Model

### TemplateEntity

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Database entity for template storage.
/// </summary>
public class TemplateEntity
{
    public int Id { get; set; }

    // Core fields
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

## Configuration Options

```csharp
namespace OoBDev.System.Text.Templating.Sources;

/// <summary>
/// Configuration options for database template source.
/// </summary>
public class DatabaseTemplateSourceOptions
{
    /// <summary>
    /// Database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "";

    /// <summary>
    /// Default tenant ID for multi-tenant scenarios (null = shared templates).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Default culture code (e.g., "en-US", "es-ES").
    /// </summary>
    public string? Culture { get; set; }

    /// <summary>
    /// Enable query result caching (default: true).
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in minutes (default: 60).
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;
}
```

---

## Dependency Injection Extensions

```csharp
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for database template source.
/// </summary>
public static class DatabaseTemplateSourceServiceCollectionExtensions
{
    /// <summary>
    /// Adds database template source to service collection.
    /// </summary>
    public static IServiceCollection AddDatabaseTemplateSource(
        this IServiceCollection services,
        string connectionString,
        Guid? tenantId = null,
        string? culture = null)
    {
        services.Configure<DatabaseTemplateSourceOptions>(options =>
        {
            options.ConnectionString = connectionString;
            options.TenantId = tenantId;
            options.Culture = culture;
        });

        services.TryAddSingleton<ITemplateRepository, SqlServerTemplateRepository>();
        services.TryAddSingleton<ITemplateSource, DatabaseTemplateSource>();

        return services;
    }

    /// <summary>
    /// Adds database template source with configuration action.
    /// </summary>
    public static IServiceCollection AddDatabaseTemplateSource(
        this IServiceCollection services,
        Action<DatabaseTemplateSourceOptions> configure)
    {
        services.Configure(configure);
        services.TryAddSingleton<ITemplateRepository, SqlServerTemplateRepository>();
        services.TryAddSingleton<ITemplateSource, DatabaseTemplateSource>();

        return services;
    }

    /// <summary>
    /// Adds database template source from configuration section.
    /// </summary>
    public static IServiceCollection AddDatabaseTemplateSource(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "DatabaseTemplateSource")
    {
        services.Configure<DatabaseTemplateSourceOptions>(
            configuration.GetSection(sectionName));

        services.TryAddSingleton<ITemplateRepository, SqlServerTemplateRepository>();
        services.TryAddSingleton<ITemplateSource, DatabaseTemplateSource>();

        return services;
    }
}
```

---

## Usage Examples

### Example 1: Basic Registration

```csharp
using OoBDev.System.Text.Templating.Sources;
using Microsoft.Extensions.DependencyInjection;

// Register database template source
services.AddDatabaseTemplateSource(
    connectionString: "Server=localhost;Database=Templates;Trusted_Connection=True;",
    tenantId: null,      // Shared templates
    culture: "en-US"     // Default culture
);

// Use template engine (automatically discovers database templates)
var engine = serviceProvider.GetRequiredService<ITemplateEngine>();
var result = await engine.ApplyAsync("welcome-email", data);
```

---

### Example 2: Multi-Tenant Configuration

```csharp
// Configure for specific tenant
services.AddDatabaseTemplateSource(options =>
{
    options.ConnectionString = "Server=localhost;Database=Templates;Trusted_Connection=True;";
    options.TenantId = Guid.Parse("12345678-1234-1234-1234-123456789012");
    options.Culture = "en-US";
    options.EnableCaching = true;
    options.CacheDurationMinutes = 120;
});

// Templates automatically filtered by tenant
var engine = serviceProvider.GetRequiredService<ITemplateEngine>();
var templates = engine.GetAll("*");  // Only returns tenant's templates
```

---

### Example 3: Configuration File Integration

```json
// appsettings.json
{
  "DatabaseTemplateSource": {
    "ConnectionString": "Server=localhost;Database=Templates;Trusted_Connection=True;",
    "TenantId": null,
    "Culture": "en-US",
    "EnableCaching": true,
    "CacheDurationMinutes": 60
  }
}
```

```csharp
// Startup.cs
services.AddDatabaseTemplateSource(configuration, "DatabaseTemplateSource");
```

---

### Example 4: Direct Repository Usage

```csharp
using OoBDev.System.Text.Templating.Data;

public class TemplateManagementService
{
    private readonly ITemplateRepository _repository;

    public TemplateManagementService(ITemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<TemplateEntity> CreateTemplateAsync(
        string name,
        string content,
        string contentType,
        Guid tenantId,
        string createdBy)
    {
        var template = new TemplateEntity
        {
            Name = name,
            Content = content,
            ContentType = contentType,
            TenantId = tenantId,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = createdBy
        };

        return await _repository.CreateAsync(template);
    }

    public async Task<TemplateEntity> PublishNewVersionAsync(
        int templateId,
        string newContent,
        string updatedBy)
    {
        return await _repository.CreateNewVersionAsync(templateId, newContent, updatedBy);
    }
}
```

---

### Example 5: Template Versioning

```csharp
// Create initial template
var template = await _repository.CreateAsync(new TemplateEntity
{
    Name = "welcome-email",
    ContentType = "text/x-handlebars-template",
    Content = "Hello {{Name}}!",
    Version = 1,
    TenantId = tenantId,
    CreatedBy = "admin@example.com"
});

// Publish new version (preserves v1)
var v2 = await _repository.CreateNewVersionAsync(
    template.Id,
    content: "Hello {{FirstName}} {{LastName}}!",
    updatedBy: "admin@example.com"
);

// Get latest version (v2)
var latest = await _repository.GetByNameAsync("welcome-email", tenantId);
Assert.AreEqual(2, latest.Version);

// Get specific version (v1)
var v1 = await _repository.GetByNameAsync("welcome-email", tenantId, version: 1);
Assert.AreEqual(1, v1.Version);

// Get version history
var history = await _repository.GetVersionHistoryAsync("welcome-email", tenantId);
Assert.AreEqual(2, history.Count());
```

---

### Example 6: Multi-Culture Templates

```csharp
// Create English template
await _repository.CreateAsync(new TemplateEntity
{
    Name = "welcome-email",
    Content = "Hello {{Name}}!",
    ContentType = "text/x-handlebars-template",
    Culture = "en-US",
    TenantId = tenantId,
    CreatedBy = "admin@example.com"
});

// Create Spanish template
await _repository.CreateAsync(new TemplateEntity
{
    Name = "welcome-email",
    Content = "¡Hola {{Name}}!",
    ContentType = "text/x-handlebars-template",
    Culture = "es-ES",
    TenantId = tenantId,
    CreatedBy = "admin@example.com"
});

// Get by culture
var english = await _repository.GetByNameAsync("welcome-email", tenantId, culture: "en-US");
var spanish = await _repository.GetByNameAsync("welcome-email", tenantId, culture: "es-ES");

Assert.AreEqual("Hello {{Name}}!", english.Content);
Assert.AreEqual("¡Hola {{Name}}!", spanish.Content);
```

---

### Example 7: Category-Based Queries

```csharp
// Create templates with categories
await _repository.CreateAsync(new TemplateEntity
{
    Name = "order-confirmation",
    Content = "Your order #{{OrderNumber}} is confirmed.",
    ContentType = "text/x-handlebars-template",
    Category = "email",
    TenantId = tenantId,
    CreatedBy = "admin@example.com"
});

await _repository.CreateAsync(new TemplateEntity
{
    Name = "invoice",
    Content = "<invoice>...</invoice>",
    ContentType = "application/xslt+xml",
    Category = "pdf",
    TenantId = tenantId,
    CreatedBy = "admin@example.com"
});

// Query by category
var emailTemplates = await _repository.GetByCategoryAsync("email", tenantId);
var pdfTemplates = await _repository.GetByCategoryAsync("pdf", tenantId);

Assert.AreEqual(1, emailTemplates.Count());
Assert.AreEqual(1, pdfTemplates.Count());
```

---

### Example 8: Template Activation Control

```csharp
// Create template
var template = await _repository.CreateAsync(new TemplateEntity
{
    Name = "test-template",
    Content = "Test content",
    ContentType = "text/plain",
    TenantId = tenantId,
    IsActive = true,
    CreatedBy = "admin@example.com"
});

// Deactivate template (soft delete)
await _repository.DeactivateAsync(template.Id);

// Active templates query does NOT return deactivated template
var activeTemplates = await _repository.GetActiveTemplatesAsync(tenantId);
Assert.IsFalse(activeTemplates.Any(t => t.Id == template.Id));

// Reactivate template
await _repository.ActivateAsync(template.Id);

// Now returns in active query
activeTemplates = await _repository.GetActiveTemplatesAsync(tenantId);
Assert.IsTrue(activeTemplates.Any(t => t.Id == template.Id));
```

---

### Example 9: Migration from FileTemplateSource

```csharp
// Before: File-based templates
services.AddFileTemplateSource(options =>
{
    options.TemplateDirectory = Path.Combine(AppContext.BaseDirectory, "Templates");
});

// After: Database-backed templates (same ITemplateSource interface)
services.AddDatabaseTemplateSource(options =>
{
    options.ConnectionString = configuration.GetConnectionString("Templates");
    options.TenantId = tenantId;
});

// No changes to template engine usage!
var engine = serviceProvider.GetRequiredService<ITemplateEngine>();
var result = await engine.ApplyAsync("welcome-email", data);
```

---

## Exception Handling

### Exception Types

```csharp
namespace OoBDev.System.Text.Templating;

/// <summary>
/// Base exception for template repository errors.
/// </summary>
public class TemplateRepositoryException : TemplateException
{
    public TemplateRepositoryException(string message)
        : base(message)
    {
    }

    public TemplateRepositoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when template not found.
/// </summary>
public class TemplateNotFoundException : TemplateRepositoryException
{
    public string? TemplateName { get; }
    public int? TemplateId { get; }

    public TemplateNotFoundException(string message)
        : base(message)
    {
    }

    public TemplateNotFoundException(string templateName, Guid? tenantId = null)
        : base($"Template '{templateName}' not found" +
               (tenantId.HasValue ? $" for tenant {tenantId}" : ""))
    {
        TemplateName = templateName;
    }

    public TemplateNotFoundException(int templateId)
        : base($"Template with ID {templateId} not found")
    {
        TemplateId = templateId;
    }
}
```

### Error Handling Example

```csharp
try
{
    var template = await _repository.GetByNameAsync("non-existent", tenantId);
}
catch (TemplateNotFoundException ex)
{
    _logger.LogWarning(ex, "Template not found: {TemplateName}", ex.TemplateName);
    // Return default template or error message
}
catch (TemplateRepositoryException ex)
{
    _logger.LogError(ex, "Database error while querying template");
    throw;
}
```

---

## Best Practices

### 1. Connection String Security
```csharp
// ✅ GOOD: Use configuration with secrets
services.AddDatabaseTemplateSource(options =>
{
    options.ConnectionString = configuration.GetConnectionString("Templates");
});

// ❌ BAD: Hardcode connection string
services.AddDatabaseTemplateSource(options =>
{
    options.ConnectionString = "Server=prod;Password=secret123;";  // Never do this!
});
```

### 2. Versioning Strategy
```csharp
// ✅ GOOD: Create new version (preserves history)
await _repository.CreateNewVersionAsync(templateId, newContent, "admin@example.com");

// ❌ BAD: Update in-place (loses history)
template.Content = newContent;
await _repository.UpdateAsync(template);
```

### 3. Tenant Isolation
```csharp
// ✅ GOOD: Always pass tenant ID for multi-tenant systems
var templates = await _repository.GetActiveTemplatesAsync(currentTenantId);

// ❌ BAD: Query without tenant filter (security risk)
var templates = await _repository.GetActiveTemplatesAsync();  // Returns ALL tenants!
```

### 4. Soft Delete
```csharp
// ✅ GOOD: Use soft delete (can be restored)
await _repository.DeactivateAsync(template.Id);

// ❌ BAD: Hard delete (permanent)
await _repository.DeleteAsync(template.Id);  // Only use if required by regulation
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 10 Overview](../README-REVISED.md)
