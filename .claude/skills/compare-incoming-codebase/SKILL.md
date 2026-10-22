---
name: compare-incoming-codebase
description: Systematically compare an incoming codebase (Incoming/ directory, BinaryDataDecoders, ContractParser, Tools, BotChat) with the main OoBDev codebase and produce a feature mapping, migration plan, bug report and tracking checklist. Use for "compare {project} with main", "analyze incoming code", "migration analysis for {project}".
---

# Compare incoming code with main

Policy: all BinaryDataDecoders features migrate; phases are priority order, not selection. Only stub projects with zero implementation, obsolete-platform code (Silverlight) and features with no net10.0 equivalent are deleted. Incomplete features migrate as-is and are tracked.

1. Read `Incoming/CHECKLIST.md`, `/TODO.md`, `docs/architecture/architectural-standards.md`, and `docs/migration/`.
2. **Find the latest source first.** For BinaryDataDecoders the newest copy is `C:\repo\mine\binary-data-decoders` (78 projects), and `Incoming/BinaryDecoders` is a 41-project renamed subset that can be behind. Device libraries may also be newer in `C:\repo\oobdev\dev-term`. Say which copy you compared and its commit.
3. Classify each incoming project (Foundation, Feature, Specialized, UI, Test, Stub) and map each feature to main: Match, Partial, Gap, Superseded, Delete. Evidence is file paths from searching, never guesses.
4. Place gaps in the right layer (Common, Framework, Extensions, ExternalServices), noting provider/factory and `TryAdd*` DI needs, package separation for specialized features, and security warnings (for example broken classic ciphers).
5. Write outputs under `docs/migration/` using the templates in `reference.md` (PlantUML diagrams only), update the checklist in `Incoming/CHECKLIST.md`, and add prioritized items to `TODO.md`.
6. Apply the design-first approach for SharedFramework-style features: produce the four design documents (`docs/patterns-discovery/06-design-document-standard.md`) instead of porting code.
7. Surface decisions the owner must make as a short list. Do not start migrating without them.

`reference.md` is the full 1,400-line procedure with templates; open only the section you need.
