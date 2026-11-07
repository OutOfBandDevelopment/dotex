# OoBDev.Fencing.ScoreMachines

Fencing score machine decoders (Favero and Saint George): frames read from a serial stream become one `IScoreMachineState` (both fencers' score, lights, cards and priority, the clock and the match number).

## Description

Both machines transmit unprompted, so the devices only receive. Bytes arrive through the existing serial adapters; the package never opens a port. Source: BinaryDataDecoders `ElectronicScoringMachines.Fencing`.

| Type | Role |
|------|------|
| `FaveroDevice` | `IDeviceDefinitionReceiver<IScoreMachineState>`, 2400 baud 8N1, 10-byte frames starting `FF` |
| `FaveroStateParser` / `FaveroMessageDecoder` | Frame to state (BCD fields); an empty frame decodes to a `FormatException` |
| `SgDevice` | Saint George, 9600 baud 8N1, frames from `SOH` to `EOT` (max 100 bytes) |
| `SgStateParser` / `SgMessageDecoder` | Each frame carries one fact (score, clock, lights, cards), so the parser keeps the last state and applies the new fact to it |
| `ScoreMachineState`, `Fencer`, `Cards`, `Lights`, `MachineType` | The decoded state |
| `ServiceCollectionExtensions` | `TryAddFencingScoreMachines()`: keyed decoders `favero` and `saint-george` (`FencingGlobals`) |

| `Panels/FaveroPanel`, `Panels/SaintGeorgePanel` | DevTerm device panels (`IDevicePanel`, serial only, presenter names `favero` and `saint-george`): read-only indicators for the clock, match and each side's score, lights, cards and priority; no controls |
| `Panels/ScoreMachineDecoder` | `IDeviceDecoder`: cuts the stream into frames, buffers partial ones, returns one state line per frame (`BAD FRAME: <hex>` when the parser rejects one) and publishes only changed values |

The Favero checksum is not verified (the source never found a matching formula).

## Verified

Unit tests cover the maker's Favero example, every lamp, card and priority bit, BCD digits, bad prefixes, and Saint George frames built from the layouts in the source notes. The panel tests cover split chunks, noise before a frame, repeated frames, the Saint George length limit and a malformed frame. No frame was captured from a real machine in dotex.

## Not yet verified on hardware

No Hardware test: no machine is attached. The Saint George priority frame is ported as in the source, where priority is never set (tracked in `BACKLOG.md`).
