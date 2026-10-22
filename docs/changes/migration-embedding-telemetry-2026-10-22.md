# Embedding generator telemetry

**Date:** 2026-10-22 · **Design:** [AllMiniLmL6V2](../design/AllMiniLmL6V2/README.md) (REQ-011)

## What changed

- `SentenceEmbeddingTelemetry` in `OoBDev.Onnx.SentenceEmbeddings` names one `ActivitySource` and `Meter`, `OoBDev.Onnx.SentenceEmbeddings`. Each inference batch records the span `embeddings.batch` (tags `embeddings.batch_size`, `embeddings.tokens`) and the metrics `oobdev.embeddings.duration` (ms), `oobdev.embeddings.batch_size` and `oobdev.embeddings.tokens`. Add the name to the OpenTelemetry sources and meters to export them.
- The design marks REQ-011 implemented and names the test.

## Verification

`TelemetryTests` (Integration, real model) observes the span and the three metrics. The whole `OoBDev.Onnx.SentenceEmbeddings.Tests` project passed twice, 61 of 61. The test looks for its own batch because the meter is static and shared by tests in the same run.

## Not done

Keyed generator registration through `EmbeddingGeneratorBuilder` and `AddValidatedOptions` stay in BACKLOG (the second is blocked by the Abstractions-only adapter rule).
