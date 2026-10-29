# DevTerm TCP and serial transports

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Transports.Tcp` (31 tests) and `OoBDev.DevTerm.Transports.Serial` (65 pass, 1 skipped real-hardware test, category `Hardware`) ported from dev-term as pipe-based `ITransport`s.
- The real-device USB/HID reachability helper was not ported; only the serial-port check the tests need.
- Remaining transports: USBTMC, VXI-11, BLE, HID, MQTT, brokers, RFC 2217.
