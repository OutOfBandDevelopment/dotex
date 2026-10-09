# Pattern 14 — Strategy collections (engine → providers → sources)

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 13 — Supervised hosted service](./13-supervised-hosted-service.md) · [Pattern 15 — Result envelope →](./15-result-envelope.md)
<!-- nav -->

`TemplateEngine(IEnumerable<ITemplateSource>, IEnumerable<ITemplateProvider>)`: sources find templates (`FileTemplateSource` scans by `IEnumerable<IFileType>`), providers declare `SupportedContentTypes` + `CanApply(context)` and render (`XsltTemplateProvider`, Handlebars adapter). New formats are added by registering another implementation – no engine change.

Same shape: search (`ISearchProvider` lexical / semantic / hybrid in `OoBDev.Search`), documents (`IBlobContainerFactory`, conversion providers), AI (`IMessageCompletion`, `IEmbeddingProvider`, `IChatProvider` – Ollama, Groq, SBert each implement the same abstractions).

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 13 — Supervised hosted service](./13-supervised-hosted-service.md) · [Pattern 15 — Result envelope →](./15-result-envelope.md)
<!-- nav -->
