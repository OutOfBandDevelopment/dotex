# image-mask

Masks volatile zones in guide screenshots and compares images so a committed image changes only when the picture does.

## Usage

Without arguments the tool prints its commands and exits with code 2.

```console
$ image-mask
image-mask mask --input a.png --zones zones.json --output masked.png
image-mask diff --expected a.png --actual b.png [--zones zones.json] [--tolerance 0] [--max-pixels 0] [--diff-out diff.png]
image-mask sync --new new.png --committed guide.png [--zones zones.json] [--tolerance 0] [--max-pixels 0]
Zones file: { "zones": [ { "name": "clock", "match": "tui-*.png", "x": 60, "y": 0, "width": 80, "height": 16, "color": "#FF0000" } ] }
```

Exit code: `2`
