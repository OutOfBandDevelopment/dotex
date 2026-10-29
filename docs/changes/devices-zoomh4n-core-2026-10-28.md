# Zoom H4n remote

Date: 2026-10-28. Branch `dev/example-app-fixes`.

- `OoBDev.Zoom.H4n`: `ZoomH4nDevice`, `ZoomH4nStatus` (flags), `ZoomH4nMessageDecoder`, `ZoomH4nButton`/`ZoomH4nAction`/`ZoomH4nCommand`/`ZoomH4nMessageEncoder`, `ZoomH4nProtocol` (baud rate, handshake constants), `TryAddZoomH4n()`; wired into `Common.HardwareDevices`.
- 22 unit tests pass (12 press codes, release, status bits, segmentation, DI); the Hardware test (handshake, then Mic) is ported and ends Inconclusive without the adapter (`ZOOM_H4N_SERIAL_PORT`, blank by default).
- The handshake loop is left to the caller (the Hardware test shows it), because the package never owns the port.
- Not done: a physical pass, LED blink patterns, the control surface (Q3).
