# Feature request: document packaging and document store

- **Status:** done (ZIP packager, document store, key generator, validation hook, resolver and directory packing built 2026-11-23, [change](../../changes/2026-11-23.md))
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.Documents*` (Epic 6 Documents)

## Need

dotex converts documents (`IDocumentConversion`, the chain builder and handlers for Tika, HtmlToOpenXml, ExternalTools and Hpgl) but cannot package several documents into one archive or store and retrieve documents by key with content validation.

## Proposed behavior

- Packaging: a packager, package handlers and a ZIP handler, selected by package type.
- Store: a document store with a key generator, a blob container resolver, request references, content and store results.
- Content validation hook (`IValidateContent`, with a bypass default) and typed exceptions for invalid input or output.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.DocumentCenter.Abstractions` (28 files)
- `OoBDev.DocumentCenter` (23 files: `DocumentPackager`, `ZipFileDocumentPackageHandler`, `DocumentStore`, `DocumentKeyGenerator`, `BlobContainerResolver`)
- `OoBDev.DocumentCenter.Tests`

## Acceptance criteria

- Given several documents, when packaged as ZIP, then the archive lists each and the originals are unchanged.
- Given a document and a key, when stored and read back, then the content is identical.
- Given content that fails validation, then the store rejects it with a typed exception.

## Existing code checked

Conversion, blob container and content type detection are covered (`OoBDev.Documents.Abstractions`, `OoBDev.Documents`). Searched `src` for `IDocumentPackager`, `IDocumentStore`, `IDocumentKeyGenerator` and `IValidateContent`: nothing. The design exists: [DocumentServices](../../design/DocumentServices/README.md) (`PackingService`, `UnpackingService`, `PersistenceService`, `RetrievalService`).

## Security and operations

Archives are untrusted: guard against zip slip and decompression bombs; bound sizes. Stored keys must not allow path traversal.

## Priority

- **Impact:** completes the document services epic.
- **Needed by:** no date (the epic waits for a consumer, Q1 of 2026-11-06)
- **Workaround today:** none

## Open questions

The design may already supersede the source types: follow the design, with the source as reference.

## Triage notes

Origin: SharedFramework. Design exists; no code until a consumer appears.

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.
