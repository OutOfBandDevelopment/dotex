# Project Structure and Build

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← DI and Composition](./01-di-and-composition.md) · [Cross-Cutting Runtime Concerns →](./03-cross-cutting-runtime.md)
<!-- nav -->

## 6. Abstractions / implementation split

**Today:** `X.Abstractions` plus `X` plus `X.Tests` ([pattern 1](../02-design-patterns/01-abstractions-implementation-registration.md)).

**Table 6 — Packaging of contracts**

| Option | Pros | Cons |
|--------|------|------|
| Current split | Adapters depend on contracts only; matches Microsoft's own `*.Abstractions` packages; small dependency graphs | Two to three projects per capability |
| Single project with public interfaces | Fewer projects | Adapters drag in the implementation and its dependencies |
| Shared contracts assembly for everything | One reference | Becomes a dumping ground; every change touches every consumer |

**Verdict: Keep.**

## 7. Glob-based Common aggregator

**Today:** roll-up projects include and remove projects by suffix through MSBuild globs ([architecture](../01-architecture/04-common-layer-aggregator.md)).

**Table 7 — Aggregation approaches**

| Option | Pros | Cons |
|--------|------|------|
| Current globs | New projects are picked up automatically; no list to maintain | Implicit: adding a project can change a package's dependencies; harder to review; restore graph is large |
| Explicit meta-package with listed references | Reviewable; deterministic | Manual upkeep |
| Solution filters (`.slnf`) per product | Fast local builds for subsets | Does not create packages |
| .NET Aspire style app host | Composition, service discovery and dashboards | A different problem (orchestration), not a package roll-up |

**Verdict: Consider.** Keep globs for the internal roll-up; publish an explicit list (generated) as a check in CI so accidental additions are visible.

## 8. Layering style

**Table 8 — Structural styles**

| Style | Pros | Cons |
|-------|------|------|
| Current: capability layers with provider adapters (close to ports and adapters) | Strong reuse across products; vendor isolation | Not optimized for a single product's use cases |
| Clean or Onion architecture per product | Familiar; domain at the centre | Heavy for libraries; suits applications more than frameworks |
| Vertical slices | Fast feature delivery, low ceremony | Weak reuse; needs discipline to avoid duplication |
| Modular monolith | Enforced module boundaries inside one deployable | Needs boundary tooling |

**Verdict: Keep** for the framework; **consider** vertical slices inside applications built on it.

## 9. Central package management

**Today:** `ManagePackageVersionsCentrally` is false and versions are inline in each project; an empty `Directory.Packages.props` exists.

**Table 9 — Version management**

| Option | Pros | Cons |
|--------|------|------|
| Current inline versions | Projects are self-contained | Drift across 120+ projects; upgrades touch many files |
| Central Package Management (`Directory.Packages.props`) | One place for versions; consistent upgrades; works with Dependabot or Renovate | One-time migration; per-project overrides need `VersionOverride` |
| Floating versions | Always latest | Non-reproducible builds |

**Verdict: Change.** Turn it on; a script can lift versions out of the project files deterministically.

## 10. Analyzers and warnings

**Today:** analyzer configuration exists but is commented out; XML documentation generation is commented out; the build had 8 warnings after cleanup.

**Table 10 — Static analysis**

| Option | Pros | Cons |
|--------|------|------|
| Current | No noise | Regressions go unnoticed |
| `AnalysisLevel` latest plus `EnforceCodeStyleInBuild` and `.editorconfig` | Built into the SDK; consistent style | Initial warning cleanup |
| `TreatWarningsAsErrors` in CI only | Keeps local flow fast | Needs a CI build configuration |
| Extra analyzers (Meziantou, Roslynator, SonarAnalyzer) | Deeper checks | More rules to tune |

**Verdict: Change.** Enable the SDK analyzers, generate XML docs for public APIs, and fail CI on warnings once the baseline is clean.

## 11. Versioning tool

**Table 11 — Version derivation**

| Option | Pros | Cons |
|--------|------|------|
| Current: GitVersion | Rich branching rules; widely known | Heavier; configuration complexity; needs full history |
| MinVer | Tiny; tag-based | Less branch-aware |
| Nerdbank.GitVersioning | Fast; `version.json` per project | Different model |
| Manual `Version` in props | Simplest | Easy to forget |

**Verdict: Keep.** It already fits the release workflows.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← DI and Composition](./01-di-and-composition.md) · [Cross-Cutting Runtime Concerns →](./03-cross-cutting-runtime.md)
<!-- nav -->
