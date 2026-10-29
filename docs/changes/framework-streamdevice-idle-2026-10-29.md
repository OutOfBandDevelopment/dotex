# StreamDevice idle transmitter (2026-10-29)

`StreamDevice<T>` kept its transmit queue in a `ConcurrentQueue` and its transmitter looped on an event that stays set, so with nothing to send it spun a core (703 ms of processor time in 700 ms). The queue is now an unbounded single-reader `Channel<T>` and the transmitter waits on `WaitToReadAsync` before draining, so an idle device uses no processor time. `Transmit` and `minimumTransmissionDelay` behave as before; cancellation ends the loop quietly.

Tests (Unit, 3 added in `OoBDev.System.Tests`, 1858 Unit pass): encoded write, two messages in order, and an idle test that measures the processor time added over a 700 ms window against a baseline window (it failed at 703 ms against the old code).
