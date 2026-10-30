# SCPI presenter and instrument panel in OoBDev.DevTerm.Devices (2026-10-29)

Core must not reference device libraries, so the SCPI pieces of dev-term that depend on `OoBDev.Scpi` live in the composition bundle.

## What changed

- `OoBDev.DevTerm.Devices.Scpi`: `ScpiReplyPresenter` (`scpi`), `ScpiAutoDetect` with its outcome and result types, `ScpiUiDefinitionBuilder`, `ScpiControlSurface` (with command preview and image reply hint) and `ScpiInstrumentPanelProvider`.
- `AddDevTermDevicePanels()` also registers the presenter and the `IInstrumentPanelProvider`.
- New `OoBDev.DevTerm.Devices.Tests` project with 73 Unit tests ported from dev-term; profile, catalog and value-path tests stay in `OoBDev.Scpi.Tests`.
- Tui Unit 355 passed (1 skipped), Web Unit 47 passed.

## Not run

DevLocal and hardware tests (no instrument attached).
