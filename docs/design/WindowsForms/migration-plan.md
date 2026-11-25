# Windows Forms extensions: migration plan

Status: done. Option confirmed by the owner on 2026-11-06 (Q9, option A); all four steps are done (the incoming copy was removed 2026-11-10).

Why: the incoming `OoBDev.Windows.Forms` holds `ValidTextBox` (a `TextBox` whose background shows whether the text matches a regex `Filter`) and `ValidationEventArgs`. Main only has them commented out in `OoBDev.System/Windows/Forms` ("TODO: Port this out"), and a Framework library cannot reference Windows Forms (rule `layering-and-migration-plans`).

**Table 1 — Projects**

| Project | Current location | Target location | Namespace | Package or deployment unit | References that change |
|---------|------------------|-----------------|-----------|----------------------------|------------------------|
| `ValidTextBox` | the former `OoBDev.Windows.Forms` project (removed; see `git log`) (and commented out in `src/Framework/OoBDev.System/Windows/Forms`) | new `src/Extensions/OoBDev.Extensions.Windows.Forms` (`net10.0-windows`, `UseWindowsForms`) and `.Tests` | `OoBDev.Extensions.Windows.Forms` | package `OoBDev.Extensions.Windows.Forms` (already named in `CLAUDE.md`) | new project; add both to `OoBDev.slnx`; delete the commented code in `OoBDev.System` |
| `ValidationEventArgs` | same | stays in Framework (`OoBDev.System`), host-free | `OoBDev.Windows.Forms` as in the source, unless the namespace is renamed to the Framework convention at implementation | none | the Extensions project references it |

Order: (1) project and readme, (2) port both types, (3) tests (the regex-to-color logic is testable without showing a form; a `Control` can be created off-screen on an STA thread), (4) remove the commented code and the incoming copy per D1.
