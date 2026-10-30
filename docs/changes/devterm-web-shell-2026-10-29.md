# DevTerm Web shell and connection editor (2026-10-29)

Step 6b of the [restructure plan](../design/DevTermParity/restructure-plan.md).

## Done

- `src/Applications/OoBDev.DevTerm.Web` (Blazor, SignalR, REST, OpenAPI with Scalar, AsyncAPI) and `OoBDev.DevTerm.Web.Tests` ported from dev-term.
- `OoBDev.DevTerm.Core.Configuration`: `ConnectionEditorViewModel` and the BLE, HID, LXI and USBTMC option classes and scanners (`BlePlatformAdapterLoader` loads `OoBDev.System.IO.Bluetooth.Windows` by reflection), with their tests and a `StubInstrumentPanelProvider`.
- `AddDevTermFrontEnd` now selects loopback, tcp, hid, usbtmc, ble, rfc2217, vxi11, mqtt, amqp, stomp and serial (default) and binds the matching `System.IO.*` adapter options from `CliOptions`.
- Core csproj references Bluetooth, Ports, UsbHids and UsbTmc.

## Verified

Core.Tests 1035 passed, 5 skipped; Web.Tests Unit 43 passed, 4 ignored, 0 failed. Playwright and hardware tests (`DevLocal`) were not run.

## Not done

k8055 and busylight panels (4 Web tests ignored), `BundleDevicePlugins`, a scpi presenter in Core, merging Web's AsyncAPI/OpenAPI helpers with the framework ones. Tracked in `BACKLOG.md`.
