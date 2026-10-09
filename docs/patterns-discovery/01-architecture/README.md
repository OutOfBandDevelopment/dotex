# 01 — Architecture: How the Solution Is Put Together


> **Scope:** what the code *actually does today* (verified against `src/`, 2026-09-30), described so it can be repeated in new products and frameworks.
> Where this differs from `CLAUDE.md` or `docs/architecture/*`, the difference is called out in [README.md → Doc/Code Drift](../README.md#doccode-drift).

---

<!-- toc:start -->
## Contents

1. [Architecture § 1 — The Shape in One Page](./01-shape-in-one-page.md)
2. [Architecture § 2 — The Five Source Layers](./02-five-source-layers.md)
3. [Architecture § 3 — The Abstractions / Implementation Split](./03-abstractions-implementation-split.md)
4. [Architecture § 4 — The Common Layer Is an Aggregator (Convention over Reference)](./04-common-layer-aggregator.md)
5. [Architecture § 5 — Composition Root](./05-composition-root.md)
6. [Architecture § 6 — Runtime Composition Model (How a Call Finds Its Implementation)](./06-runtime-composition-model.md)
7. [Architecture § 7 — Cross-Cutting Building Blocks (all in `OoBDev.System*`)](./07-cross-cutting-building-blocks.md)
8. [Architecture § 8 — Hosting Model](./08-hosting-model.md)
9. [Architecture § 9 — Build & Packaging Architecture](./09-build-and-packaging.md)
10. [Architecture § 10 — Architectural Principles (Distilled from the Code)](./10-architectural-principles.md)

### List of Figures

1. [Figure 1 — System context (C4 level 1)](./01-shape-in-one-page.md)
2. [Figure 2 — Container view of the solution layers (C4 level 2)](./01-shape-in-one-page.md)

### List of Tables

1. [Table 1 — Source layers and dependency rules](./02-five-source-layers.md)
2. [Table 2 — Common roll-up projects](./04-common-layer-aggregator.md)
3. [Table 3 — Runtime composition mechanisms](./06-runtime-composition-model.md)
4. [Table 4 — Cross-cutting building blocks](./07-cross-cutting-building-blocks.md)
<!-- toc:end -->
---
