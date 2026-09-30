# Patterns Discovery

Architecture, design patterns and practices of the OoBDev (dotex) framework, written down so that future products and frameworks can be built the same way, followed by a list of industry alternatives with pros and cons.

The current patterns are the author's discovered preferences. Sections 01 to 04 document them as the standard to carry forward. Section 05 is where alternatives are argued.

## Sections

**Table 1 — Document set**

| Section | Purpose |
|---------|---------|
| [01 Architecture](./01-architecture/README.md) | Shape, layers, composition root, hosting, build and principles |
| [02 Design patterns](./02-design-patterns/README.md) | 20 patterns with code references, plus how they interact |
| [03 Practices and conventions](./03-practices-and-conventions/README.md) | Naming, build, documentation, testing, logging, CI/CD, known warts |
| [04 New project blueprint](./04-new-project-blueprint/README.md) | Step-by-step recipes for a capability, adapter, application and framework repository |
| [05 Industry alternatives](./05-industry-alternatives/README.md) | Different industry approaches, pros and cons, verdict per topic |
| [06 Design document standard](./06-design-document-standard.md) | How every document in this set (and future design documents) is written |

## Method

1. Read the existing docs, `CLAUDE.md` and `TODO.md`.
2. Read the code: project files, props, registration extensions, tests, workflows.
3. Record only what the code confirms; note differences from the docs as drift (below).
4. Validate every document with `python scripts/docs/validate-docs.py docs/patterns-discovery` (renders each PlantUML diagram, checks links and captions). See [scripts/docs/README.md](../../scripts/docs/README.md).

## Doc/Code Drift

Places where existing documentation and the code disagree. The new documents follow the code.

**Table 2 — Drift found**

| Item | Docs say | Code shows | Suggested action |
|------|----------|------------|------------------|
| GitVersion location | `CLAUDE.md`: `/src/GitVersion.yml` | `GitVersion.yml` at the repository root | Fix `CLAUDE.md` |
| Integration workflow | CLAUDE.md: runs daily at 4 PM UTC | The cron trigger is commented out | Enable the schedule or correct the text |
| MailKit | Described as a common dependency | Compiled into Common only in Debug and disabled by the example app | Decide and document |
| `ContractConfig` attribute | Present in the API | Unused | Remove or document |
| Readme file names | Packed as `README.{Name}.md` | Files are named `ReadMe.*.md` | Rename for case-sensitive systems |
| Project counts | CLAUDE.md: 112+ projects | 123 project files | Refresh counts |
| `docs/architecture` | Presented as current | Older than the code in places | Review against these documents |
| CLAUDE.md status sections | Docker services 13/15 healthy, 14 services | 15 services listed elsewhere | Refresh |

The full list of code-level issues is in [known warts](./03-practices-and-conventions/07-known-warts.md).

## Reading order

New to the codebase: 01, then 02, then 03. Building something new: 04. Deciding what to change: 05.

<!-- toc:start -->
<!-- toc:end -->
