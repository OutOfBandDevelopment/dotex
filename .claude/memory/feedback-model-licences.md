---
name: feedback-model-licences
description: Models should be ONNX/.NET-hosted, cached like Hugging Face, MIT-compatible; copyleft models deployed separately
metadata:
  type: feedback
---

Models that can be hosted as ONNX in .NET may live in the framework and use the Hugging Face cache and download pattern (`HuggingFaceHubCache`) to be reused. Prefer MIT-compatible licences; a model that needs AGPL or LGPL is maintained and deployed separately, outside the MIT packages. (owner, 2026-11-12)

**Why:** keeps the packages MIT-clean while still supporting the storage application.
**How to apply:** record each model's licence in its readme and manifest; never bundle non-commercial weights (for example InsightFace `buffalo_l`) or AGPL code (YOLOv10). See [[feedback-breaking-changes-ok]] for the unreleased-framework stance.
