# Shared FakeTransport (2026-11-03)

Closes the last candidate in the [test-utilities plan](../design/DevTermParity/test-utilities-plan.md).

## Done

- `OoBDev.DevTerm.TestUtilities/FakeTransport.cs` is the one in-memory `IDeviceAdapter` and `ILineStatusSource` for tests. It is the superset of the four copies: records writes (`WrittenPayloads`, `WrittenText`), answers each write (`ReplyToEveryWrite`), fails every write (`FailWritesWith`), the next write or open (`FailNextWrite`, `FailNextOpen`), hangs up or fails the read side, and reopens with a fresh pipe after a close.
- The copies in `Core.Tests` (Logging, DeviceManifests), `Devices.Tests` and `Tui.Tests` are deleted. `Devices.Tests` references the project and has the global using.
- State changes raise `StateChanged` only when the state really changes (the Logging and Tui behaviour; the other copies raised it on every set).

## Verified

`dotnet test`: `Core.Tests` 1065 passed (8 skipped), `Devices.Tests` 77 passed, `Tui.Tests` 385 passed (8 skipped); no failures. `Tui.Tests` rewrote the generated guide screenshots, which were reverted (nondeterminism is tracked in `BACKLOG.md`).
