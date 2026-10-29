# Z-Wave product configuration table

**Date:** 2026-10-29

Ported dev-term `product-configs.json` (2.2 MB, embedded in `OoBDev.ZWave`) with `ZWaveConfigParameter` and `ZWaveNames.ConfigParameters(manufacturer, type, id)`. The table maps 1,237 manufacturer/type/id keys to 830 configuration files, each listing parameters (index, label, size, range, default, help, named values). A size of 0 in the data is taken from the type (`short` 2, `int` 4, otherwise 1); an array where one value is expected takes its first element. Unknown products return an empty list. The data is factual parameter documentation as collected in dev-term; none of the OpenZWave or PyZwaver code is used.

**Verified:** `OoBDev.ZWave.Tests` Unit, 39 passed (two new: a known Aeotec product lists parameter 81 with size 1, default 1 and two items; an unknown product is empty).

The configuration panel that consumes it waits on owner question Q3. Readme: `README.ZWave.md`.
