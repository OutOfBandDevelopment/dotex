# ImageEmbeddings — Testing strategy

[← API design](api-design.md) · [Overview](README.md)

## Comparison with Python

`scripts/embeddings/make-reference-images.py <model> <output.json>` writes vectors (and top-5 labels for classifiers) from the original models: `transformers` image processors with `CLIPModel`, `AutoModel` (DINOv2, CLS output) and `ViTForImageClassification`. The image set is generated deterministically (gradients, noise with a fixed seed, shapes, text) plus a few small public-domain photographs checked in under `TestData/`, in PNG and JPEG, with odd sizes, a grayscale image and an alpha image.

Pass bar: cosine 0.999 or better for every image; classifiers must agree on the top-1 label and the top-5 set, probabilities within 0.01. Comparison tests download the models, so they are `Integration` category and report `Inconclusive` when offline.

## Unit tests (no model)

- Resize against fixed expected pixels from PIL for bicubic and bilinear, shortest-edge and direct modes, up- and down-scaling.
- Center crop, normalisation and NCHW layout.
- Options validation; `MaxImageBytes` and `MaxPixels` rejection; decoder abstraction with a fake decoder.

## Conformance tests (model, Integration)

Same checks for every preset: unit length, determinism, batch against single, order, concurrency, corrupt and empty bytes, grayscale, alpha, very large and 1 by 1 images, and similarity ordering (a picture is closer to a re-encoded copy than to an unrelated picture). CLIP: image-to-text ordering and zero-shot labels.

## Coverage

80 percent for the runner project from unit tests alone; presets are covered by the Integration suite.

[Overview](README.md)
