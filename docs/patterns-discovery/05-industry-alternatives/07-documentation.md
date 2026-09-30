# Documentation

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Testing and Quality](./06-testing-and-quality.md) · [UI Patterns →](./08-ui.md)
<!-- nav -->

## 22. Documentation tooling

**Today:** hand-written markdown, per-project readmes, a design-first document set, PlantUML diagrams validated by script ([documentation practices](../03-practices-and-conventions/03-documentation-practices.md)).

**Table 22 — Documentation options**

| Option | Pros | Cons |
|--------|------|------|
| Current markdown in the repository | Versioned with code; simple; validated by `scripts/docs` | No searchable site; API reference not generated |
| DocFX | API reference from XML docs plus conceptual docs; static site | Needs XML docs enabled (currently off); build step |
| MkDocs or Docusaurus | Fast search, navigation | Separate toolchain |
| Architecture decision records (ADRs) | Captures why, with date and status | Needs a habit; complements, does not replace design docs |
| Diátaxis structure (tutorial, how-to, reference, explanation) | Clear document roles | Restructuring effort |
| C4 with Structurizr | Model-based diagrams | Extra tool; the plain PlantUML C4 style already covers the need |

**Verdict: Consider.** Add ADRs (`docs/decisions/NNNN-title.md`) as the place where the choices in this section are recorded once decided. Turn on XML documentation and add DocFX when the public API needs a browsable reference. Keep PlantUML and the validation scripts.

**Owner decision:** a documentation generation tool chain existed before; spikes should compare the options.

**Table 23 — Suggested next documentation steps**

| Step | Effort | Value |
|------|--------|-------|
| Add `docs/decisions/` with the first ADR using the [design document standard](../06-design-document-standard.md) header | Low | High |
| Enable XML docs and fail on missing public documentation | Medium | High |
| DocFX site published from CI | Medium | Medium |

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Testing and Quality](./06-testing-and-quality.md) · [UI Patterns →](./08-ui.md)
<!-- nav -->
