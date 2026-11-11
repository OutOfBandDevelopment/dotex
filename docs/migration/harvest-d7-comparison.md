# D7 harvest candidates — comparison

[↑ Migration plan](README.md#harvest-candidates)

**Status:** Proposal for owner decision · **Date:** 2026-11-10 · Sources under `C:\repo\mine\proving-grounds\examples`. Nothing is copied by this document.

## Contents

- [Summary](#summary)
- [Findings](#findings)

### List of Tables

1. [Table 1 — Recommendation per candidate](#table-1)

## Summary

Compared by file listing and reading the key files against `src`. Six of the seven are already covered, trivial or small enough to decide at once. Oobtainium is already decided (not migrated; see the plan) and `postsql-playground` is not on this disk, so it was not compared.

<a id="table-1"></a>**Table 1 — Recommendation per candidate**

| Candidate | Already in dotex | Recommendation |
|-----------|------------------|----------------|
| markdownplantuml | `OoBDev.System.Text.Markdown` holds the same seven `PlantUml*` classes | Decline: fully migrated. Mark the source as moved |
| TotpExample | `OoBDev.System.Security.Cryptography.OneTimeCode` and `Codecs/Base32Codec` with tests | Decline: covered. The XML-signing sample and the license provider are old .NET Framework samples |
| HandyClasses | `Code39` (`OoBDev.Extensions.Drawing`, SkiaSharp), `ConsoleEx` (`OoBDev.System`), `XFragment` (`OoBDev.System.Xml.Linq`), MailKit adapter (`OoBDev.MailKit`) | Nothing to copy: `Code39`, `ConsoleEx`, `XFragment` and the SMTP service are already migrated, so the source copies are not taken again (not a rejection). **Open:** `CsvWriter` and `IniFile` have no equivalent; migrate them as small `OoBDev.System` features if wanted |
| BuildFirstOnce | Not used | **Open:** the `Directory.Build.targets` tasks `DeepClean` and `CleanPaths` (`dotnet clean /T:DeepClean`) would help the 112-project solution. The "build a tool first, once" trick has no current use; record it as a technique only |
| hybridsearch | `OoBDev.Search`, `Qdrant`, `OpenSearch`, `Ollama`, `SBert`, `Documents` via Tika, `Markdig`, `WkHtmlToPdf` | Mostly covered (see [HybridSearch docs](../design/HybridSearch/README.md)). **Open:** the Azure, Neo4j and Go variants have no equivalent; the lexical-plus-vector merge is already tracked in `BACKLOG.md` |
| postsql-playground | Not on this disk | Keep as a design input for the PostgreSQL support feature request; compare when the repository is available |

## Findings

- **Decline outright (3):** markdownplantuml, TotpExample, and the covered parts of HandyClasses. After the owner agrees, add a "moved" note to each source README as was done for Ser2NetWin.
- **Small features to decide (3):** `CsvWriter`, `IniFile`, and the `DeepClean` build targets. Each would be a feature request with tests (Framework layer, 80%).
- **Large and already tracked (1):** the hybrid merge, with the Azure and Neo4j providers as optional later adapters.
- Hybridsearch's Go version is a language port of the same design; no action, except to keep it as a read-only reference.
