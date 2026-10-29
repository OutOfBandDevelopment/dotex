# OoBDev.Scpi

SCPI-style text instrument control: command and reply lines, with the command sets declared as JSON profiles.

## Description

`ScpiDevice(profile)` is an `IDeviceDefinitionReceiver<ScpiReply>` and `IDeviceDefinitionTransmitter<ScpiRequest>` from `OoBDev.System.Abstractions`; bytes travel through the existing adapters (serial, TCP, USB), so the package never opens a port.

- **Profiles.** `ScpiInstrumentProfile` holds a name, an `*IDN?` match pattern, the terminator, notes and commands. A command has a template with `{Name}` tokens, parameters (numeric with bounds, fixed-width formatting and padding, choice, text), `IsQuery` and an expected response format.
- **Building commands.** `profile.Build("vset", ["5"])` returns a `ScpiRequest` (wire text with the terminator, query flag, response format). Substitution is a single pass; numeric values are clamped and formatted (`DecimalPlaces`, `IntegerDigits`), so the Korad `VSET1:05.00` fixed-width form is produced. `BuildCustom` sends text verbatim.
- **Replies.** A profile with a terminator gets `ScpiReplySegmenter` (CR, LF or CRLF each end a line); a profile with an empty terminator (Korad) treats each arrival as one reply. `ScpiMessageDecoder` returns `ScpiReply.Text` without line endings.
- **Catalog.** `ScpiProfileCatalog.Create(dropInDirectories)` loads the bundled profiles (embedded JSON) and any `*.json` in the given directories. Bad files are skipped and listed in `LoadErrors`. `TryMatchByIdn(reply)` picks a profile from an `*IDN?` reply; `ScpiProfileCatalog.Generic` is the fallback.
- `services.TryAddScpi()` registers the codecs and the bundled catalog.

Bundled profiles: HP/Agilent/Keysight 34401A, Korad KA3005P and KA6003P, Rigol DG1022, DG1062Z, DM3058E and DS1102E, Tektronix 2230 and TDS2024. Their notes cite documents that live in the dev-term repository.

Source: dev-term `Devices.Scpi` (base); BinaryDataDecoders has no SCPI package. The generated control panel, reply tracker and auto-detect flow of dev-term depend on its UI framework and come with the DevTerm merge. Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md).

## Limits

- A reply that is a definite-length block (`#<n><length>...`, such as a screen dump) is not framed; the line segmenter would split it at the first CR or LF inside the data. `ExpectedResponseFormat` is carried for the layer that handles those replies.

## Not yet verified on hardware

No instrument has been connected to this package. The profile contents are as verified in dev-term (the Korad and Tektronix notes come from real devices there).
