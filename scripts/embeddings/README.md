# scripts/embeddings

`make-reference-vectors.py <model> <output.json>` (model: `all-minilm-l6-v2`, `all-mpnet-base-v2` or `nomic-embed-text-v1.5`) writes reference sentence embeddings from the original
Hugging Face model (Hugging Face) for a fixed set of texts: plain English, accents,
CJK, Korean, Cyrillic, Arabic, emoji, whitespace, URLs, literal special tokens and inputs of 76 to 1482 tokens.

The output is checked in as
`src/ExternalServices/Onnx/OoBDev.Onnx.SentenceEmbeddings.Tests/TestData/all-minilm-l6-v2.reference.json` and used by
`ReferenceTests`, which require cosine similarity of at least 0.999 for every item.

```text
python -m venv .venv
.venv/Scripts/python -m pip install --index-url https://download.pytorch.org/whl/cpu torch
.venv/Scripts/python -m pip install sentence-transformers
.venv/Scripts/python -m pip install onnxruntime   # only for nomic-embed-text-v1.5
.venv/Scripts/python scripts/embeddings/make-reference-vectors.py all-minilm-l6-v2 <output.json>
```

Nomic is run through its official ONNX file with the Hugging Face tokenizer (the sentence-transformers wrapper needs `trust_remote_code`); the other two use `sentence-transformers`. `PresetReferenceTests` checks the MPNet and Nomic presets against `all-mpnet-base-v2.reference.json` and `nomic-embed-text-v1.5.reference.json`.
