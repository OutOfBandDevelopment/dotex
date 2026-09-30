# Pattern 14 — Strategy collections (engine → providers → sources)

[↑ Design Patterns](./README.md) · [← 13. Supervised hosted service](./13-supervised-hosted-service.md) · [15. Result envelope →](./15-result-envelope.md)

`TemplateEngine(IEnumerable<ITemplateSource>, IEnumerable<ITemplateProvider>)`: sources find templates (`FileTemplateSource` scans by `IEnumerable<IFileType>`), providers declare `SupportedContentTypes` + `CanApply(context)` and render (`XsltTemplateProvider`, Handlebars adapter). New formats are added by registering another implementation – no engine change.

Same shape: search (`ISearchProvider` lexical / semantic / hybrid in `OoBDev.Search`), documents (`IBlobContainerFactory`, conversion providers), AI (`IMessageCompletion`, `IEmbeddingProvider`, `IChatProvider` – Ollama, Groq, SBert each implement the same abstractions).

---

[↑ Design Patterns](./README.md) · [← 13. Supervised hosted service](./13-supervised-hosted-service.md) · [15. Result envelope →](./15-result-envelope.md)
