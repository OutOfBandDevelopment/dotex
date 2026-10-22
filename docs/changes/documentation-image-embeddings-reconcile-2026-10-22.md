# ImageEmbeddings design reconciled with the code

**Date:** 2026-10-22 · **Design:** [ImageEmbeddings](../design/ImageEmbeddings/README.md)

- The testing strategy listed `ConformanceTests` as no-model unit tests; the code marks them Integration. The document now says so.
- Added a requirement-coverage table that maps REQ-001 to REQ-010 to the existing tests. Every requirement has a test; no requirement lacks code.
- Found: the image generator has no `ActivitySource` or `Meter` (the sentence embedding generator now has them). Added to BACKLOG.

Verification: `OoBDev.Docs.Tests` on `docs/design/ImageEmbeddings` passed. No code or tests ran.
