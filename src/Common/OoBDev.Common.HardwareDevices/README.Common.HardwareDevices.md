# OoBDev.Common.HardwareDevices

Aggregator for the hardware device drivers in `src/HardwareDevices`, in the same way `OoBDev.Common.Extensions` aggregates `ExternalServices`.

## Description

MSBuild globs reference every project under `src/HardwareDevices` except the test projects, so a new device package is included without editing this project. The readmes of the device packages are packed with it.

| Type | Role |
|------|------|
| `ServiceCollectionExtensions.TryCommonHardwareDeviceExtensions` | Calls each device's `TryAdd*` (`TryAddBusylight`, `TryAddDe5000`, `TryAddK8055`, `TryAddRadexOne`) |

The globs include every device project, but registration is a line per device, so a new package adds its `TryAdd*` call here when it gets one (the Zoom H4n has none yet). Generic protocols (NMEA, SCPI, Z-Wave) stay in `src/Extensions`.

## Verified

Builds with the device projects. No behaviour of its own beyond the registration calls, which each package tests.
