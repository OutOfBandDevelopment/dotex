# Architecture § 2 — The Five Source Layers

[↑ Architecture](./README.md) · [← 1. The Shape in One Page](./01-shape-in-one-page.md) · [3. The Abstractions / Implementation Split →](./03-abstractions-implementation-split.md)

**Table 1 — Source layers and dependency rules**

| Layer | Path | Contains | May depend on | Notes |
|-------|------|----------|---------------|-------|
| **Framework** | `src/Framework` | Vendor-neutral capabilities (caching, message queueing, identity, search, documents, ASP.NET helpers, test utilities) | Own `*.Abstractions`, `OoBDev.System*`, `OoBDev.Extensions` | Where "the design" lives. |
| **Extensions** | `src/Extensions` | Optional, heavier or niche features (vector math + SQL CLR, HTML/Markdown/YAML text) | Framework | Packaged separately so consumers opt in. |
| **ExternalServices** | `src/ExternalServices` | Adapters that wrap one third-party product and implement a Framework abstraction | **Framework `*.Abstractions` only** (e.g. `OoBDev.Redis.Caching` → `OoBDev.Caching.Abstractions`) | Third-party SDK versions stay contained here. |
| **Common** | `src/Common` | Roll-up projects that register *everything* with one call | All of the above, via globs | Opposite of "pure interfaces" – see §4. |
| **Tools / Examples** | `src/Tools`, `src/Examples` | CLIs and a reference Web API | Common or Framework | Examples double as integration documentation. |

**The dependency rule that actually holds:** *adapters point at abstractions, abstractions point at nothing (except `Microsoft.Extensions.*.Abstractions`), and only the Common layer and applications see the whole graph.*

---

[↑ Architecture](./README.md) · [← 1. The Shape in One Page](./01-shape-in-one-page.md) · [3. The Abstractions / Implementation Split →](./03-abstractions-implementation-split.md)
