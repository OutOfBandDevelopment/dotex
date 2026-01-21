# Configuration Documentation Protocol

**Version:** 1.0.0
**Last Updated:** 2026-01-21
**Category:** Documentation
**Trigger Phrases:** "find all my configurations", "document configurations", "create configuration reference"

---

## Purpose

This protocol systematically discovers and documents all configuration settings used across the OoBDev framework, including:
- IOptions<T> pattern classes
- IConfiguration keys
- Environment variables
- Connection strings
- Test parameters

The output is a comprehensive CONFIGURATION_SETTINGS.md file that serves as a reference for developers and operations teams.

---

## When to Use This Protocol

Use this protocol when:
1. User asks to "find all my configurations"
2. User requests "document all settings"
3. User needs a configuration reference document
4. After major feature additions that introduce new configuration
5. Before releases to ensure configuration documentation is current

---

## Output File

**Location:** `/src/CONFIGURATION_SETTINGS.md`

**Purpose:** Centralized reference for all configuration settings across the framework

---

## Discovery Process

### Phase 1: Options Pattern Classes

Search for all classes used with IOptions<T> pattern.

**Search Strategy:**
```bash
# Find all Options classes (naming convention)
find . -name "*Options.cs" -o -name "*Settings.cs" -o -name "*Configuration.cs"

# Find all IOptions<T> usage
grep -r "IOptions<" --include="*.cs"
grep -r "IOptionsSnapshot<" --include="*.cs"
grep -r "IOptionsMonitor<" --include="*.cs"

# Find all Configure<T> calls
grep -r "services.Configure<" --include="*.cs"
```

**For Each Options Class:**
1. **Class Name** - Full namespace and class name
2. **Configuration Section** - The section path (e.g., "ConnectionStrings:MongoDB")
3. **Properties** - All public properties with:
   - Property name
   - Type
   - XML documentation (if available)
   - Default value (from initializer)
   - Validation attributes ([Required], [Range], [RegularExpression], etc.)
4. **Used By** - Projects/services that consume this options class
5. **Registration** - Where/how it's registered (AddOptions, Configure, etc.)

**Example Output:**
```markdown
### MongoDbOptions

**Namespace:** `OoBDev.MongoDB.Configuration`
**Configuration Section:** `MongoDB` or `ConnectionStrings:MongoDB`
**Used By:** OoBDev.MongoDB, OoBDev.MongoDB.Tests

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| ConnectionString | string | Yes | - | MongoDB connection string |
| DatabaseName | string | Yes | - | Database name to connect to |
| MaxConnectionPoolSize | int | No | 100 | Maximum connection pool size |
| ServerSelectionTimeout | TimeSpan | No | 00:00:30 | Timeout for server selection |

**Validation:**
- ConnectionString: Required, must be valid MongoDB connection string
- DatabaseName: Required, must not be empty
- MaxConnectionPoolSize: Range [1, 1000]

**Registration:**
```csharp
services.Configure<MongoDbOptions>(configuration.GetSection("MongoDB"));
```
```

### Phase 2: Direct IConfiguration Usage

Search for direct configuration key access.

**Search Strategy:**
```bash
# Find configuration key access
grep -r "configuration\[" --include="*.cs"
grep -r "\.GetValue<" --include="*.cs"
grep -r "\.GetSection(" --include="*.cs"
grep -r "\.GetConnectionString(" --include="*.cs"
```

**For Each Configuration Key:**
1. **Key Path** - Full configuration path (e.g., "Logging:LogLevel:Default")
2. **Type** - Expected value type
3. **Default** - Default value if not specified
4. **Used In** - File and line number where accessed
5. **Purpose** - What this configuration controls

**Example Output:**
```markdown
### Direct Configuration Keys

| Key Path | Type | Default | Used In | Purpose |
|----------|------|---------|---------|---------|
| `Logging:LogLevel:Default` | string | "Information" | Startup.cs:45 | Default logging level |
| `AllowedHosts` | string | "*" | Startup.cs:23 | CORS allowed hosts |
| `Redis:ConnectionMultiplexer:Config` | string | - | ConnectionMultiplexerFactory.cs:16 | Redis connection string |
```

### Phase 3: Environment Variables

Search for environment variable usage.

**Search Strategy:**
```bash
# Find Environment.GetEnvironmentVariable usage
grep -r "Environment.GetEnvironmentVariable" --include="*.cs"
grep -r "GetEnvironmentVariable" --include="*.cs"

# Find ASPNETCORE_ and DOTNET_ references
grep -r "ASPNETCORE_" --include="*.cs" --include="*.json" --include="*.yml"
grep -r "DOTNET_" --include="*.cs" --include="*.json" --include="*.yml"
```

