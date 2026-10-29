# OoBDev.Velleman.K8055

Velleman K8055 USB experiment board (vendor 0x10CF, product 0x5500): the unprompted 9-byte input report and the output and counter-reset commands.

## Description

Hardware driver, so it lives in `src/HardwareDevices`. Source: dev-term `Devices.K8055` with merge M2 (named input flags) from `mine/binary-data-decoders`. Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).

The device is `[UsbHid(0x10CF, 0x5500)]`; bytes go through `OoBDev.System.IO.UsbHids`, so the package never opens the device. Board addresses 1 to 3 (jumpers SK5/SK6) use products 0x5501 to 0x5503 and need their own definition class.

| Type | Role |
|------|------|
| `K8055Device` | Receiver of `K8055Reading`, transmitter of `K8055Command` |
| `K8055Reading` | Digital input byte, two analogue inputs, two 16-bit counters |
| `K8055DigitalInputs` | Named bits of the digital input byte: `I1=0x10`, `I2=0x20`, `I3=0x01`, `I4=0x40`, `I5=0x80` |
| `K8055MessageDecoder` | 9-byte report to `K8055Reading` (byte 0 is the HID report id) |
| `K8055Command` / `K8055CommandKind` / `K8055Outputs` | Reset counter 1 (0x03) or 2 (0x04), or set all outputs at once (0x05) |
| `K8055MessageEncoder` | Command to the 9-byte write (report id `00` first) |
| `ServiceCollectionExtensions` | `TryAddK8055()` |

The board sets every output with one report, so `K8055Outputs.WithDigital` returns the new state and the caller sends the whole of it; holding that state is a consumer concern.

## Verified

- Unit tests: decoder on the report dev-term recorded from a real board, named input bits, encoder frames, output state, stream segmentation, DI.
- Hardware tests (reset counter, set outputs then all off, read a streamed report) are ported and end Inconclusive when no board is plugged in. They have not run against a board yet.

## Not yet verified on hardware

Everything that needs the board, in particular which physical input pin sets which bit (the names come from BinaryDataDecoders; dev-term published the raw byte because it never wired an input) and the two trailing duration bytes, which are sent as zero.
