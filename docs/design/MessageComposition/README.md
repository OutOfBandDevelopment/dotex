# Message Composition

Orchestrates template rendering, data enhancement and document conversion to produce pre-formatted messages ready for delivery (Epic 12). It joins Epic 11 (Data Enhancement), Epic 10 (Text Templating) and Epic 6 (Document Conversion).

**Status:** design only for the full `MessageComposition` API (no such project exists in `src`). The narrow part that the communication orchestrator needed was built 2026-11-25 as `IMessageComposer` (per channel, in `OoBDev.Communications.Abstractions`, a different shape from the sketch below) with `TemplateEmailComposer` and `TemplateSmsComposer` ([Core design](../Communications/Core/README.md)); template selection, format conversion and attachments below are still planned.

**Table 1 — Documents**

| Document | Holds |
|----------|-------|
| [requirements.md](requirements.md) | scope and acceptance criteria |
| [architecture.md](architecture.md) | components and flow |
| [api-design.md](api-design.md) | public contracts |
| [testing-strategy.md](testing-strategy.md) | scenarios and coverage targets |

Related: [DataEnhancement](../DataEnhancement/README.md), [TextTemplating](../TextTemplating/README.md), [DocumentServices](../DocumentServices/README.md), [Communications](../Communications/README.md).
