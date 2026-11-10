# Device IO bug review: open findings

- **Status:** open, found by code reading on 2026-11-10 (reviewed: `OoBDev.System.IO.Pipelines` pump and streams, `OoBDev.System.IO.Ports` read and write streams, `Rfc2217Session`, ISO 9660 dates, `TempFileHandle`; not yet reviewed: `BridgeWorker`, `SerialDeviceAdapter`, the Bluetooth, Mqtt and Brokers projects)
- **Backlog line:** `BACKLOG.md`, bug review of migrated projects
- **Fixed in the same review (tests added):** ISO 9660 directory-record day taken from the month byte, wrong sign on the GMT offset, culture and machine time zone leaking into volume dates, `TempFileHandle` finalizer able to throw on Linux

## Open

1. **`SerialPortReadStream.ReadAsync` can block on a benign `ErrorReceived`.** The wait loop ends when `ErrorReceived` or `DataReceived` completes the task, then calls the synchronous `port.Read`. After a framing, parity or overrun error `BytesToRead` can still be 0, so `Read` blocks for `ReadTimeout` (infinite by default) and ignores cancellation. Fix: loop the whole wait until `BytesToRead > 0`, and treat the error event as a wake-up only. Needs the Hardware or a fake `SerialPort` to test; the read stream takes the concrete type.
2. **`SerialPortWriteStream.WriteAsync` is synchronous.** It calls `port.Write` inline and ignores the cancellation token, so a stalled handshake line blocks the caller. Fix: run the write on the thread pool and honour `WriteTimeout`.
3. **`WriteDelayStream` overrides only `WriteAsync(ReadOnlyMemory<byte>)`.** `Write` throws `NotSupportedException`, and the array overload of `WriteAsync` routes to `Write`, so callers using it fail. Fix: override the array overloads to forward to the memory one.

## Done when

Each item has a test (a fake stream for 3, a fake or hardware port for 1 and 2), then this file is deleted and the decisions are in the project readmes.
