# DevTerm configuration

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Configuration` (Extensions): options and validation, project files, profiles, themes, preferences, converter tools, capture export, stream monitor, routing, network discovery, session builder and tabs, service registration.
- 381 ported tests in `OoBDev.DevTerm.Configuration.Tests` passing. The tests isolate user data with `DEVTERM_HOME`; tests that built TCP or serial sessions use loopback until those transports are ported.
- Not done: connection editor view model and tests, BLE/HID/USBTMC/LXI options and scanners, `RoutingLinkFactory`, `AddDevTermFrontEnd` for transports other than loopback, SCPI/Z-Wave presenter expectations, DeviceConfigViewModel tests.
