---
name: document-configuration
description: Discover and document every IConfiguration key, IOptions class, environment variable, connection string and test parameter, producing CONFIGURATION_SETTINGS.md. Use when asked to "find all my configurations", "document configurations", or "create configuration reference".
---

# Document configuration settings

Output: `CONFIGURATION_SETTINGS.md` at the repo root (existing file; update it in place, do not start over).

1. Scan `src/` for `IOptions<T>` / `IOptionsMonitor<T>` classes, `AddValidatedOptions<T>()` and `Configure<T>` calls, `IConfiguration` key reads (including provider selection paths such as `OoBDev:CachingProvider:Type`), `Environment.GetEnvironmentVariable`, connection strings, and `appsettings*.json` / `.runsettings` / `.env.integration` entries. Use the search tools, not memory.
2. For each setting record: key path, type, default, required or optional, owning project, and where it is read. Group by project, then by feature.
3. Mark secrets and credentials (never print real values), settings that differ between test and production, and any setting that is read but undocumented.
4. Diff against the existing `CONFIGURATION_SETTINGS.md`: add new, mark removed, keep the existing structure and header (counts, generated date, framework version from GitVersion).
5. Cross-check test properties against `TEST_VARIABLES.md` and `src/.runsettings`.
6. Report counts added, changed, removed. Do not commit unless asked.

`reference.md` has the original search patterns and the document template.
