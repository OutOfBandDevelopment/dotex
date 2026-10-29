# Z-Wave ACK responder (2026-10-29)

`ZWaveAckResponder.Respond(frame)` picks the one-byte answer the Serial API wants for each received unit: `ZWaveFrame.Ack` for a checksum-valid data frame, the new `ZWaveFrame.Nak` for a frame that starts with `0x01` but fails its checksum, and `null` for lone ACK, NAK and CAN bytes and stray bytes. The hardware test helper `ZWaveStick` now uses it instead of its own check.

The framework has no session observer, so the package does not write the answer itself: the consumer writes what `Respond` returns, as `ZWaveStick` does. A responder that owns the port would need a session abstraction first; `README.ZWave.md` says so.

Tests (Unit, 37 pass in `OoBDev.ZWave.Tests`): data frame, bad checksum, lone control bytes and stray bytes, null argument. The Hardware test was not run in this change.
