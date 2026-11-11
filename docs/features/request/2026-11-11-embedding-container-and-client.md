# Feature request: Embedding service container with a configurable client

- **Status:** new
- **Requested by:** owner (filed from the NAS storage catalog application, `C:\repo\_storage`)
- **Date:** 2026-11-11
- **Area:** `OoBDev.Onnx.ImageEmbeddings`, `Onnx.SentenceEmbeddings`, `Vision.*`, `Ollama`, `SBert`; a new client abstraction

## Need

The catalog runs image and text embedding in a Python GPU container (DINOv2, CLIP, sentence models) because the in-process ONNX path was not measured against it. Other applications should be able to use the same container, or a different one, without code changes.

## Proposed behavior

1. The container exposes a common embedding signature (recommended: the OpenAI-compatible `POST /v1/embeddings` for text, plus an image variant with the same response shape), so any compatible server can replace it.
2. dotex gets one configurable embedding client (base URL, model, dimensions, timeout, batch size) behind the existing embedding abstractions, with the in-process ONNX providers as another implementation of the same interface.
3. The container image and its compose file are published with the framework if no equivalent exists.

## Acceptance criteria

- Given the container and the client, when a batch of images is sent, then vectors of the configured dimension come back in request order.
- Given the same input through the ONNX provider and the container, when compared, then cosine similarity is at least 0.995 (the bar in `docs/design/ImageEmbeddings`).
- Given the client pointed at another OpenAI-compatible server, when text is embedded, then it works with configuration only.

## Existing code checked

- `ExternalServices/Onnx`, `Vision`, `Ollama`, `SBert` list embedding providers; whether one already offers a configurable HTTP client and what signature it uses was not read.

## Security and operations

Inputs are private files (photos, scans), so nothing is logged beyond identifiers and counts, and no source file is modified. Runs as a worker with a lease and a runtime ceiling (see `2026-11-11-database-stage-work-queue.md`).

## Priority

- **Impact:** the owner wants these features to live in the framework and be reintegrated into the catalog application (owner 2026-11-11).
- **Needed by:** no date
- **Workaround today:** the catalog application owns the implementation.

## Open questions

- Is `/v1/embeddings` the right common signature, or is the Hugging Face text-embeddings-inference shape more widely supported (recommended: OpenAI-compatible first)?
- Does the container ship from this repository or stay with the catalog application?

## Triage notes
