# DevTerm observability

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Observability` (Extensions): `TelemetryExporter` (OTLP export of the DevTerm activity source and meter), 7 ported tests passing.

## Moved to `OoBDev.Telemetry` (step 3b of the restructure)

`OoBDev.DevTerm.Observability` and its tests are deleted. `TelemetryExporter` now lives in the host-free Framework library `OoBDev.Telemetry` (namespace `OoBDev.Telemetry`) and takes the source and meter names as parameters (`Start(endpoint, serviceName, sourceNames)`); DevTerm Configuration passes `DevTermTelemetry.Name`. The three exporter tests moved to `OoBDev.Telemetry.Tests` (13 Unit tests pass); Configuration still passes 381.
