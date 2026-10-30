# DevTerm WPF SVG preview (2026-10-30)

`SvgPreview.TryRender(svg, out error)` in `OoBDev.DevTerm.Wpf` draws the small SVG subset the stream monitor's captures use (path, line, polyline, polygon, rect, circle, ellipse with stroke, fill and stroke width) into a frozen `DrawingImage` on a white background, scaled by the `viewBox` (or `width`/`height`), with a minimum stroke of a 250th of the larger side so hairlines stay visible. Anything else (text, `use`, gradients) is skipped, and a malformed or non-SVG document returns null with the reason. Ported from dev-term; WPF has no SVG decoder and no dependency was added.

- Tests (`SvgPreviewTests`, 6): viewBox sizing, width/height fallback, shapes drawn, non-SVG root, bad XML, hairline minimum. The WPF test project has 96 Unit tests passing.
- Not done: the stream monitor window does not use it yet (capture preview and Export As stay on the backlog), and nothing was viewed running.
