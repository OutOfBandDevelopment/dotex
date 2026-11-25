# USBTMC: DM3058E bulk-IN read fails with "Pipe"

**Status:** open, to be reviewed later (owner, 2026-11-24)
**Feature:** USBTMC driver `OoBDev.System.IO.UsbTmc` (found through the former USB bridge, removed 2026-11-25)
**Backlog:** the USB passthrough line in [BACKLOG.md](../../BACKLOG.md)

## Finding

- Rigol DM3058E (`1AB1:09C4`, serial `DM3R232301438`): `*IDN?` fails with `USBTMC bulk-IN transfer failed: Pipe` (from `UsbTmcDeviceAdapter.ReadBulkInOrAbort`).
- It fails the same way with the direct adapter and through the bridge, so the bridge is not the cause. Writes work: `:FUNCtion:CAPacitance` changed the meter's mode over the bridge.
- The Rigol DG1022 (`1AB1:0588`) on the same machine answers `*IDN?` directly and through the bridge.
- Earlier bench notes list the DM3058 as passing USBTMC (2026-11-05, [test-hardware](../how-tos/test-hardware.md)), so this may be a regression or a device state.

## To review

- Review the USBTMC bulk-IN path against the DM3058E: request size and the transfer-size limit the device reports, short-packet and zero-length-packet handling, the abort and clear sequence after a pipe error, and the bTag handling.
- Compare with the working DG1022 and the earlier DM3058 pass.
- Hardware test: `RealHardwareBridgeTests.UsbTmc_IdentifyThroughTheBridge_Answers` (DM3058E row) is Inconclusive until this is fixed; it should then pass without change.
