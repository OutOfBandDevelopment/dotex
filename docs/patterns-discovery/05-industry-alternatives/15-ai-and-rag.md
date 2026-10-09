# AI and RAG Approaches

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Resilience Approaches](./14-resilience.md) · [Index →](./README.md)
<!-- nav -->

## 30. Model abstractions, vector stores and orchestration

**Today:** `ILanguageModelProvider` and `IEmbeddingProvider` contracts with Ollama, Groq, SBert and Qdrant adapters, Tika for extraction, and an optional Semantic Kernel layer ([AI practices](../03-practices-and-conventions/15-ai-vector-rag-practices.md)).

**Table 39 — Model abstraction options**

| Option | Pros | Cons |
|--------|------|------|
| Own contracts (current) | Small; no dependency; adapters stay thin | Must track new features (tools, structured output) by hand; one interface mixes concerns |
| `Microsoft.Extensions.AI` (`IChatClient`, `IEmbeddingGenerator`) | Platform-aligned abstractions with middleware for caching, logging and telemetry | Another abstraction to map to; check that the version is stable and its license and vendor coverage fit |
| Semantic Kernel | Plug-ins, planning, connectors | Larger surface; fast-moving; already used only for two plug-ins |
| Vendor SDKs directly | Full feature access | Locks applications to a vendor |

**Table 40 — Vector store options**

| Option | Pros | Cons |
|--------|------|------|
| Qdrant (current) | Purpose-built; gRPC; container for tests | Another service to run |
| SQL Server vector types (`OoBDev.Data.Vectors`, or native vector support if available in the target version) | One less service; joins with relational data | Scale and index features differ from a dedicated store |
| OpenSearch vector search (already in the test stack) | Combines keyword and vector search | Heavier; tuning needed |
| PostgreSQL with a vector extension, Redis | Reuse existing infrastructure | Feature gaps versus a dedicated store |

**Table 41 — RAG design choices**

| Choice | Options | Note |
|--------|---------|------|
| Chunking | Fixed size with overlap; by document structure | Structure-aware chunks give better citations |
| Retrieval | Vector only; hybrid keyword plus vector; re-ranking | Hybrid is a good default when OpenSearch is present |
| Model location | Local (Ollama) or hosted (Groq) | Sensitivity of the data decides |
| Evaluation | Fixed question sets with expected sources | Needed before changing models or chunking |

Package status and capabilities reflect general knowledge and must be re-checked before a decision.

**Verdict: Keep the own-contract and provider model; Consider `Microsoft.Extensions.AI` as an alternative surface once verified.** Split `ILanguageModelProvider` into chat, embedding and RAG contracts before adding providers, keep Qdrant behind `IVectorStore`, and add an evaluation set to the test suite. Semantic Kernel stays optional.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Resilience Approaches](./14-resilience.md) · [Index →](./README.md)
<!-- nav -->
