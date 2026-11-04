# DevTerm device notes live with the device libraries

**Date:** 2026-11-04

The imported `docs/applications/devterm/devices/` copies were deleted. The library copies (`src/Extensions/OoBDev.Scpi/docs`, `src/Extensions/OoBDev.ZWave/docs` and `src/HardwareDevices/*/docs`) are the source; they differed from the imports only by repointed links.

- Stale "in the dev-term repository" and `dev-term docs/...` pointers in the library notes now name this repository's paths (`docs/protocols/`, `docs/applications/devterm/{test,bugs,changes}`).
- The SCPI profile `Notes` fields and the Tektronix 2230 manual no longer cite `docs/devices/`.
- The device index moved to a section in [test hardware](../how-tos/test-hardware.md#where-each-devices-notes-live).
- Verified: whole-`docs` validator clean apart from imported DevTerm history.
