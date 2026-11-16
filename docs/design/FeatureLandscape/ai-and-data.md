# Landscape: AI and data

Part of the [feature landscape](README.md). Status is "a project exists under `src`", not "verified".

## Contents

1. [Summary](#summary)
2. [Embeddings: AllMiniLmL6V2 and ImageEmbeddings](#embeddings-allminilml6v2-and-imageembeddings)
3. [VectorSearch and HybridSearch](#vectorsearch-and-hybridsearch)
4. [LlmCodeGen and ContractParser](#llmcodegen-and-contractparser)
5. [DesiredStateDatabase](#desiredstatedatabase)
6. [GeneratedGuides](#generatedguides)

**List of tables**

1. [Table 1 — AI and data features](#table-1)

## Summary

**Table 1 — AI and data features**

| Feature | Code | Standards | Main gap |
|---------|------|-----------|----------|
| AllMiniLmL6V2 | in-process ONNX embedder and tokenizer | ONNX | none recorded |
| ImageEmbeddings | DINOv2, ViT and CLIP runners | ONNX | model licence notes per preset |
| VectorSearch | `OoBDev.Data.Vectors` plus SqlStore, DB, Hosting | SQL Server vector types | other stores (Qdrant, OpenSearch) only through adapters |
| HybridSearch | `OoBDev.Search` plus Abstractions; diagrams fixed 2026-11-11 | reciprocal rank fusion | evaluation set missing |
| LlmCodeGen | provider clients (Ollama, Groq) | OpenAI-compatible chat API | prompt versioning |
| ContractParser | none confirmed | none | design only |
| DesiredStateDatabase | `OoBDev.DacFx`, `OoBDev.DacPacCompiler.Cli`, `Extensions.CodeAnalysis.DacFx` | DACPAC | drift detection scope |
| GeneratedGuides | `OoBDev.Documents.Guides` | none | journey coverage per front end |

## Embeddings: AllMiniLmL6V2 and ImageEmbeddings

- **Works with:** VectorSearch (stores the vectors), HybridSearch (query and document embeddings), LlmCodeGen (retrieval context).
- **Known issues:** model files come from a shared Hugging Face cache and are not in the repository; tests for them are Integration or DevLocal.
- **Practices:** one embedder interface, presets by key, NULL-safe vectors.
- **Built 2026-11-16:** `EmbeddingVersion` (OoBDev.Search) stamps the model tag into vector metadata and finds stale rows after a model change; the example vector store controller stamps on ingest; `IVectorStore.DeleteAsync` and `EmbeddingVersion.ReembedAsync` replace stale rows (store the new tagged row, then delete the old); a command or screen that calls it is still to build.

## VectorSearch and HybridSearch

- **Works with:** DocumentServices (text in), Embeddings (vectors), OpenSearch and Qdrant (test stack).
- **Known issues:** diagrams crashed the PlantUML server until `fork` was replaced by `split` (fixed 2026-11-11); ranking quality is not measured.
- **Standards:** none formal; BM25 plus vector fusion (reciprocal rank fusion) is the usual pattern.
- **Built 2026-11-16:** `RelevanceMetrics` (recall at k, reciprocal rank, mean reciprocal rank) in OoBDev.Search to score a query set; the query set itself is per project.
- **Missing:** access filters tied to ApplicationRights so search never leaks documents.

## LlmCodeGen and ContractParser

- **Works with:** Embeddings for retrieval; ContractParser would feed it contract text.
- **Known issues:** ContractParser is a harvest project with no migration decision yet; live LLM tests are manual (LiveIntegration).
- **Practices:** provider/factory per LLM service, local Ollama for CI.
- **Built 2026-11-16:** prompt-injection guidance in the LlmCodeGen requirements ("Untrusted input and output").
- **Missing:** token and cost accounting.

## DesiredStateDatabase

- **Works with:** MasterData, VectorSearch (schemas) and the SQL Server test container.
- **Known issues:** schema compare needs a SQL Server instance, so tests are Integration.
- **Decided 2026-11-16:** the data-loss guard is requirement REQ-003 of the DesiredStateDatabase design (unsafe changes refused without an explicit flag; hazard and `ApplyAsync` test in its testing strategy), built with the feature after the bake-off, not separately.
- **Missing:** a rollback story.

## GeneratedGuides

- **Works with:** every UI feature (journey tests produce screenshots) and OpenApiScalar/AsyncApi (API guides).
- **Known issues:** screenshots need the interactive desktop for WPF, so generation is a manual step.
- **Missing:** a freshness check in the docs validator that compares screenshot dates with the XAML they show.