**For Each Environment Variable:**
1. **Variable Name** - Full environment variable name
2. **Type** - Expected value type
3. **Default** - Default if not set
4. **Platform** - All platforms, Windows-only, Linux-only, etc.
5. **Used In** - Where it's accessed
6. **Purpose** - What it controls

**Example Output:**
```markdown
### Environment Variables

#### Runtime Environment Variables

| Variable | Type | Default | Platform | Purpose |
|----------|------|---------|----------|---------|
| `ASPNETCORE_ENVIRONMENT` | string | "Production" | All | ASP.NET Core environment name |
| `DOTNET_ENVIRONMENT` | string | - | All | .NET environment name |

#### Framework-Specific Environment Variables

| Variable | Type | Default | Platform | Purpose |
|----------|------|---------|----------|---------|
| `MONGODB_CONNECTION_STRING` | string | - | All | MongoDB connection string (testing) |
| `REDIS_CONNECTION_STRING` | string | - | All | Redis connection string (testing) |
```

### Phase 4: Test Parameters

Search for test configuration parameters.

**Search Strategy:**
```bash
# Find test parameter usage
grep -r "TestContext.GetRequiredProperty" --include="*.cs"
grep -r "TestContext.GetPropertyOrDefault" --include="*.cs"

# Review .runsettings file
cat src/.runsettings
```

**Note:** Test parameters are already documented in TEST_VARIABLES.md, so reference that file instead of duplicating.

**Example Output:**
```markdown
### Test Configuration

Test parameters are documented separately in [TEST_VARIABLES.md](./TEST_VARIABLES.md).

Key test configuration patterns:
- Use `TestContext.GetRequiredProperty<T>("KEY")` for required values
- Use `TestContext.GetPropertyOrDefault("KEY", default)` for optional values with defaults
- All test parameters defined in `src/.runsettings`
```

### Phase 5: Connection Strings

Search for connection string patterns.

**Search Strategy:**
```bash
# Find connection string usage
grep -r "ConnectionStrings" --include="*.cs" --include="*.json"
grep -r "GetConnectionString" --include="*.cs"
```

**For Each Connection String:**
1. **Name** - Connection string name
2. **Format** - Expected format/provider
3. **Used By** - Services using this connection
4. **Example** - Example connection string (sanitized)

**Example Output:**
```markdown
### Connection Strings

| Name | Provider | Used By | Example Format |
|------|----------|---------|----------------|
| `DefaultConnection` | SQL Server | OoBDev.EntityFramework | `Server=localhost;Database=mydb;User Id=sa;Password=***` |
| `MongoDB` | MongoDB | OoBDev.MongoDB | `mongodb://localhost:27017/mydb` |
| `Redis` | Redis | OoBDev.Redis.Caching | `localhost:6379,password=***` |
```

### Phase 6: Service-Specific Configuration

Search for service-specific configuration files.

**Search Strategy:**
```bash
# Find JSON configuration files
find . -name "appsettings.json" -o -name "appsettings.*.json"

# Find YAML configuration files
find . -name "*.yml" -o -name "*.yaml"
```

**For Each Service:**
Document service-specific settings not covered by Options pattern.

---

## Document Structure

The CONFIGURATION_SETTINGS.md file should follow this structure:

```markdown
# OoBDev Framework - Configuration Reference

**Last Updated:** YYYY-MM-DD
**Framework Version:** [from GitVersion]

---

## Table of Contents

