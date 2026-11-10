# Device IO bug review: open findings

- **Status:** open, found by code reading on 2026-11-10 (reviewed: `OoBDev.System.IO.Pipelines` pump and streams, `OoBDev.System.IO.Ports` read and write streams, `Rfc2217Session`, ISO 9660 dates, `TempFileHandle`; not yet reviewed: `BridgeWorker`, `SerialDeviceAdapter`, the Bluetooth, Mqtt and Brokers projects)
- **Backlog line:** `BACKLOG.md`, bug review of migrated projects
- **Fixed in the same review (tests added):** `SerialPortWriteStream` async writes now run on the thread pool and honour a canceled token, `WriteDelayStream` array and synchronous writes (threw `NotSupportedException`), ISO 9660 directory-record day taken from the month byte, wrong sign on the GMT offset, culture and machine time zone leaking into volume dates, `TempFileHandle` finalizer able to throw on Linux

## Open

None found so far in the reviewed projects.


## Done when

Review the projects not yet reviewed (list in Status), then delete this file. `SerialPortReadStream` no longer wakes on `ErrorReceived` (it takes the concrete `SerialPort`, so it is covered only by the Hardware test, not run here).
