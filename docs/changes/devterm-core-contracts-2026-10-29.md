# DevTerm engine contracts

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- New `OoBDev.DevTerm.Core` (Framework) with `ITransport`, `ConnectionState`, `ConnectionStateChangedEventArgs`, `IPresenter`, `StreamToPipePump` and `StreamTransport`, ported from dev-term's `DevTerm.Core`. First slice of the [DevTerm parity](../design/DevTermParity/README.md) engine (group A).
- `StreamTransport` wraps any `IDeviceAdapter` as an `ITransport` (default A of open question Q1: `IDeviceAdapter` stays, `ITransport` is added beside it).
- 12 unit tests pass (pump: data, read failure, cancellation, null arguments; transport: open and read, write, state changes, reopen, failed open, write before open, close when closed, null adapter).
- Not done: the reverse adapter (a stream view over a transport), the loopback transport, the session engine, logging and configuration; nothing was run against a real device. Q1 is still open for the owner.
