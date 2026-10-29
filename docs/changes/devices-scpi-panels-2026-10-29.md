# SCPI generated control panel (2026-10-29)

New `OoBDev.Scpi.Panels` (Extensions) with `ScpiPanelBuilder`, `ScpiPanel`, `ScpiPanelCommand` and `ScpiPanelIds`. It turns a `ScpiInstrumentProfile` into a `UiDefinition`, a run-time model (`DynamicModelFactory`) and a `FormBinding`, using the parameterised buttons added the same day (`InvokeCommand` with `ParameterFieldIds`). Queries show their reply in a text field written through the binding; send failures show as `Error: ...`.

Tests (Unit, 6 in `OoBDev.Scpi.Panels.Tests`): sections and controls, button sends the built request, reply shown and `Changed` raised, error shown, defaults seeded, null profile. Not done: wiring into `OoBDev.DevTerm.Wpf` (`IPanelFactory`), `ParameterExpressions`, a real instrument. Readme: `src/Extensions/OoBDev.Scpi.Panels/README.Scpi.Panels.md`.

Shell wiring: `ScpiPanelFactory` in `OoBDev.DevTerm.Wpf` (1 test). No transport is attached in `App` yet (phases 8-9), so no SCPI panel is registered at start-up.

Parameter expressions: `InvokeCommand` now evaluates `ButtonControl.ParameterExpressions` (an `Expression` over the sibling values) before joining; a blank or unparsable entry keeps the bare value. 2 tests in `InvokeCommandParameterTests`.
