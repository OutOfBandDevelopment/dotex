import json
import sys
import sentence_transformers
from sentence_transformers import SentenceTransformer

out = sys.argv[1]
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
model = SentenceTransformer("sentence-transformers/all-MiniLM-L6-v2", device="cpu")
vecs = model.encode(texts, normalize_embeddings=True, batch_size=8)
tok = model.tokenizer
data = {
    "source": "sentence-transformers/all-MiniLM-L6-v2",
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
print(len(texts), model.max_seq_length, vecs.shape)
