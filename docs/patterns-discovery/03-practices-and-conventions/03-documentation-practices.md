# Documentation Practices

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Build and Project File Conventions](./02-build-and-csproj.md) · [Testing Practices →](./04-testing-practices.md)
<!-- nav -->

**Table 4 — Documentation artifacts**

| Artifact | Rule | Where |
|----------|------|-------|
| Project readme | One per packable project, named `README.{Name}.md` (in-repo files are written `ReadMe.{Name}.md`), purpose plus a usage example plus configuration keys | project folder |
| XML documentation | On public APIs; the documentation-file switch is currently commented out in the shared props | source |
| Architecture docs | Layer rules, standards, patterns | `docs/architecture/` |
| How-tos | Practical guides such as `.runsettings` variables | `docs/how-tos/` |
| Change records | Completed work archived, index kept | `docs/changes/` |
| Design docs | Four per feature, see [the design document standard](../06-design-document-standard.md) | `docs/` |
| Tracking | Current work and status | `TODO.md` |
| Configuration reference | Every `IConfiguration`, `IOptions` and environment variable | `CONFIGURATION_SETTINGS.md` |
| Test variables | Every test property | `TEST_VARIABLES.md` |

## Rules of thumb

- A README documents the *registration call*, the *configuration keys* and one *minimal usage* example.
- Diagrams are PlantUML (C4 style for architecture); mockups are PlantUML Salt. Validate with `python scripts/docs/validate-docs.py docs` ([scripts README](../../../scripts/docs/README.md)).
- Prefer relative cross references to prose pointers; split long documents into folders.
- Update `TODO.md` in the same change that finishes work; archive it into `docs/changes/` when files pass about 400 lines.
- Documentation states what the code *does*; where older documents disagree, record it in [Doc/Code Drift](../README.md).

## Protocols

Repeatable documentation tasks have written protocols in `.claude/protocols/` (documentation style, standards, archival, configuration discovery). Use the protocol rather than improvising.

---

<!-- nav -->
[↑ 03 — Practices and Conventions](./README.md) · [← Build and Project File Conventions](./02-build-and-csproj.md) · [Testing Practices →](./04-testing-practices.md)
<!-- nav -->
