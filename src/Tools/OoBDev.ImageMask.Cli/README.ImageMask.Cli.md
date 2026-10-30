# image-mask

## Summary

A dotnet tool (`image-mask`) that paints solid redaction blocks over listed bounding boxes, compares two images with those zones masked, and keeps committed guide images unmasked while ignoring volatile regions (clocks, temp paths, ports). Built on SkiaSharp. See [GeneratedGuides](../../../docs/design/GeneratedGuides/README.md).

## Zones file

```json
{
  "zones": [
    { "name": "clock", "match": "tui-*.png", "x": 60, "y": 0, "width": 80, "height": 16, "color": "#FF0000" }
  ]
}
```

`match` is a file-name wildcard (default `*`), `color` is `#RRGGBB` or `#AARRGGBB` (default red). Comments and trailing commas are allowed.

## Commands

| Command | Does | Exit code |
|---------|------|-----------|
| `image-mask mask --input a.png --zones zones.json --output masked.png` | Writes a redacted copy | 0 |
| `image-mask diff --expected a.png --actual b.png [--zones zones.json] [--tolerance 0] [--max-pixels 0] [--diff-out diff.png]` | Masks both, counts pixels differing by more than the per-channel tolerance | 0 equal, 1 different, 2 error |
| `image-mask sync --new new.png --committed guide.png [--zones zones.json] [--tolerance 0] [--max-pixels 0]` | Copies the unmasked new capture over the committed image only when they differ once masked | 0, 2 error |

Install from the repository: `dotnet pack` the project and `dotnet tool install`. Tests: `OoBDev.ImageMask.Cli.Tests` (Unit).
