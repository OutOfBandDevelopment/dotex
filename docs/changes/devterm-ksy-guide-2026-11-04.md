# Device binary-format guide

**Date:** 2026-11-04

`KsyGuideTests` (`OoBDev.DevTerm.Core.Tests`, `Unit`) reads every `.ksy` under the hardware device libraries' `docs` folders and `KsyGuideRenderer` (`OoBDev.DevTerm.TestUtilities`) writes `docs/applications/devterm/api-guide/binary-formats.md`: one section per format with its title, description, length, endianness and a field table (type, size, notes), through the existing DevTerm importer. Files sort by name and `WriteIfChanged` leaves an unchanged guide alone.

Verified: the test passes and the guide holds one table per `.ksy` file (10 at the time). Left in `BACKLOG.md`: API guides for other applications and the `src/Tools` guides.
