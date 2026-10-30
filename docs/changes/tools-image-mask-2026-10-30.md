# image-mask tool (2026-10-30)

## What changed

- New dotnet tool `OoBDev.ImageMask.Cli` (command `image-mask`, `src/Tools`) with `mask`, `diff` and `sync`, and `OoBDev.ImageMask.Cli.Tests` (11 Unit tests, passing). Both are in `src/OoBDev.slnx`.
- Zones come from a JSON file (bounding boxes with optional name, file-name wildcard and color); redactions are solid blocks painted before comparing.
- `sync` keeps the committed guide images unmasked (owner, 2026-10-30) and replaces one only when a new capture differs once masked.
- Tui screenshot tests (`TuiReview.DocImages`) write into `docs/applications/devterm/user-guide/images`.

## Not done

- The screenshot tests still overwrite the committed images directly; routing them through `sync` with a zones file per guide is in `BACKLOG.md`.

Design: [GeneratedGuides](../design/GeneratedGuides/README.md). Readme: `src/Tools/OoBDev.ImageMask.Cli/README.ImageMask.Cli.md`.
