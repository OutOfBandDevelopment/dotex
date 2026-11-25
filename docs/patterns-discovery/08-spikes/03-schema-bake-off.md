# Spike — Desired-state database bake-off as repeatable tests

<!-- nav -->
[↑ 08 — Spikes](./README.md) · [← Spike — Aspire versus Docker Compose](./02-aspire-vs-compose.md) · [Index →](./README.md)
<!-- nav -->

## Question

The [bake-off result](../../design/DesiredStateDatabase/bake-off-result.md) was a manual run in a scratch folder. Can its scenarios be replayed by tests so a tool upgrade or a new candidate is scored the same way?

**Status:** built 2026-11-24 for sqldef on SQLite (`src/Spikes/OoBDev.Spike.SchemaBakeOff`, sqldef 3.11.30). The other columns are not built.

## Result

**Table 1 — Spike results**

| Scenario | Test | Result |
|----------|------|--------|
| S1, S2 | `S1S2_ApplyToEmptyThenAgain_CreatesTablesThenChangesNothing` | Pass; `--check` exits 0 on a synced database |
| S3 | `S3_AddColumnIndexAndView_PlansExactlyThoseSteps` | Pass |
| S5 | `S5_DropTableWithoutTheFlag_IsSkippedAndDroppedWithIt` | Pass; a drop needs `--enable-drop` |
| S6 | `S6_NarrowAType_NeverNamesTheHazard` | Pass (documents the gap: no hazard is named, so the layer-2 `PlanAnalyzer` is needed) |
| S8 | `S8_ChildScriptBeforeParent_IsReordered` | Pass |
| S9 | `S9_ViewChange_IsPlannedAndAnUnchangedSchemaPlansNothing` | Pass |
| S11 | `S11_HandAddedColumn_IsNotDroppedWithoutTheFlag` | Pass; `--check` exits 2 on drift |
| S12 | `S12_ExitCodes_AreUsableFromDotNet` | Pass; the plan is SQL text and exit codes carry the state |

All eight tests pass and agree with the manual SQLite column of the result. S4 (rename), S7 and S10 are not replayed: S4 needs the `@renamed` annotation, S7 only has the dry-run text, and SQLite has no extensions.

## Not built

- **pgschema and the container engines (Atlas dropped, owner 2026-11-24):** the separate stack in [`containers/bakeoff`](../../../containers/bakeoff/README.md) (built and started 2026-11-24: sqldef and pgschema from pinned tags with checksums, PostgreSQL 17.7, a scratch Postgres and SQL Server 2022) now supplies the binaries and databases. The scenario harnesses for those columns are not written yet; the harness is SQLite-only today. Atlas SQLite and SQL Server were never run.
- **Maintenance score:** not a test.

## Verdict

The replay is cheap and deterministic for SQLite, so keep it as the regression check for sqldef upgrades. Extending it to the container engines is a separate item.
