# scripts/embeddings

`make-reference-vectors.py <output.json>` writes reference sentence embeddings from the original
`sentence-transformers/all-MiniLM-L6-v2` model (Hugging Face) for a fixed set of texts: plain English, accents,
CJK, Korean, Cyrillic, Arabic, emoji, whitespace, URLs, literal special tokens and inputs of 76 to 1482 tokens.

The output is checked in as
`src/ExternalServices/Onnx/OoBDev.Onnx.SentenceEmbeddings.Tests/TestData/all-minilm-l6-v2.reference.json` and used by
`ReferenceTests`, which require cosine similarity of at least 0.999 for every item.

```text
python -m venv .venv
.venv/Scripts/python -m pip install --index-url https://download.pytorch.org/whl/cpu torch
.venv/Scripts/python -m pip install sentence-transformers
.venv/Scripts/python scripts/embeddings/make-reference-vectors.py <output.json>
```
