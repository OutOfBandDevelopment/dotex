# DevTerm RFC 2217 and VXI-11 transports

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Transports.Rfc2217` (46 pass, 1 skipped) and `OoBDev.DevTerm.Transports.Vxi11` (6 pass, 1 skipped), ported from dev-term as pipe-based `ITransport`s.
- Skipped tests are the ser2net Integration test and the real-hardware VXI-11 test (`Hardware`), which end inconclusive without their runsettings parameters.
- Remaining transports: USBTMC, HID, BLE, MQTT, brokers.
