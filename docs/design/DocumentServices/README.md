# Document Services

Design documents for the document-processing services of the SharedFramework (Epic 6). Each service has four documents: requirements, architecture, API design and testing strategy.

## Contents

- [Services](#services)
- [Implementation status](#implementation-status)

## Services

**Table 1 — Document services and their documents**

| Service | Requirements | Architecture | API design | Testing strategy |
|---------|--------------|--------------|------------|------------------|
| Composition | [requirements](CompositionService/requirements.md) | [architecture](CompositionService/architecture.md) | [api-design](CompositionService/api-design.md) | [testing-strategy](CompositionService/testing-strategy.md) |
| Conversion | [requirements](ConversionService/requirements.md) | [architecture](ConversionService/architecture.md) | [api-design](ConversionService/api-design.md) | [testing-strategy](ConversionService/testing-strategy.md) |
| Extraction | [requirements](ExtractionService/requirements.md) | [architecture](ExtractionService/architecture.md) | [api-design](ExtractionService/api-design.md) | [testing-strategy](ExtractionService/testing-strategy.md) |
| Media type detection | [requirements](MediaTypeDetection/requirements.md) | [architecture](MediaTypeDetection/architecture.md) | [api-design](MediaTypeDetection/api-design.md) | [testing-strategy](MediaTypeDetection/testing-strategy.md) |
| OCR | [requirements](OcrService/requirements.md) | [architecture](OcrService/architecture.md) | [api-design](OcrService/api-design.md) | [testing-strategy](OcrService/testing-strategy.md) |
| Packing | [requirements](PackingService/requirements.md) | [architecture](PackingService/architecture.md) | [api-design](PackingService/api-design.md) | [testing-strategy](PackingService/testing-strategy.md) |
| Persistence | [requirements](PersistenceService/requirements.md) | [architecture](PersistenceService/architecture.md) | [api-design](PersistenceService/api-design.md) | [testing-strategy](PersistenceService/testing-strategy.md) |
| Rendering | [requirements](RenderingService/requirements.md) | [architecture](RenderingService/architecture.md) | [api-design](RenderingService/api-design.md) | [testing-strategy](RenderingService/testing-strategy.md) |
| Retrieval | [requirements](RetrievalService/requirements.md) | [architecture](RetrievalService/architecture.md) | [api-design](RetrievalService/api-design.md) | [testing-strategy](RetrievalService/testing-strategy.md) |
| Splitting | [requirements](SplittingService/requirements.md) | [architecture](SplittingService/architecture.md) | [api-design](SplittingService/api-design.md) | [testing-strategy](SplittingService/testing-strategy.md) |
| Unpacking | [requirements](UnpackingService/requirements.md) | [architecture](UnpackingService/architecture.md) | [api-design](UnpackingService/api-design.md) | [testing-strategy](UnpackingService/testing-strategy.md) |

## Implementation status

Reconciled with `src/Framework/OoBDev.Documents`, `.Abstractions`, `.ExternalTools`, `.Hpgl` and `.Guides` on 2026-11-06. The designs are proposals; the code is the current truth where they differ.

**Table 2 — Designed services against the code**

| Service | Code | State |
|---------|------|-------|
| Conversion | `IDocumentConversion`, `IDocumentConversionHandler`, `DocumentConversionChainBuilder` (chained steps), `ExternalToolConversionHandler`, `HpglToSvgConversionHandler` | built under other names than `IDocumentConversionService` and `IDocumentConversionProvider` |
| Media type detection | `IContentTypeDetector`, `IDocumentTypeTools`, `DocumentTypeTools` | built under other names than `IMediaTypeDetectionService` |
| Persistence and retrieval | `IBlobContainer`, `IBlobContainerProvider`, `IBlobContainerProviderFactory`, `IBlobContainerFactory`, `BlobContainerAttribute`, `ContentReference` and `ContentMetaDataReference` | partly built: one blob container abstraction covers both the persistence and retrieval services |
| Extraction, OCR, rendering, splitting, packing, unpacking, composition | none | not built |

`OoBDev.Documents.Guides` (API and CLI guide rendering) is not in the designs; it implements [GeneratedGuides](../GeneratedGuides/README.md).
