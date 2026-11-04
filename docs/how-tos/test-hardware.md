# Test hardware (the bench)

**Last Updated:** 2026-10-30

The physical devices attached to the owner's test machine, and where each one is configured. The bench is set up for testing: Hardware tests may use every device below (owner, 2026-10-30). Ask the owner to watch when a test makes something visible happen (relays, lights, outputs), and run those steps one at a time.

## Contents

1. [Device list](#device-list)
2. [Where the settings live](#where-the-settings-live)
3. [Running the tests](#running-the-tests)
4. [Known gaps](#known-gaps)

## Device list

**Table 1 — Bench devices**

| Device | Connection | Setting | Notes |
|---|---|---|---|
| Korad KA3005P power supply | `COM6`, 9600 8N1 | `KORAD_KA3005P_SERIAL_PORT` | `*IDN?` answers `KORAD KA3005P V5.8 SN:03396447` (checked 2026-10-30) |
| Korad KA6003P power supply | `COM7`, 9600 8N1 | `KORAD_KA6003P_SERIAL_PORT` | `*IDN?` answers `KORAD KA6003P V5.8 SN:50266913` (checked 2026-10-30) |
| Aeotec Z-Stick (Z-Wave) | `COM13`, 115200 8N1 | `ZWAVE_SERIAL_PORT`, `ZWAVE_SWITCH_NODE` | answers `Z-Wave 2.78`; node 2 is a Binary Switch relay |
| Radex One | `COM8`, 9600 8N1 | `RADEX_ONE_SERIAL_PORT` | plain virtual COM port |
| Zoom H4n | `COM3`, 2400 8N1 via the h4n2rs485 adapter | `ZOOM_H4N_SERIAL_PORT` | remote port |
| Rigol DS1102E oscilloscope | USB (USBTMC) | none; found by enumeration | shares VID:PID 0x1AB1:0x0588 with the DG1022 |
| Rigol DM3058 multimeter | USB (USBTMC) | none | |
| Rigol DG1022 function generator | USB (USBTMC) | none | |
| Rigol DG1062Z function generator | USB (USBTMC) and LXI (VXI-11) | none; LXI found by discovery | |
| Velleman K8055 | USB HID | none | online |
| Kuando Busylight | USB HID | none | online |
| HP 34401A multimeter | TCP `192.168.0.109:23` | `HP34401A_TCP_HOST` | serial-to-Ethernet bridge; LF terminator, 50 ms between bytes (bursts are dropped), no `SYST:REM` needed; `*IDN?` answers `HEWLETT-PACKARD,34401A,0,5-1-1` (checked 2026-10-30) |
| Tektronix TDS2024 oscilloscope | TCP `192.168.0.110:23` | `TDS2024_TCP_HOST` | `*IDN?` answers `TEKTRONIX,TDS 2024,0,CF:91.1CT FV:v4.12 TDS2CM:CMV:v1.04` |
| Tektronix 2230 oscilloscope (1) | TCP `192.168.0.108:23` | `TEK2230_1_TCP_HOST` | pre-SCPI (`ID?`); CR terminator, 175 ms between bytes; answers `ID TEK/2230,V81.1,VERS:13;` (checked 2026-10-30; the first reply after connecting can be a stale `STATUS 97;READY;` line) |
| Tektronix 2230 oscilloscope (2) | TCP `192.168.0.107:23` | `TEK2230_2_TCP_HOST` | offline on 2026-10-30 |

COM numbers change when a USB adapter is replugged: confirm them before blaming a test.

## Where the settings live

- `src/.runsettings` holds every parameter, under "Hardware tests". It is the single settings file for all test runs.
- `TEST_VARIABLES.md` documents each parameter (section "Hardware").
- Tests read them with `TestContext` properties, never environment variables (rule `testing`). A blank or absent value, or a device that is not found, ends the test `Inconclusive`.
- Devices without a setting (USB HID, USBTMC, LXI) are found by enumeration.

## Running the tests

```bash
cd src
dotnet test <TestProject> --settings .runsettings --filter "TestCategory=Hardware"
```

One project per call; two runs at once fight over the same port.

## Known gaps

- The Korad Hardware tests (`KoradSupplyTests` in `OoBDev.Scpi.Tests`) are read-only: identify, setpoint queries and the status byte. They never set a value or switch the output on. Written 2026-11-04 with the supplies not attached, so only the Inconclusive path ran; run them once with the supplies on COM6 and COM7.
- The DevTerm identify tests through the command-line mode (`RealHardwareCliTests` in `OoBDev.DevTerm.Tui.Tests`, category `Hardware`) use the parameters above, with each device terminator and byte delay from the table. They also ended Inconclusive on 2026-11-04 because the bench network was offline.
