# Keyed embedding generator (2026-10-29)

`TryAddAllMiniLmL6V2Services` now also registers `IEmbeddingGenerator<string, Embedding<float>>` under `SBertGlobals.AllMiniLmL6V2Key`, forwarding to the shared singleton, so callers can pick it next to other models. Unit test `Generator_IsRegisteredDefaultAndKeyed` passes. `EmbeddingGeneratorBuilder` wrapping is not done on purpose: the generator already emits its own span and metrics (REQ-011). Design: [api-design](../design/AllMiniLmL6V2/api-design.md).
