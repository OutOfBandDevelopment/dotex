# Device parity: dev-term against BinaryDataDecoders

**Created:** 2026-10-28 · **Status:** phase 0 first pass of the [device decoding migration plan](device-decoding-migration-plan.md). The comparison is by reading the code; nothing was run, and no hardware was available.

Base (owner, 2026-10-26): dev-term (`C:\repo\oobdev\dev-term`, branch `dev/zstick-s2`, `d777358`). Compared source: `C:\repo\mine\binary-data-decoders` (HEAD `e590b420`). Only parts that dev-term lacks are listed for merging; everything else is rejected with the reason. Line counts are `.cs` lines without `bin` and `obj`.

## Contents

1. [Summary](#1-summary)
2. [Per device](#2-per-device)
3. [Merge list](#3-merge-list)
4. [Not yet checked](#4-not-yet-checked)

## 1. Summary

**Table 1 — Size and tests per device**

| Device | BinaryDataDecoders (src / tests) | dev-term (src / test methods) | Verdict |
|---|---|---|---|
| Busylight | 144 lines / 2 DevLocal tests | 369 lines / 18 test methods (2 real hardware) | dev-term base; merge the step-sequence structs |
| K8055 | 148 / none | 374 / tests present | dev-term base; merge the named digital input bits |
| NMEA | 317 real lines (about 6,000 more are generated WMI notes) / none | 486 / tests present | dev-term base; nothing to merge |
| Radex One | 759 / 1 test | 772 / tests present | dev-term base; merge the serial and version layout (and the unit claim, as a hypothesis) |
| Zoom H4n | 154 / none | 418 / tests present | dev-term base; nothing to merge |

## 2. Per device

**Busylight.** BinaryDataDecoders models the 8-byte step command (`NextStep`, `Repeat`, three colour bytes, `On`, `Off`, audio byte) as structs, plus the audio byte (`0x80 | track << 3 | volume`). dev-term has the same audio byte inside `BusylightControlSurface.BuildFrame`, but it sends one step only; its `programSequence` command is a no-op. The BinaryDataDecoders `Class1` is a hardware loop with commented experiments and is rejected. The 16-step frame layout seen in `Class1` (a 64-byte report of consecutive 8-byte steps) is the input for the merge.

**K8055.** BinaryDataDecoders names the digital input bits: `I1=0x10`, `I2=0x20`, `I3=0x01`, `I4=0x40`, `I5=0x80`, and the command bytes (`0x03` and `0x04` reset counters, `0x05` set outputs). dev-term has the same commands and 9-byte frames but publishes the digital inputs as a raw hex value and notes that the bit mapping still needs confirming. The bit names are the missing part. The request and response structs are rejected: dev-term already decodes the same offsets.

**NMEA.** dev-term decodes GGA, RMC, GSA, GSV and VTG. BinaryDataDecoders has typed classes for the same sentences (GGA, GSA, RMC, GSV) with a fix-quality enum, but its decoder has the GSV and RMC branches commented out and the rest is WMI and HID scratch notes. Nothing to merge. The fix-quality enum text is compared against dev-term's fix text in the merge task below.

**Radex One.** Both use the `0xFF7B` and `0x7AFF` prefixes and the same `0xFFFF - sum % 0xFFFF` checksum. dev-term reads the read-data reply as three 16-bit values, each followed by a reserved word, and verifies the inner checksum, all checked against a captured trace. BinaryDataDecoders reads the same reply as three 32-bit integers and scales them as `/100` microsieverts per hour and microsieverts; dev-term states the unit is unconfirmed. BinaryDataDecoders formats the serial number as grouped numeric fields and the version as `major.minor`; dev-term shows the raw ASCII payload. The write-settings request in BinaryDataDecoders has its setting fields zeroed (the real expression is commented out), so it is incomplete and rejected.

**Zoom H4n.** The button codes (record, play, stop, fast forward, rewind, volume, mic, channels) and the status bits (record, peak, mic, LED 1, LED 2) are identical; dev-term writes the codes as two bytes where BinaryDataDecoders writes one 16-bit value. dev-term adds the wake-up handshake and the release code. Nothing to merge. The richer protocol notes in `mine/proving-grounds` are outside this comparison (see [section 4](#4-not-yet-checked)).

## 3. Merge list

**Table 2 — Parts to merge into the dev-term base**

| Id | Device | Part | Source | Condition |
|---|---|---|---|---|
| M1 | Busylight | Step command and colour structs, 16-step sequence frame, to give `programSequence` a real implementation | `BusylightCommand`, `BusylightColor`, `Class1.cs` | Test with a frame-builder unit test; confirm on hardware (DevLocal) |
| M2 | K8055 | Named digital input flags `I1` to `I5` | `DigitalInputs.cs` | The bit order is not the numeric order (`I3` is `0x01`), so confirm on hardware before dropping the raw value |
| M3 | Radex One | Serial number and version layout | `ReadSerialNumberResponse.cs` | dev-term's own note says its reserved bytes do not fully reconcile with the documentation; verify against a capture |
| M4 | Radex One | Unit scaling (`/100` µSv/h and µSv) | `ReadValuesResponse.cs` | Hypothesis only: record it as a documented unit claim until a capture confirms it, because the field widths differ (32-bit against 16-bit plus reserved word) |
| M5 | NMEA | Fix-quality names | `FixQuality.cs` | Merge only if dev-term's text lacks a name |

## 4. Not yet checked

- Whether the dev-term tests already cover M1 to M5 (they were not read in this pass).
- The other sources named in [incoming-harvest-crosscheck.md](incoming-harvest-crosscheck.md) (`mine/proving-grounds` Zoom H4n and Busylight notes and providers).
- De-5000, Scpi and Z-Wave: no BinaryDataDecoders counterpart, so they have no phase 0 comparison.
