---
name: compare-incoming-codebase
description: Systematically compare an incoming codebase (Incoming/ directory, BinaryDataDecoders, ContractParser, Tools, BotChat) with the main OoBDev codebase and produce a feature mapping, migration plan, bug report and tracking checklist. Use for "compare {project} with main", "analyze incoming code", "migration analysis for {project}".
---

# Compare incoming code with main

Policy: all BinaryDataDecoders features migrate; phases are priority order, not selection. Only stub projects with zero implementation, obsolete-platform code (Silverlight) and features with no net10.0 equivalent are deleted. Incomplete features migrate as-is and are tracked.

1. Read `Incoming/CHECKLIST.md`, `/TODO.md`, `docs/architecture/architectural-standards.md`, and `docs/migration/`.
2. **Find the latest source first.** For BinaryDataDecoders the newest copy is `C:\repo\mine\binary-data-decoders` (78 projects), and `Incoming/BinaryDecoders` is a 41-project renamed subset that can be behind. Device libraries may also be newer in `C:\repo\oobdev\dev-term`. Say which copy you compared and its commit.
2a. **Review the whole project, not only code** (owner, 2026-10-30): the project guide, `.claude/` (rules, skills, hooks, settings), docs, open questions, TODO and BACKLOG (future plans), UIs, tests, scripts and run settings. Record each as migrated, merged or rejected with the reason.
3. Classify each incoming project (Foundation, Feature, Specialized, UI, Test, Stub) and map each feature to main: Match, Partial, Gap, Superseded, Delete. Evidence is file paths from searching, never guesses.
4. Place gaps in the right layer (Common, Framework, Extensions, ExternalServices), noting provider/factory and `TryAdd*` DI needs, package separation for specialized features, and security warnings (for example broken classic ciphers).
5. Write outputs under `docs/migration/` using the templates in `reference.md` (PlantUML diagrams only), update the checklist in `Incoming/CHECKLIST.md`, and add prioritized items to `TODO.md`.
6. Apply the design-first approach for SharedFramework-style features: produce the four design documents (`docs/patterns-discovery/06-design-document-standard.md`) instead of porting code.
7. Surface decisions the owner must make as a short list. Do not start migrating without them.

`reference.md` is the full 1,400-line procedure with templates; open only the section you need.

## Documentation and surroundings checklist (pattern approved by the owner 2026-10-30)

Per migrated project, in the same series as the code. The DevTerm run is the worked example: [review](docs/migration/README.md#surroundings-guides-agent-config-scripts) and [map](docs/migration/README.md#devterm-ui-shells-and-plugins).

1. Import its docs whole under `docs/applications/{app-name}/` (reusable designs later to `docs/design/<Feature>/`, protocol references to `docs/protocols/`); keep change history and bug reports.
2. Write the transient map `docs/migration/{app}-docs-map.md` (folder, now at, living home, state); it is deleted when the move is complete.
3. Add the surroundings rows to the "Surroundings" table of `docs/migration/README.md`: guide, `.claude` (rules, skills, hooks, settings), TODO, BACKLOG, open questions, containers, scripts, tests, each migrated, merged or rejected with a reason.
4. Constraints from its project guide become a path-scoped rule in `.claude/rules/` (see `devterm-constraints`); its skills are adapted, not copied blindly.
5. Future plans become one-line items in `BACKLOG.md`; open questions go to `OPEN-QUESTIONS.md`.
6. Guides and screenshots are produced by tests and land in `docs/applications/{app}/user-guide` (design: `docs/design/GeneratedGuides/`).
7. Do not clean up imported history; cleanup is one plan after all migrations (rule `owner-decisions`).
