# Docs check for imported material (2026-11-03)

`OoBDev.Docs.Tests` now fails when a markdown file under `docs/applications/{app}/` is orphaned.

## Done

- `ImportedDocsCoverage.FindOrphans` treats a file as covered when the application readme reaches it through links (a link to a folder covers everything below it) or when a document in `docs/migration` names the file or one of its folders. An application folder without a `README.md` is reported.
- `Applications_ImportedDocuments_AreLinkedOrMapped` runs it for every application folder; `ImportedDocsCoverageTests` covers the rules (linked, folder-linked, mapped, orphan, no readme) against a throwaway tree.
- The default scope widens to `docs/patterns-discovery;docs/applications` (`DocsTests`, `src/.runsettings`, `TEST_VARIABLES.md`, the style guide and `CLAUDE.md`).
- First run found 21 `docs/applications/devterm/changes` files that nothing linked; the devterm readme now links the `devices`, `bugs`, `changes` and `source` folders (and no longer names a `scratchpad` folder that does not exist).

## Verified

`dotnet test src/Tools/OoBDev.Docs.Tests --filter "TestCategory=Unit"`: 8 passed, 0 failed. The PlantUML render test (Integration) was not run.
