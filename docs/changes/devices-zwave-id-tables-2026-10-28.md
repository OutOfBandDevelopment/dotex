# Z-Wave id tables

Date: 2026-10-28. Branch `dev/example-app-fixes`.

- `OoBDev.ZWave` embeds the dev-term manufacturer (with products), command class and device class tables; `ZWaveNames.Manufacturer`/`CommandClass` fall back to them before hex, and `ZWaveNames.Product`/`DeviceClass` are new. Duplicate manufacturer entries in the data are merged.
- 7 new unit tests; the Z-Wave suite is 36 passing.
- Not done: the configuration parameter table (`product-configs.json`) and the automatic ACK responder (needs a session observer; the consumer sends `ZWaveFrame.Ack` for now).
