# OoBDev.Extensions.Windows.Forms

Windows Forms controls (`net10.0-windows`), migrated from BinaryDataDecoders `OoBDev.Windows.Forms`. A Framework library cannot reference Windows Forms, so the controls live here ([plan](../../../docs/design/WindowsForms/migration-plan.md), owner decision Q9, 2026-11-06).

| Type | Purpose |
|------|---------|
| `ValidTextBox` | A `TextBox` whose background shows whether the text matches the regular expression `Filter` (no filter: always valid). `ValidColor` (light green) and `InvalidColor` (light pink) are designer properties, `IsValid` reads the state, `Validate()` reapplies it, and `ValidateChanged` fires only when the state flips. The match has a one-second timeout. |

`ValidationEventArgs` (`IsValid`, `Value`) is host-free and stays in `OoBDev.System` (namespace `OoBDev.System.Windows.Forms`).

## Verified

5 Unit tests in `OoBDev.Extensions.Windows.Forms.Tests` create the control without a handle or window (no filter, mismatch, match, change events, filter change). The control has not been looked at in a designer or a running form.
