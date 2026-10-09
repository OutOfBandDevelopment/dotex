"""Writes Pillow resize results used to test ImageResizer pixel for pixel.

Usage: python scripts/embeddings/make-resize-golden.py <output.json>

Each case has deterministic source pixels (a small linear congruential generator), the filter number
(Pillow Image.Resampling: 2 bilinear, 3 bicubic) and the Pillow result, both as base64 RGB bytes.
"""
import base64
import json
import sys

from PIL import Image

CASES = [
    # (source width, source height, target width, target height, filter)
    (37, 23, 16, 16, 3),
    (37, 23, 16, 16, 2),
    (23, 37, 16, 16, 3),
    (32, 32, 112, 112, 3),
    (32, 32, 112, 112, 2),
    (250, 150, 224, 134, 3),
    (150, 250, 134, 224, 3),
    (83, 129, 112, 174, 3),
    (333, 517, 100, 100, 2),
    (200, 60, 373, 112, 3),
    (1, 1, 224, 224, 3),
    (1, 1, 8, 8, 2),
    (50, 40, 50, 20, 3),
    (50, 40, 25, 40, 2),
    (10, 10, 10, 10, 3),
]


def pixels(width, height, seed):
    state = seed
    out = bytearray()
    for _ in range(width * height * 3):
        state = (state * 1103515245 + 12345) & 0x7FFFFFFF
        out.append((state >> 16) & 0xFF)
    return bytes(out)


def main():
    cases = []
    for index, (w, h, nw, nh, flt) in enumerate(CASES):
        source = pixels(w, h, 1000 + index)
        image = Image.frombytes("RGB", (w, h), source)
        result = image.resize((nw, nh), Image.Resampling(flt))
        cases.append({
            "width": w, "height": h, "newWidth": nw, "newHeight": nh, "filter": flt,
            "source": base64.b64encode(source).decode("ascii"),
            "expected": base64.b64encode(result.tobytes()).decode("ascii"),
        })
    with open(sys.argv[1], "w", encoding="utf-8") as handle:
        json.dump({"pillow": Image.__version__, "cases": cases}, handle)
    print("wrote", len(cases), "cases, Pillow", Image.__version__)


main()
