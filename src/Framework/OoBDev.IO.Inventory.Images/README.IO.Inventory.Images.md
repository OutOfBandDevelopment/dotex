# OoBDev.IO.Inventory.Images

Phase two of the [file inventory](../../../docs/design/FileInventory/README.md): near-duplicate images.

- `ImageHasher.DHash(stream)` decodes with SkiaSharp and uses the framework dHash (`OoBDev.Imaging.Analysis.Bursts.DHash`, Pillow-compatible), so inventory and burst-ranking hashes are the same; `Distance` counts differing bits.
- `NearDuplicateFinder.Scan(root, maxDistance, ct)` hashes png, jpg, gif, bmp and webp files under a folder, skips unreadable ones (returned with the reason) and groups images within the distance (single linkage; about 5 for resized copies). `Group` does the grouping on precomputed hashes.
- Read-only: nothing is moved or deleted. Embedding-based matching stays with [image embeddings](../../../docs/design/ImageEmbeddings/README.md).
