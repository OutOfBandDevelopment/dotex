# Feature request: software bill of materials report

- **Status:** new
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

`TemplateEngine.Cli` runs XSLT and Handlebars templates, so the report step exists. No CycloneDX tool is wired; the [framework wish list](2026-11-10-framework-wish-list.md) keeps the CycloneDX and Microsoft sbom-tool links.

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
