# DevTerm Tui shell (2026-10-29)

Step 6c of the [restructure plan](../design/DevTermParity/restructure-plan.md).

## Done

- `src/Applications/OoBDev.DevTerm.Tui` (Terminal.Gui) and `OoBDev.DevTerm.Tui.Tests` ported from dev-term `DevTerm.Console`, with `OoBDev.DevTerm.Core` types and `IDeviceAdapter` in place of `ITransport`.
- `LxiDeviceScanner` (VXI-11 discovery over `OoBDev.System.IO.Tcp`) added to `OoBDev.DevTerm.Core.Configuration`.
- Test doubles moved to the pipe-based adapter (`LoopbackTransport`, `FakeTransport`), plus the shared test helpers (`FakeDeviceConfigEditor`, `ManualTimeProvider`, `ChildProcessJob`, `StreamContentSamples`, `RawPresenter`, `ReplyCollector`, `RealDeviceReachability`).
- Tests isolate user data with `DEVTERM_HOME` in a temp folder (`TestEnvironment`).

## Verified

Tui.Tests Unit: 355 passed, 1 skipped, 0 failed. The expression picker and converter tools screenshot tests are `DevLocal` (they write into `docs/user-guide/images`). Hardware, broker and Playwright tests were not run, and the Tui was not run interactively.

## Not done

Removed tests that need unported pieces (scpi, k8055, busylight, USBTMC transport, serial and TCP hardware, loopback screenshots); `BundleDevicePlugins`; the Windows BLE backend copy target. Tracked in `BACKLOG.md`.