- [Overview](#overview)
- [Configuration Hierarchy](#configuration-hierarchy)
- [Options Pattern Classes](#options-pattern-classes)
  - [Database Configuration](#database-configuration)
  - [Caching Configuration](#caching-configuration)
  - [Messaging Configuration](#messaging-configuration)
  - [External Services](#external-services)
  - [AI/ML Services](#aiml-services)
- [Direct Configuration Keys](#direct-configuration-keys)
- [Environment Variables](#environment-variables)
  - [Runtime Environment](#runtime-environment)
  - [Framework-Specific](#framework-specific)
- [Connection Strings](#connection-strings)
- [Test Configuration](#test-configuration)
- [Provider-Specific Settings](#provider-specific-settings)
- [Validation Rules](#validation-rules)
- [Migration Guide](#migration-guide)

---

## Overview

This document provides a comprehensive reference for all configuration settings used across the OoBDev framework. Configuration follows the .NET Options Pattern and supports multiple configuration sources.

### Configuration Sources (Priority Order)

1. Command-line arguments
2. Environment variables
3. appsettings.{Environment}.json
4. appsettings.json
5. Default values in Options classes

### Configuration Naming Conventions

- **Options Classes**: `{Feature}Options` or `{Feature}Settings`
- **Configuration Sections**: Match the feature name (e.g., `MongoDB`, `Redis`, `Caching`)
- **Environment Variables**: UPPERCASE_WITH_UNDERSCORES
- **Configuration Keys**: PascalCase or colon-separated sections

---

## Configuration Hierarchy

```
appsettings.json
├── Logging
│   └── LogLevel
│       ├── Default
│       ├── Microsoft
│       └── System
├── ConnectionStrings
│   ├── DefaultConnection
│   ├── MongoDB
│   └── Redis
├── MongoDB
│   ├── ConnectionString
│   ├── DatabaseName
│   └── Options...
├── Redis
│   └── ConnectionMultiplexer
│       └── Config
├── Caching
│   ├── Provider
│   └── DefaultExpiration
└── [Service-specific sections...]
```

---

## Options Pattern Classes

[Generated from Phase 1]

### Database Configuration

#### MongoDbOptions
[Details from Phase 1]

#### SqlServerOptions
[Details from Phase 1]

### Caching Configuration

#### CachingOptions
[Details from Phase 1]

#### RedisCachingOptions
[Details from Phase 1]

### Messaging Configuration

#### RabbitMQOptions
[Details from Phase 1]

#### ServiceBusOptions
[Details from Phase 1]

[Continue for all Options classes organized by category]

---

## Direct Configuration Keys

[Generated from Phase 2]

---

## Environment Variables

[Generated from Phase 3]

---

## Connection Strings

[Generated from Phase 4]

---

## Test Configuration

See [TEST_VARIABLES.md](./TEST_VARIABLES.md) for complete test configuration reference.

**Quick Reference:**
- 30+ test parameters for integration testing
- Docker-based services configuration
- Live cloud services configuration

---

## Provider-Specific Settings

### MongoDB Provider

**Configuration Section:** `MongoDB`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| [Provider settings] | | | | |

### Redis Provider

**Configuration Section:** `Redis:ConnectionMultiplexer`

| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| [Provider settings] | | | | |

[Continue for all providers]

---

## Validation Rules

### Data Annotations

Common validation attributes used:
- `[Required]` - Value must be provided
- `[Range(min, max)]` - Numeric value range
- `[StringLength(max)]` - String length constraints
- `[RegularExpression(pattern)]` - Pattern matching
- `[EmailAddress]` - Valid email format
- `[Url]` - Valid URL format

### Custom Validation

[Document any IValidatableObject implementations or custom validators]

---

## Migration Guide

### From Legacy Configuration

If migrating from older configuration patterns:

**Old Pattern:**
```csharp
var connectionString = Environment.GetEnvironmentVariable("MONGODB_URL");
```

**New Pattern:**
```csharp
services.Configure<MongoDbOptions>(configuration.GetSection("MongoDB"));

public class MyService
{
    private readonly MongoDbOptions _options;

    public MyService(IOptions<MongoDbOptions> options)
    {
        _options = options.Value;
    }
}
```

### Configuration Key Changes

| Old Key | New Key | Version Changed |
|---------|---------|-----------------|
| [Document any breaking changes] | | |

---

## Related Documentation

- [TEST_VARIABLES.md](./TEST_VARIABLES.md) - Test configuration reference
- [Architecture Documentation](./docs/architecture/) - Framework architecture
- [Migration Guide](./docs/migration/) - Version migration guides

---

**Last Generated:** [Timestamp]
**Generated By:** Configuration Documentation Protocol v1.0.0
```

---

## Execution Steps

### Step 1: Create Search Tasks

Create a todo list for tracking discovery:

```markdown
- [ ] Phase 1: Discover all Options classes
- [ ] Phase 2: Find direct IConfiguration usage
- [ ] Phase 3: Catalog environment variables
- [ ] Phase 4: Reference test parameters
- [ ] Phase 5: Document connection strings
- [ ] Phase 6: Review service-specific configs
- [ ] Phase 7: Generate CONFIGURATION_SETTINGS.md
- [ ] Phase 8: Validate and review
```

### Step 2: Use Task Tool for Discovery

For each phase, use the Task tool with subagent_type=Explore:

```
Use Task tool to explore codebase for [Phase N objective]
- Search for pattern: [search pattern]
- Collect: [what to collect]
- Organize by: [organization criteria]
```

### Step 3: Generate Document Sections

For each category:
1. Create markdown table with all discovered items
2. Add examples and descriptions
3. Cross-reference related configuration
4. Note any validation rules

### Step 4: Create Cross-References

Link related configuration:
- Options classes → Configuration keys
- Environment variables → Options properties
- Connection strings → Provider options
- Test parameters → Runtime configuration

### Step 5: Validate Completeness

Check that documentation includes:
- [ ] All NuGet packages with configuration
- [ ] All external service integrations
- [ ] All provider-specific settings
- [ ] All test parameters
- [ ] All environment variables
- [ ] Validation rules for each setting
- [ ] Examples for complex configuration

### Step 6: Review and Format

- [ ] Verify all tables are properly formatted
- [ ] Check all internal links work
- [ ] Ensure consistent naming/casing
- [ ] Add helpful examples
- [ ] Include troubleshooting tips

---

## Tips for Effective Discovery

### Using Grep Patterns

**Find Options classes:**
```bash
# By naming convention
rg "class \w+Options" --type cs

# By IOptions usage
rg "IOptions<(\w+)>" --type cs -o

# By Configure calls
rg "Configure<(\w+)>" --type cs -o
```

**Find configuration keys:**
```bash
# Direct indexer access
rg 'configuration\["([^"]+)"\]' --type cs -o

# GetSection calls
rg '\.GetSection\("([^"]+)"\)' --type cs -o

# GetValue calls
rg '\.GetValue<[^>]+>\("([^"]+)"\)' --type cs -o
```

**Find environment variables:**
```bash
# GetEnvironmentVariable calls
rg 'GetEnvironmentVariable\("([^"]+)"\)' --type cs -o

# Common prefixes
rg '(ASPNETCORE_|DOTNET_|MONGODB_|REDIS_|RABBITMQ_)\w+' --type cs -o
```

### Organizing by Feature Area

Group configuration by architectural layer and feature:

**Common Layer:**
- Logging
- Diagnostics
- Health checks

**Framework Layer:**
- Caching
- Messaging
- Data access

**External Services:**
- MongoDB
- Redis
- RabbitMQ
- SQL Server
- Azure services
- AWS services
- AI/ML services

### Handling Duplicates

If the same configuration key is used in multiple places:
1. Document the primary/canonical usage
2. List all other usages with "See also" references
3. Note if usage patterns differ

### Documentation Best Practices

1. **Be Complete**: Include all settings, even internal ones
2. **Be Accurate**: Verify default values and validation rules
3. **Be Helpful**: Add examples and common patterns
4. **Be Organized**: Logical grouping and clear structure
5. **Be Current**: Timestamp the document and note version

---

## Example Workflow

**User:** "find all my configurations"

**Assistant Actions:**

1. **Acknowledge and Create Todo List**
   ```
   I'll discover and document all configuration settings across the framework.
   Creating todo list for 8 phases...
   ```

2. **Phase 1: Options Classes**
   - Use Task tool to search for all *Options.cs files
   - Read each Options class
   - Extract properties, validation, XML docs
   - Organize by feature area

3. **Phase 2: Direct Configuration**
   - Use Task tool to search for configuration["key"] patterns
   - Search for GetSection, GetValue, GetConnectionString
   - Catalog all direct configuration access
   - Map to Options classes where possible

4. **Phase 3: Environment Variables**
   - Search for Environment.GetEnvironmentVariable
   - Search for ASPNETCORE_, DOTNET_ prefixes
   - Review Docker compose files for env vars
   - Cross-reference with Options classes

5. **Phase 4-6: Complete Discovery**
   - Review test parameters (reference TEST_VARIABLES.md)
   - Document connection strings
   - Review service-specific configuration files

6. **Phase 7: Generate Document**
   - Create CONFIGURATION_SETTINGS.md
   - Populate all sections
   - Add cross-references
   - Include examples

7. **Phase 8: Validate**
   - Review for completeness
   - Check formatting
   - Verify links
   - Add to documentation index

**Output:** CONFIGURATION_SETTINGS.md with complete configuration reference

---

## Maintenance

### When to Update

Update CONFIGURATION_SETTINGS.md when:
- New Options classes are added
- Configuration keys change
- New environment variables are introduced
- Provider configuration changes
- After major feature releases

### Validation Checklist

Before finalizing the document:

- [ ] All Options classes documented
- [ ] All configuration sections covered
- [ ] Environment variables cataloged
- [ ] Connection strings documented
- [ ] Validation rules included
- [ ] Examples are accurate
- [ ] Cross-references work
- [ ] Table of contents is complete
- [ ] Document is formatted properly
- [ ] Version/timestamp is current

---

## Integration with Other Documentation

### Cross-References

**From CONFIGURATION_SETTINGS.md:**
- Link to TEST_VARIABLES.md for test parameters
- Link to provider-specific README files
- Link to architecture documentation
- Link to migration guides

**To CONFIGURATION_SETTINGS.md:**
- Update README.md with link to configuration reference
- Add to documentation index
- Reference from getting started guides
- Include in API documentation

### Documentation Structure

```
src/
├── CONFIGURATION_SETTINGS.md (NEW - This protocol creates this)
├── TEST_VARIABLES.md (Existing - Test configuration)
├── README.md (Update with link)
├── docs/
│   ├── architecture/
│   │   ├── caching/configuration.md (Link back to CONFIGURATION_SETTINGS.md)
│   │   └── [other features]/configuration.md
│   └── migration/
│       └── configuration-changes.md (Link to CONFIGURATION_SETTINGS.md)
└── .runsettings (Reference in CONFIGURATION_SETTINGS.md)
```

---

## Advanced Discovery Techniques

### Finding Options Registration

```bash
# Find where Options are configured
rg "services\.Configure<" --type cs -A 3

# Find Options pattern registration
rg "services\.AddOptions<" --type cs -A 3

# Find OptionsBuilder usage
rg "services\.AddOptions<(\w+)>" --type cs -o
```

### Finding Validation

```bash
# Data annotation attributes
rg "\[Required\]|\[Range|\[StringLength|\[RegularExpression" --type cs

# IValidatableObject implementations
rg "class \w+ : IValidatableObject" --type cs

# IValidateOptions implementations
rg "class \w+ : IValidateOptions<" --type cs
```

### Finding Default Values

```bash
# Property initializers
rg "public \w+ \w+ \{ get; set; \} = " --type cs

# Constructor initialization
rg "public \w+Options\(\)" --type cs -A 10
```

### Finding Configuration Binding

```bash
# Bind method usage
rg "\.Bind\(" --type cs

# GetSection with Bind
rg "configuration\.GetSection\([^)]+\)\.Bind" --type cs
```

---

## Output Quality Standards

### Required Information for Each Setting

Every configuration setting must include:

1. **Key/Path** - Full configuration path
2. **Type** - Data type expected
3. **Required** - Yes/No/Conditional
4. **Default** - Default value or "None"
5. **Description** - Clear explanation of purpose
6. **Example** - Valid example value
7. **Validation** - Any validation rules
8. **Used By** - Which services/features use it

### Table Format Standards

Use consistent table formats:

```markdown
| Setting | Type | Required | Default | Description |
|---------|------|----------|---------|-------------|
| ConnectionString | string | Yes | - | MongoDB connection string with authentication |
| MaxPoolSize | int | No | 100 | Maximum number of connections in pool |
```

### Code Example Standards

Provide clear, copy-paste examples:

```csharp
// Good example - complete and runnable
services.Configure<MongoDbOptions>(options =>
{
    options.ConnectionString = "mongodb://localhost:27017";
    options.DatabaseName = "myapp";
    options.MaxConnectionPoolSize = 200;
});

// Show JSON configuration equivalent
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "myapp",
    "MaxConnectionPoolSize": 200
  }
}
```

---

## Troubleshooting Discovery

### Issue: Too Many Results

**Problem:** Search returns hundreds of results

**Solution:**
- Narrow search to specific directories (`src/Framework/`, `src/ExternalServices/`)
- Exclude test files initially: `--glob '!*.Tests.cs'`
- Search by feature area one at a time
- Use more specific patterns

### Issue: Inconsistent Naming

**Problem:** Configuration keys use different naming conventions

**Solution:**
- Document both patterns in migration guide
- Note preferred/canonical pattern
- List all variations in "Also Known As" section
- Add migration guide for standardization

### Issue: Missing Documentation

**Problem:** Options classes lack XML documentation

**Solution:**
- Infer purpose from class/property names
- Review usage in code
- Note "Documentation needed" in output
- Mark as technical debt

### Issue: Duplicate Configuration

**Problem:** Same setting in multiple places

**Solution:**
- Document primary source
- List all locations
- Explain precedence/priority
- Recommend consolidation if appropriate

---

## Related Protocols

- [Documentation Style Guide](./documentation-style-guide.md) - Writing standards
- [Documentation Standards](./documentation-standards.md) - File organization
- [Integration Test Maintenance](../software/integration-test-maintenance.md) - Test parameters

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-01-21 | Initial protocol creation |

---

**Protocol Owner:** Documentation Team
**Review Cycle:** Quarterly or after major configuration changes
**Next Review:** 2026-04-21