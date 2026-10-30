# LxiDeviceOption tests (2026-10-29)

`LxiDeviceOptionTests` (6 Unit tests) covers the mapping from an LXI device or a discovery hit to the picker option: the raw-SCPI port or the 5025 fallback, VXI-11 dropping the port, MQTT keeping it, anything else a plain TCP hit. `LxiDeviceScanner.Scan` and `ScanNetwork` themselves open sockets and stay untested here (no scanner tests exist in dev-term either); a Docker-backed Integration test would need a fake LXI responder.
