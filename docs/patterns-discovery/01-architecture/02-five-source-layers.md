# Architecture § 2 — The Five Source Layers

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 1 — The Shape in One Page](./01-shape-in-one-page.md) · [Architecture § 3 — The Abstractions / Implementation Split →](./03-abstractions-implementation-split.md)
<!-- nav -->

**Table 1 — Source layers and dependency rules**

| Layer | Path | Contains | May depend on | Notes |
|-------|------|----------|---------------|-------|
| **Framework** | `src/Framework` | Vendor-neutral capabilities (caching, message queueing, identity, search, documents, ASP.NET helpers, test utilities) | Own `*.Abstractions`, `OoBDev.System*`, `OoBDev.Extensions` | Where "the design" lives. |
| **Extensions** | `src/Extensions` | Optional, heavier or niche features (vector math + SQL CLR, HTML/Markdown/YAML text) | Framework | Packaged separately so consumers opt in. |
| **ExternalServices** | `src/ExternalServices` | Adapters that wrap one third-party product and implement a Framework abstraction | **Framework `*.Abstractions` only** (e.g. `OoBDev.Redis.Caching` → `OoBDev.Caching.Abstractions`) | Third-party SDK versions stay contained here. |
| **Common** | `src/Common` | Roll-up projects that register *everything* with one call | All of the above, via globs | Opposite of "pure interfaces" – see §4. |
| **Applications** | `src/Applications` | Product applications and the shared logic their front ends use (for example `OoBDev.DevTerm.Core` with the Wpf, Tui and Web shells) | Framework, Extensions, ExternalServices | Highest layer; nothing below may reference it. Application-specific names and behaviour live here, reusable capabilities do not. |
| **Tools / Examples** | `src/Tools`, `src/Examples` | CLIs and a reference Web API | Common or Framework | Examples double as integration documentation. |

**The dependency rule that actually holds:** *adapters point at abstractions, abstractions point at nothing (except `Microsoft.Extensions.*.Abstractions`), and only the Common layer and applications see the whole graph.*


## Placement rules (owner, 2026-10-29)

1. **Reusable means Framework.** A capability with no application-specific names (transports, telemetry, accessors, pipelines) is a Framework library with a generic name, even if one application is the first user. Application logic stays in `src/Applications`, as one shared core library for the application's front ends rather than many small libraries.
2. **Host-free by default.** A Framework library does not reference a host (`Microsoft.AspNetCore.App`, WPF, Windows Forms) unless it exists to serve that host. Host-specific adapters sit in the host helper (for example `OoBDev.AspNetCore.Mvc`), so CLI and desktop applications can use the generic library. Check a project's references before choosing it as a home.
3. **Context through accessors, not host types.** Code that needs the user or the correlation context reads an abstraction (`ICurrentUserAccessor` with its `ClaimsPrincipal`, `IAccessor<CorrelationInfo>`). The web host wraps `IHttpContextAccessor`; a CLI or WPF application uses the Windows identity or its own sign-in.
4. **Transports are device-independent.** Device transports live in `OoBDev.System.IO.*` behind a pipe-based `IDeviceAdapter` (zero-copy where possible) and never reference decoders or an application.
5. **Abstractions later.** An application extracts an abstractions library (for example for its plugin model) only when extension needs it.
6. **Ask early, plan first.** Layering and composition questions go to the owner before work starts, with a migration plan (projects, deployment units, namespaces, changed references) and a spike when the shape is unclear (rule `layering-and-migration-plans`).

---

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 1 — The Shape in One Page](./01-shape-in-one-page.md) · [Architecture § 3 — The Abstractions / Implementation Split →](./03-abstractions-implementation-split.md)
<!-- nav -->
