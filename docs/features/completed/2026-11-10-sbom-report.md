# Feature request: software bill of materials report

- **Status:** done (2026-11-16)
- **Requested by:** owner (found while cleaning `docs/templates`, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** build, `OoBDev.TemplateEngine.Cli`, `docs/`

## Need

The security practices ([12-security-practices](../../patterns-discovery/03-practices-and-conventions/12-security-practices.md)) and the `write-docs` skill say an SBOM is generated per build, but nothing produces one now. `docs/templates/SoftwareBillOfMaterials.md.xslt` turns a CycloneDX 1.6 SBOM into markdown; the old `docs/sbom` output is gone.

## Proposed behavior

A repeatable command writes a CycloneDX SBOM for the solution and a markdown report from it (component count, scope, licences, versions). The report step runs through `TemplateEngine.Cli` with the XSLT template, and the output is deterministic.

## Acceptance criteria

- Given the solution, when the SBOM script runs, then a CycloneDX file and a markdown report are written to a fixed folder.
- Given two runs on the same commit, then the markdown is identical.
- Given a package version change in `Directory.Packages.props`, then the report shows it.

## Existing code checked

`TemplateEngine.Cli` runs XSLT and Handlebars templates, so the report step exists. No CycloneDX tool is wired; the [framework wish list](../request/2026-11-10-framework-wish-list.md) keeps the CycloneDX and Microsoft sbom-tool links.

## Security and operations

Reads package metadata only; no network beyond package restore.

## Priority

- **Impact:** a stated security practice is not met.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

CycloneDX dotnet tool or the Microsoft sbom-tool (SPDX)? Commit the report or only generate it?

## Triage notes

The template is kept in [docs/templates](../../templates/README.md) (`SoftwareBillOfMaterials.md.xslt`).

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.

2026-11-16: done. `scripts/docs/sbom.ps1` writes `artifacts/sbom/bom.xml` (CycloneDX 1.6) from `dotnet list package --include-transitive` (no new tool, so the CycloneDX and sbom-tool question is moot) and `artifacts/sbom/bom.md` through `TemplateEngine.Cli` and the XSLT. The timestamp is the last commit's date, so two runs on one commit are identical (checked). The output is generated, not committed (`artifacts/` is ignored). The package list carries id, version and direct or transitive scope; licences, authors and hashes need the NuGet metadata and are not included.
