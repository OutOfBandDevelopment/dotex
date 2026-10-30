# DevTerm surroundings and documentation layout (2026-10-30)

Reviewed everything around dev-term's code and carried it into this repository's layout (owner rule: migrate the whole project, with application documents under `docs/applications/{app-name}/` as living documents, no decision records).

- **Documentation:** `docs/applications/devterm/` holds the living requirements, architecture, API design and testing strategy (moved from `docs/design/DevTermParity/`), the screen specs, the user guide, bench reports and the imported dev-term design material; protocol references moved to `docs/protocols/`. The transient [documentation map](../migration/devterm-docs-map.md) lists where each imported folder ends up and is deleted when done.
- **Decisions:** [surroundings review](../migration/devterm-surroundings-review-2026-10-30.md) records every item as migrated, merged or rejected with the reason (skills `hardware-test` and `device-manual` adopted, `docs-sync` merged into the `work-tracking` rule, `bug-report`, `work-docs-audit` and `merge-gate.sh` rejected).
- **Carried over:** containers (`containers/devterm`), scripts (`scripts/devterm`), snapshots of the project guide, TODO, BACKLOG and open questions, and the coding-standards tests as reference.
- **Rules:** `work-tracking` gained the application documentation line; `coding-standards` points at dev-term's recorded hardware constraints.
- **Future plans** went into `BACKLOG.md` section 1.
- **Verified:** documentation validator passes (5 of 5) for `docs/applications`, `docs/protocols`, `docs/migration` and `docs/design/DevTermParity`. Links inside the imported `source/` and `changes/` snapshots still use dev-term paths and are not repaired.
