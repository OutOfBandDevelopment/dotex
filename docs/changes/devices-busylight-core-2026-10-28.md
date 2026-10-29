# Busylight status light and the HardwareDevices folder

Date: 2026-10-28. Branch `dev/example-app-fixes`.

- `OoBDev.Kuando.Busylight`: `BusylightDevice` (`[UsbHid]`), `BusylightCommand`/`BusylightColor`/`BusylightAudio`, `BusylightMessageEncoder` (9-byte write), `BusylightSequence` (7-step program frame, poll), `BusylightColors`, segmenter, decoder, `TryAddBusylight()`. Merge M1 applied.
- Finding: the BinaryDataDecoders program frame stores its checksum little-endian (`86 0C`) while dev-term traces are big-endian; the frame is built big-endian here.
- Hardware tests passed on the plugged-in light (colours, poll answers `PLENOM`).
- New folder `src/HardwareDevices` (solution folder `/HardwareDevices/`): the Busylight, DE-5000, K8055, Radex One and Zoom H4n packages and tests moved from `src/Extensions` (owner request). Generic protocols (NMEA, SCPI, Z-Wave) stay in `Extensions`. Relative paths are unchanged because the depth is the same.
- Not done: confirming the program frame animates the light; control surface (Q3).
