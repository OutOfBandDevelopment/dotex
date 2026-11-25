# SharedFramework duplication review (Epics 2, 6, 7, 10)

**Last Updated:** 2026-11-25
**Status:** Done 2026-11-25 at the owner's request: compare the SharedFramework features with what is implemented, expecting duplication; where it is not a duplicate, raise feature requests against the existing features. Compared by reading the code under `src/Framework` and `src/ExternalServices`, not the older status tables.

[SharedFramework](README.md) · [Migration plan](migration-plan.md)

## Contents

- [Verdicts](#verdicts)
- [Requests raised](#requests-raised)

## Verdicts

Duplicate means an existing project already does the job under another name, so the designed feature is closed by pointing at it. Gap means no code does it.

**Table 1 — Designed features against what exists**

| Epic | Designed feature | Existing code | Verdict |
|------|------------------|---------------|---------|
| 2 | Orchestration engine, preferences, quiet hours, deferral | `ICommunicationOrchestrator`, `TargetPreference`, `IDeferralStore` (in-memory and EF), `DeferralReleaseHost` | Duplicate (built 2026-11-25) |
| 2 | Template email and SMS composition | `TemplateEmailComposer`, `TemplateSmsComposer` over `ITemplateEngine` | Duplicate |
| 2 | Attribute-based data enhancement | `IDataEnhancer`, `[Communication]`, `TryAddCommunicationEnhancers` | Duplicate (the sketched `Priority` property and `IMessageData` are dropped: the request carries its priority and a dictionary) |
| 2 | Push and WhatsApp channels | `IChannelProvider` exists; no provider | Gap, speculative: no consumer, add a provider when one appears |
| 6 | Persistence and retrieval | `IBlobContainer` family, `ContentReference` | Duplicate |
| 6 | Conversion, media type detection | `IDocumentConversion`, Tika handlers, `IContentTypeDetector` | Duplicate |
| 6 | Packing and unpacking | `IDocumentPackager`, `ZipDocumentPackageHandler`, `DirectoryPackageItems`, `PackageSigner`, `PackageVerifier` | Duplicate (passwords were declared out of scope) |
| 6 | OCR | `IDocumentOcr`, `IDocumentLayoutOcr`, `DoclingOcr`, `SegmentedOcr` | Duplicate for one engine; engine choice and preprocessing are small gaps |
| 6 | Text extraction, metadata, tables | `OoBDev.Documents.Markdown` (and Office), the Tika client, EXIF, IPTC and audio tag readers | Duplicate for text, tables and metadata; embedded images and page selection are a gap |
| 6 | Splitting | `ChunkText` and `MarkdownPages` split text for retrieval | Duplicate for text; splitting a binary PDF by page, size or bookmark is a gap |
| 6 | Rendering (page to image, thumbnails, watermark) | none for documents | Gap |
| 6 | Composition (merge, table of contents, page numbers) | none | Gap |
| 7 | Account management | `IUserManagementProvider`, `IIdentityManager` | Partly built; the identity admin screen is already a BACKLOG item |
| 7 | Role and claims management, claims enhancement | OAuth 2.0 and JWT bearer, `[ApplicationRight]` with role-to-rights middleware (owner decisions) | Rejected: duplicates the owner's authorization model; claims enhancement declined 2026-11-19 (Q21, Q23) |
| 7 | Session management and token service | Keycloak and the STS issue and refresh tokens; `OoBDev.Identity.Jws` signs | Rejected: services understand only JWT (owner decision) |
| 7 | Modular profile management | none | Rejected for now: application data, not framework; no consumer |
| 10 | Handlebars provider, database template source | `OoBDev.Handlebars`, `EfTemplateSource` | Duplicate (built) |
| 10 | `ICompiledTemplate` | the Handlebars provider keeps a compilation cache | Duplicate |
| 10 | `IDataContainer` lazy data integration | `IDataEnhancer` fills the data before rendering | Duplicate in effect; lazy evaluation is not built and has no consumer |

## Requests raised

The real gaps are two document requests, filed under `docs/features/request/` and tracked in BACKLOG:

- [Document page operations](../../features/request/2026-11-25-document-page-operations.md): render pages and thumbnails, split and merge PDFs.
- [Document extraction extras](../../features/request/2026-11-25-document-extraction-extras.md): embedded images, page selection and OCR engine choice.

Push and WhatsApp channels stay a note here until a consumer exists.
