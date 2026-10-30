# DevTerm WPF stream monitor preview and export (2026-10-30)

The stream monitor window now has a split view: the capture list on the left, and for the selected capture its summary, where it was saved and a preview on the right. `StreamCapturePreview.Render` draws SVG captures with `SvgPreview` and the native image kinds (BMP, PNG, JPEG, GIF, TIFF) with WPF's decoders; anything else, or a truncated capture, shows the reason instead. "Export as..." writes a copy of the selected capture through the `IStreamExportDialog` seam (`WpfStreamExportDialog` is a save file dialog) and reports the outcome under the list.

- Tests (`StreamCapturePreviewTests`, 6): SVG drawn, broken SVG explained, PNG decoded, truncated PNG explained, HP-GL says no preview, an empty monitor shows the empty message and cannot export. The WPF test project has 102 Unit tests passing.
- Not done: the export write path has no test (a capture needs a live session feeding the monitor); the window was not viewed running. Still on the backlog for this window: search, kind filter, sort, per-device panes, Open Folder and Convert.
