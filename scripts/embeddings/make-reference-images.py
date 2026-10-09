"""Writes reference image embeddings and labels from the original Hugging Face models.

Usage: make-reference-images.py {clip-vit-base-patch32|dinov2-small|vit-base-patch16-224} <image-folder> <output.json>

The image folder is filled with a deterministic synthetic set on first use (PNG and JPEG, odd sizes, grayscale,
palette and alpha images) so the .NET tests decode exactly the same files. Run it with the Python from a virtual
environment that has torch, transformers and pillow, and use `python -I` so nothing is imported from the working folder.
"""
import json
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

MODELS = {
    "clip-vit-base-patch32": "openai/clip-vit-base-patch32",
    "dinov2-small": "facebook/dinov2-small",
    "vit-base-patch16-224": "google/vit-base-patch16-224",
}

CLIP_TEXTS = [
    "a photo of a cat",
    "a photo of a dog",
    "a red square",
    "a blue circle on a white background",
    "a gradient",
    "random noise",
    "text on a page",
    "a landscape",
]


def scene(width, height, seed):
    rng = np.random.default_rng(seed)
    x = np.linspace(0, 1, width)[None, :, None]
    y = np.linspace(0, 1, height)[:, None, None]
    base = np.concatenate(
        [
            x * rng.uniform(0.3, 1.0) + 0 * y,
            y * rng.uniform(0.3, 1.0) + 0 * x,
            (x + y) / 2 * rng.uniform(0.3, 1.0),
        ],
        axis=2,
    )
    pixels = (base * 255).astype(np.uint8)
    image = Image.fromarray(pixels, "RGB")
    draw = ImageDraw.Draw(image)
    for _ in range(6):
        x0, y0 = int(rng.integers(0, max(width - 2, 1))), int(rng.integers(0, max(height - 2, 1)))
        x1, y1 = x0 + int(rng.integers(1, max(width // 2, 2))), y0 + int(rng.integers(1, max(height // 2, 2)))
        colour = tuple(int(c) for c in rng.integers(0, 256, 3))
        if rng.random() < 0.5:
            draw.rectangle([x0, y0, x1, y1], fill=colour)
        else:
            draw.ellipse([x0, y0, x1, y1], fill=colour, outline=(255, 255, 255))
    draw.text((width // 10, height // 2), "OoBDev image test", fill=(255, 255, 255))
    return image


def noise(width, height, seed):
    rng = np.random.default_rng(seed)
    return Image.fromarray(rng.integers(0, 256, (height, width, 3), dtype=np.uint8), "RGB")


def make_images(folder):
    os.makedirs(folder, exist_ok=True)
    specs = [
        ("scene-224.png", lambda: scene(224, 224, 1)),
        ("scene-640x480.png", lambda: scene(640, 480, 2)),
        ("scene-480x640.png", lambda: scene(480, 640, 3)),
        ("wide-1000x300.png", lambda: scene(1000, 300, 4)),
        ("tall-300x1000.png", lambda: scene(300, 1000, 5)),
        ("small-64.png", lambda: scene(64, 64, 6)),
        ("tiny-1x1.png", lambda: Image.new("RGB", (1, 1), (200, 30, 90))),
        ("odd-333x517.png", lambda: scene(333, 517, 7)),
        ("noise-256.png", lambda: noise(256, 256, 8)),
        ("noise-800x600.png", lambda: noise(800, 600, 9)),
        ("gray-300.png", lambda: scene(300, 300, 10).convert("L")),
        ("palette-200.png", lambda: scene(200, 200, 11).convert("P", palette=Image.Palette.ADAPTIVE, colors=32)),
        ("alpha-256.png", lambda: scene(256, 256, 12).convert("RGBA")),
        ("red-square.png", lambda: Image.new("RGB", (300, 300), (255, 0, 0))),
        ("white-100.png", lambda: Image.new("RGB", (100, 100), (255, 255, 255))),
    ]
    for name, build in specs:
        path = os.path.join(folder, name)
        if os.path.exists(path):
            continue
        image = build()
        if name == "alpha-256.png":
            alpha = Image.linear_gradient("L").resize(image.size)
            image.putalpha(alpha)
        image.save(path)
    for name, quality, source in [("scene-640x480-q85.jpg", 85, "scene-640x480.png"), ("scene-333x517-q40.jpg", 40, "odd-333x517.png"), ("noise-256-q90.jpg", 90, "noise-256.png")]:
        path = os.path.join(folder, name)
        if not os.path.exists(path):
            Image.open(os.path.join(folder, source)).convert("RGB").save(path, quality=quality)
    return sorted(f for f in os.listdir(folder) if f.lower().endswith((".png", ".jpg")))


def unit(vector):
    vector = np.asarray(vector, dtype=np.float64)
    return vector / np.linalg.norm(vector)


def main():
    if len(sys.argv) != 4 or sys.argv[1] not in MODELS:
        sys.exit(__doc__)
    key, folder, output = sys.argv[1], sys.argv[2], sys.argv[3]
    files = make_images(folder)
    images = [Image.open(os.path.join(folder, f)) for f in files]

    import torch
    from transformers import AutoImageProcessor

    repo = MODELS[key]
    processor = AutoImageProcessor.from_pretrained(repo)
    result = {"model": key, "source": repo, "items": []}

    with torch.no_grad():
        if key == "clip-vit-base-patch32":
            from transformers import CLIPModel, CLIPTokenizer

            model = CLIPModel.from_pretrained(repo).eval()
            tokenizer = CLIPTokenizer.from_pretrained(repo)
            text_inputs = tokenizer(CLIP_TEXTS, padding=True, return_tensors="pt")
            text_vectors = model.get_text_features(**text_inputs)
            text_vectors = text_vectors.pooler_output if hasattr(text_vectors, "pooler_output") else text_vectors
            result["texts"] = [{"text": t, "vector": unit(v.numpy()).tolist()} for t, v in zip(CLIP_TEXTS, text_vectors)]
            for name, image in zip(files, images):
                inputs = processor(images=image.convert("RGB"), return_tensors="pt")
                vector = model.get_image_features(pixel_values=inputs["pixel_values"])
                vector = vector.pooler_output if hasattr(vector, "pooler_output") else vector
                logits = model.logit_scale.exp() * unit(vector[0].numpy()) @ np.stack([unit(v.numpy()) for v in text_vectors]).T
                probabilities = torch.softmax(torch.tensor(logits, dtype=torch.float32), dim=0).tolist()
                result["items"].append({"file": name, "vector": unit(vector[0].numpy()).tolist(), "zeroShot": probabilities})
        elif key == "dinov2-small":
            from transformers import AutoModel

            model = AutoModel.from_pretrained(repo).eval()
            for name, image in zip(files, images):
                inputs = processor(images=image.convert("RGB"), return_tensors="pt")
                hidden = model(**inputs).last_hidden_state
                result["items"].append({"file": name, "vector": unit(hidden[0, 0].numpy()).tolist()})
        else:
            from transformers import AutoModelForImageClassification

            model = AutoModelForImageClassification.from_pretrained(repo).eval()
            result["labels"] = [model.config.id2label[i] for i in range(len(model.config.id2label))]
            for name, image in zip(files, images):
                inputs = processor(images=image.convert("RGB"), return_tensors="pt")
                logits = model(**inputs).logits[0]
                probabilities = torch.softmax(logits, dim=0)
                top = torch.topk(probabilities, 5)
                result["items"].append(
                    {
                        "file": name,
                        "top": [{"label": model.config.id2label[int(i)], "index": int(i), "probability": float(p)} for p, i in zip(top.values, top.indices)],
                        "logits": [float(v) for v in logits],
                    }
                )

    with open(output, "w", encoding="utf-8") as handle:
        json.dump(result, handle)
    print(f"{key}: {len(result['items'])} items -> {output}")


if __name__ == "__main__":
    main()
