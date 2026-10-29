# OoBDev.Kuando.Busylight

Kuando Busylight USB HID status light (vendor 0x04D8, product 0xF848): command report (colour, on/off time, audio), 7-step program report, poll and identification reply.

## Description

Hardware driver, so it lives in `src/HardwareDevices` beside the other device packages, apart from the generic protocols in `src/Extensions`. Source: dev-term `Devices.Busylight` plus merge M1 from `mine/binary-data-decoders` (step record, program frame). Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).

The device is `[UsbHid(0x04D8, 0xF848)]`; bytes go through `OoBDev.System.IO.UsbHids`, so the package never opens the device. The light is told its whole state, so the message is the state.

| Type | Role |
|------|------|
| `BusylightDevice` | Receiver of `BusylightIdentification`, transmitter of `BusylightCommand` |
| `BusylightCommand` / `BusylightColor` / `BusylightAudio` | One 8-byte step record: next step, repeat, R, G, B, on, off, audio byte (play bit 7, track bits 6-3, volume bits 2-0) |
| `BusylightMessageEncoder` | Command to the verified 9-byte write (report id `00` + record) |
| `BusylightSequence` | `Encode` (1 to 7 steps + commit record `06 04 55 FF FF FF` + big-endian sum checksum) and `Poll()` |
| `BusylightColors` | Presets (Red, Green, Blue, Yellow, Off) and `TryParse` of a name or `r,g,b` |
| `BusylightSegmenter` / `BusylightMessageDecoder` | One HID read is one report; printable ASCII shown, the rest as dots |
| `ServiceCollectionExtensions` | `TryAddBusylight()` |

State holding (colour, track, volume set separately and applied together) is a consumer concern; the control surface waits on owner question Q3.

## Verified

- Unit tests: single command, audio byte, colour parsing, poll request and program frame against recorded bytes.
- Hardware tests (run against the plugged-in light on 2026-10-28): the red, green, blue single commands and the poll, which answers with the `PLENOM` identification.
- The program frame is built big-endian like the dev-term traces. The BinaryDataDecoders `Class1` frame carries the same sum little-endian (`86 0C`), which may be why it never acted on the device; the `Sequence_SixColorSteps` Hardware test sends the big-endian frame, but only the send is asserted, since whether the light cycles has to be watched.

## Not yet verified on hardware

That the program frame makes the light cycle through its steps.
