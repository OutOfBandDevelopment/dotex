import json
import sys
import numpy as np
import sentence_transformers
from sentence_transformers import SentenceTransformer

MODELS = ("all-minilm-l6-v2", "all-mpnet-base-v2", "nomic-embed-text-v1.5")
if len(sys.argv) != 3 or sys.argv[1] not in MODELS:
    sys.exit("usage: make-reference-vectors.py {" + "|".join(MODELS) + "} <output.json>")
key, out = sys.argv[1], sys.argv[2]
para = ("Sentence embeddings map text to dense vectors so that texts with similar meaning end up close together. "
        "They power semantic search, clustering, deduplication and retrieval augmented generation. ")
texts = [
    "The quick brown fox jumps over the lazy dog.",
    "A man is eating food.",
    "A man is eating a piece of bread.",
    "The girl is carrying a baby.",
    "Two men pushed carts through the woods.",
    "A cheetah is running behind its prey.",
    "How do I reset my password?",
    "What is the capital of France?",
    "Paris is the capital and most populous city of France.",
    "I love programming in C# and .NET 10!",
    "UPPER lower MiXeD 12345 3.14159 and e-mail test@example.com",
    "Café déjà vu naïve résumé coöperate Zürich Ångström",
    "punctuation!?;: (brackets) \"quotes\" - dashes... and 'apostrophes'",
    "a",
    "I",
    "The meeting was moved to 3:30 p.m. on March 5th, 2026.",
    "東京は日本の首都です",
    "Привет, как дела? Это тест.",
    "مرحبا بالعالم",
    "안녕하세요 세계",
    "emoji 🙂 and more 🚀🔥 in a sentence",
    "hello 🙂 world",
    "tabs\tand\nnewlines\r\nare whitespace",
    "   leading and trailing spaces   ",
    "Dr. Smith's co-worker didn't go to the U.S. office; they're working remotely.",
    "snake_case camelCase PascalCase kebab-case",
    "https://example.com/path?query=value&other=1#fragment",
    "The unbelievably uncharacteristically antidisestablishmentarian pneumonoultramicroscopicsilicovolcanoconiosis",
    "Zażółć gęślą jaźń",
    "This sentence contains the word [CLS] and [SEP] and [UNK] literally.",
    para * 2,
    para * 6,
    para * 12,
    para * 40,
]
if key == "all-mpnet-base-v2":
    texts.append("The literal markers <s> and </s> and <pad> and <mask> appear in this sentence.")

if key == "nomic-embed-text-v1.5":
    # The sentence-transformers wrapper needs trust_remote_code (third-party code). Run the official ONNX file with the
    # Hugging Face tokenizer instead and apply the model card steps: mean pooling, layer norm, truncate, normalise.
    import onnxruntime as ort
    from huggingface_hub import hf_hub_download
    from transformers import BertTokenizerFast

    repo = "nomic-ai/nomic-embed-text-v1.5"
    prefix = "search_document: "
    session = ort.InferenceSession(hf_hub_download(repo, "onnx/model.onnx"), providers=["CPUExecutionProvider"])
    print("inputs", [i.name for i in session.get_inputs()])
    tok = BertTokenizerFast.from_pretrained(repo)
    max_len = 512

    def embed(text):
        enc = tok(prefix + text, truncation=True, max_length=max_len, return_tensors="np")
        feeds = {i.name: enc[i.name].astype(np.int64) for i in session.get_inputs()}
        hidden = session.run(None, feeds)[0][0]
        pooled = hidden.mean(axis=0)
        normed = (pooled - pooled.mean()) / np.sqrt(pooled.var() + 1e-5)
        full = normed / np.linalg.norm(normed)
        cut = normed[:256]
        return full, cut / np.linalg.norm(cut), len(enc["input_ids"][0])

    items = []
    for t in texts:
        full, small, n = embed(t)
        items.append({"text": t, "tokens": n, "vector": [round(float(x), 7) for x in full], "vector256": [round(float(x), 7) for x in small]})
    data = {"source": repo, "prefix": prefix, "max_seq_length": max_len, "items": items}
else:
    repo = {"all-minilm-l6-v2": "sentence-transformers/all-MiniLM-L6-v2", "all-mpnet-base-v2": "sentence-transformers/all-mpnet-base-v2"}[key]
    model = SentenceTransformer(repo, device="cpu")
    vecs = model.encode(texts, normalize_embeddings=True, batch_size=8)
    tok = model.tokenizer
    data = {
        "source": repo,
        "sentence_transformers": sentence_transformers.__version__,
        "max_seq_length": model.max_seq_length,
        "items": [
            {
                "text": t,
                "tokens": len(tok(t, add_special_tokens=True)["input_ids"]),
                "vector": [round(float(x), 7) for x in v],
            }
            for t, v in zip(texts, vecs)
        ],
    }
with open(out, "w", encoding="utf-8") as f:
    json.dump(data, f, ensure_ascii=False)
print(len(texts), data["max_seq_length"])
