# Bench instrument identify tests (2026-10-30)

`OoBDev.Scpi.Tests` has `BenchInstrumentTests` (category `Hardware`) for the network bench instruments, with `BenchTcpInstrument` writing one byte at a time because the serial bridges drop bursts. Settings come from `src/.runsettings` (`HP34401A_TCP_HOST`, `TDS2024_TCP_HOST`, `TEK2230_1_TCP_HOST`, `TEK2230_2_TCP_HOST`, `TCP_DEVICE_PORT`); a blank or unreachable host ends the test Inconclusive.

- HP 34401A: LF, 50 ms per byte, `*IDN?` answers `HEWLETT-PACKARD,34401A,0,5-1-1`. `SYST:REM` is not needed (the earlier refusal and the need for it came from sending bursts).
- Tektronix 2230: `ID?` with CR and 175 ms per byte, `ID TEK/2230,V81.1,VERS:13;`.
- Tektronix TDS2024: `*IDN?` answers `TEKTRONIX,TDS 2024`.

**Verified:** `dotnet test src/Extensions/OoBDev.Scpi.Tests --settings src/.runsettings --filter TestCategory=Hardware`: 3 passed, 1 skipped (the second 2230 at 192.168.0.107 is offline). Not run: Korad supplies (no test yet).

Bench quirks are in [test-hardware.md](../how-tos/test-hardware.md).
