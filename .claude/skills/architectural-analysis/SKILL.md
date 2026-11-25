---
name: architectural-analysis
description: Analyze and document the architecture of a component or project (layers, patterns, dependencies, decisions), then verify completeness. Use for "analyze architecture of {component}", "document architecture", periodic architecture reviews, or onboarding documentation.
---

# Architectural analysis

Start from what already exists: `docs/architecture/` and `docs/patterns-discovery/` (the owner's patterns are recorded, not corrected; rough edges go to `TODO.md` backlogs). The generated project catalog is in `docs/patterns-discovery/07-project-catalog`.

1. Scope the component and read its code, project files and readme; use search tools and cite file paths.
2. Document: context and containers (C4 style, plain PlantUML), components, dependencies and layer fit, patterns used (provider/factory, DI, options, handler), data and message flows, configuration, tests and coverage, and known gaps.
3. Check against `completeness-checklist.md` (technical, operational, security, testing, ownership perspectives) and list what is missing instead of inventing it.
4. Write to `docs/architecture/` (split long documents into a folder with a README index), follow the `docs-and-diagrams` rule, run the `OoBDev.Docs.Tests` project on the folder (`DOCS_VALIDATE_PATHS`).
5. Report findings, gaps and proposed `TODO.md` backlog items.

`reference.md` has the full original procedure and document templates.
