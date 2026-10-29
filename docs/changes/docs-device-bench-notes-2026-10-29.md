# Device bench notes carried from dev-term

**Date:** 2026-10-29

Copied the dev-term `docs/devices/` material next to the packages it describes: `known-configuration.md` and the Kaitai Struct `.ksy` frame layouts for DE-5000, Busylight, K8055, Radex One and Zoom H4n (`src/HardwareDevices/<package>/docs/`), the Aeotec Z-Stick S2 notes (`OoBDev.ZWave/docs/`), and the nine instrument folders with vendor programming manuals plus the USR-TCP232-302 bridge notes (`OoBDev.Scpi/docs/`). Links to dev-term `docs/design`, `docs/test` and `docs/bugs` were rewritten as plain references naming the dev-term repository, so no link points at a missing file. Each package readme gained a "Device documentation" or "Instrument documentation" section.

**Verified:** all 18 relative links in the copied markdown resolve; `OoBDev.Docs.Tests` Unit tests pass (4) with the seven folders as scope. Not done: the numbering part of the documents review waits on Q4; `docs/user-guide`, `docs/specs` and `docs/design` of dev-term are not carried (they describe the DevTerm shell, Q3).
