# OoBDev.Zoom.H4n

Zoom H4n recorder remote control: one status byte inbound, two-byte button codes outbound.

## Description

The recorder's RC04 / RC2 remote port, tapped as a serial port (2400 baud, 8-N-1, no handshake) by the h4n2rs485 adapter. Source: dev-term `Devices.ZoomH4n`. Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md).

Implements `IDeviceDefinitionReceiver<T>` and `IDeviceDefinitionTransmitter<T>` from `OoBDev.System.Abstractions`; bytes arrive through the existing adapters, so the package never opens a port.

| Type | Role |
|------|------|
| `ZoomH4nDevice` | Definition: one-byte segments, decoder and encoder |
| `ZoomH4nStatus` | Flags: Record 0x01, Peak 0x02, Mic 0x10, Led1 0x20, Led2 0x40; undocumented bits are dropped |
| `ZoomH4nMessageDecoder` | One byte to `ZoomH4nStatus` |
| `ZoomH4nButton`, `ZoomH4nAction`, `ZoomH4nCommand` | The 12 remote buttons, press or release |
| `ZoomH4nMessageEncoder` | Press: the button's two-byte code; release: `80 00` (send as two writes) |
| `ZoomH4nProtocol` | Baud rate and the init handshake constants |
| `TryAddZoomH4n()` | Registers the encoder and decoder |

## Init handshake

The recorder ignores everything until woken. The caller sends `ZoomH4nProtocol.ProbeByte` (`00`) about every 30 ms, up to 1024 times, until a received byte passes `IsWakeByte` (high bit set), then sends `ZoomH4nProtocol.WakeSequence` (`A1 80 00`). The package does not run it because only the caller owns the port; the Hardware test shows the loop.

## Verified

Unit tests: all 12 press codes, the release, status bits, stream segmentation, DI. The Hardware test (set `ZOOM_H4N_SERIAL_PORT`) ends Inconclusive without the adapter.

## Not yet verified on hardware

Everything on the wire: the handshake, the button codes, and whether the recorder echoes status for a given unit (the dev-term test only proved no fault or timeout). Not ported: the per-recording-mode LED blink patterns, and the dev-term control surface (the panel, Q3).

## Device documentation

[docs/known-configuration.md](docs/known-configuration.md) records how the device is reached on the bench (transport, framing, byte order, what was verified on hardware); the frame layouts are Kaitai Struct files in [docs/](docs/): `zoom-h4n-command.ksy`, `zoom-h4n-status.ksy`. Paths such as `docs/design/...`, `docs/test/...` and `src/DevTerm.*` inside those notes name files in the dev-term repository, where they were written.
