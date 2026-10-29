# Documentation Style Guide

The conventions every document in this repository follows. Where a rule can be checked by a machine it is: `src/Tools/OoBDev.Docs.Tests` (Unit tests) and the always-on rule `.claude/rules/docs-and-diagrams.md`.

**Table 1 — Style rules and how they are enforced**

| Rule | Enforced by |
|------|-------------|
| Diagrams are PlantUML, mockups are PlantUML Salt, no remote `!include` | `PlantUml_Blocks_AreWellFormed`, `PlantUml_Blocks_Render` (Integration) |
| Relative links resolve | `Links_Relative_Resolve` |
| `*Figure N — title*` and `**Table N — title**` captions are sequential | `Captions_FiguresAndTables_AreSequential` |
| Project readmes are `README.{Name}.md` (upper-case `README`) | `Names_Documents_FollowStyleGuide` |
| Numbered names are only used under `docs/patterns-discovery` | `Names_Documents_FollowStyleGuide` |
| Root holds only the allowed files | rule `work-tracking` |

## Names

- **Reading order numbers** (`01-`, `02-`) are used only inside `docs/patterns-discovery/`, where the order of reading matters. Every other folder uses descriptive kebab-case names, with no number prefix.
- **Project readmes** are `README.{Project}.md` beside the project file. The same spelling applies to copies under `docs/code/`.
- **Folders** with more than one document have a `README.md` index.
- **Change documents** are `docs/changes/{epic}-{feature}-{YYYY-MM-DD}.md` and are listed in `docs/changes/README.md`.
- **Feature documents** are `docs/design/<Feature>/{requirements,architecture,api-design,testing-strategy}.md` ([standard](../patterns-discovery/06-design-document-standard.md)).
- Generated output lives under `docs/generated/` and is not hand edited or validated.

## Structure

- Add a table of contents, list of figures and list of tables when a document has 3 or more sections, 2 or more figures or 2 or more tables.
- Split a document longer than about 250 lines or 6 major sections into a folder with a `README.md` index and nav links.
- Use relative links (file plus `#anchor`), not prose pointers.
- State what the code does; record differences from older documents in the drift table instead of hiding them.

## Diagrams and captions

- Architecture diagrams are C4 style in plain PlantUML; UI mockups are Salt.
- Caption a figure with `*Figure N — title*` after it and a table with `**Table N — title**` before it; number each in order within the document.

## Checking a change

```bash
DOCS_VALIDATE_PATHS=docs/how-tos dotnet test src/Tools/OoBDev.Docs.Tests --filter "TestCategory!=Integration"
```

The default scope is `docs/patterns-discovery`; widen it with `DOCS_VALIDATE_PATHS` (semicolon separated). The whole `docs/` tree still has older problems, so check the folder you changed.
