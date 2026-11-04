# Bench Hardware tests (2026-11-04)

## What changed

- `KoradSupplyTests` (`OoBDev.Scpi.Tests`, category `Hardware`) for the KA3005P and KA6003P on `KORAD_KA3005P_SERIAL_PORT` and `KORAD_KA6003P_SERIAL_PORT`: identify, setpoint queries within the model range, and the one-byte status reply. Read-only; the output is never switched on. `BenchSerialInstrument` reads unterminated replies until the line goes quiet.
- `RealHardwareCliTests` (`OoBDev.DevTerm.Tui.Tests`) moved from the `DevLocal` category and dev-term's `RealTcpDevice*` parameters to category `Hardware` and the shared `*_TCP_HOST` and `TCP_DEVICE_PORT` parameters in `src/.runsettings`. Each row carries the device query, line ending and `--writebytedelayms` from `docs/how-tos/test-hardware.md`; the HP 34401A row is new.

## Verification

- Both classes build and run. On 2026-11-04 the Korad ports (COM6, COM7) did not exist and the bench network (192.168.0.107 to .110) was unreachable, so all 8 and all 4 tests ended Inconclusive. The assertions against real replies have not run yet.
