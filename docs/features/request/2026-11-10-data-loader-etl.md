# Feature request: data loader (ETL pipeline and CLI)

- **Status:** triaged
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Framework` (abstractions and pipeline) and `src/Tools` (CLI)

## Need

Load data files (CSV, JSON) into a database through a validated, prioritized pipeline, with duplicate handling, lookups by alternative key and master data. dotex has no ETL or seed-data loader.

## Proposed behavior

- File providers and readers (CSV, JSON) behind interfaces.
- A pipeline of handlers ordered by `DataPipelinePriority`, with source validation.
- Duplicate and import handling options; entity and entity-type models; lookup by property or query.
- A DbContext provider and a master data provider; a database deployment template.
- A CLI that runs a load.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.DataLoader.Abstractions` (28 files)
- `OoBDev.DataLoader` (20 files: readers, pipeline, lookups, master data, `DocumentUpload`, `GetTextData`)
- `OoBDev.DataLoader.Cli` (2 files)
- `OoBDev.DataLoader.Tests`

## Acceptance criteria

- Given a CSV and a JSON file with a duplicate row, then the configured handling (skip, replace, fail) is applied and reported.
- Given an invalid row, then it is reported with file and line and the rest still load (or the load stops, per option).
- Given a database, then integration tests run against the SQL Server container.

## Existing code checked

Searched `src` for `IDataPipelineHandler`, `DataLoader`, `IDataFileReader` and `DuplicateHandling`: nothing. `docs/design/MasterData` and `DataEnhancement` describe related master data; `MasterData` is a design only.

## Security and operations

Loads write to a database: run in a transaction, support a dry run, and never run against live data without an explicit switch. `AssemblyResolver` in the source loads assemblies at runtime; treat that as untrusted input.

## Priority

- **Impact:** repeatable seed and migration data loads.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

**Answered (checked 2026-11-23):** owner Q18 (assumed answer A in the [design](../../design/DataLoader/README.md)): a library only, no command line, stands alone until the MasterData design is built; the master data provider waits for that build.

Does this wait for the MasterData design (Epic 5) or stand alone? Is the CLI wanted (see the CSV test data utility request: related reader code)?

## Triage notes

Origin: SharedFramework. Not covered by any design; design comes first.

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.

2026-11-19: proposed design written: [DataLoader](../../design/DataLoader/README.md).
