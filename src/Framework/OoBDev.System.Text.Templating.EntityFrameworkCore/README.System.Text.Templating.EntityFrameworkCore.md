# OoBDev.System.Text.Templating.EntityFrameworkCore

Versioned, multi-tenant, culture-aware template storage in a relational database, served to the template engine as an `ITemplateSource`.

| Type | Role |
|------|------|
| `TemplatesDbContext` / `TemplateRow` | The `Templates` table, unique on `(Name, Culture, TenantId, Version)`. A published version is never changed. |
| `EfTemplateRepository` | `PublishAsync` (next version number, retried when two publishers collide), `GetAsync` (latest or a specific version; the tenant's own template wins over the shared one), `GetHistoryAsync`, `RollbackAsync` (republishes old content as a new version, history stays linear), `WithdrawAsync` (hides every version, keeps the audit trail). |
| `EfTemplateSource` | `ITemplateSource`: `welcome` (latest), `welcome@2` (version 2), `welcome.es-ES` (culture, the form `LocalizedTemplateEngine` asks for). |
| `EfTemplateSourceOptions` | `TenantId` and `Priority`. |

```csharp
services.TryAddEntityFrameworkTemplates((sp, o) => o.UseSqlite("Data Source=templates.db"));
services.Configure<EfTemplateSourceOptions>(o => o.TenantId = "acme");
```

The consumer picks the database provider. Tables are created with `EnsureCreated` or the consumer's own migrations (shipped migrations are not built). Culture and tenant are empty strings when absent. The audit trail is `CreatedBy`, `CreatedAtTicks` and `Description` on each row.

Tests: `OoBDev.System.Text.Templating.EntityFrameworkCore.Tests` (Simulate, Sqlite in memory). Not run: SQL Server and PostgreSQL.
