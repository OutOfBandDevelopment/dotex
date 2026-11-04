# Whole-solution restore works again

**Date:** 2026-11-04

`dotnet restore src/OoBDev.slnx` failed with NU1201: the `OoBDev.Common` aggregator's glob pulled in `OoBDev.System.IO.Bluetooth.Windows`, which targets `net10.0-windows10.0.19041.0`, into a `net10.0` project. The aggregator now removes `OoBDev.*.Windows.csproj` like it already removes the Wpf projects (Windows-only libraries are packaged on their own).

Verified: restore of the whole solution has no errors; `OoBDev.Common` builds.
