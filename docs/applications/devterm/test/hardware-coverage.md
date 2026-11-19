# Hardware test coverage

**Last Updated:** 2026-11-19

One row per bench device: when it last passed against real hardware and how, so the least-tested devices are visible. Update the row in the same change as every bench pass (skill `hardware-test`; the pass itself is written to `{yyyy-MM-dd-HH-mm-ss}.md` in this folder, or to the day's file in `docs/changes/` for the Hardware category run). Device list and settings: [test-hardware](../../../how-tos/test-hardware.md).

**Table 1 — Last verified per device** (oldest first; "never" means no passing real-device run is recorded)

| Device | Last passed | By | Result | Gap |
|--------|-------------|----|--------|-----|
| Korad KA3005P, KA6003P (`Korad_*`) | never by the Hardware test (2026-11-19 and 2026-11-04: COM6 and COM7 absent, 8 of 8 Inconclusive); last mentioned in a bench report 2026-10-02 | `KoradSupplyTests` | assertions against real replies have not run | ports absent; output is never switched on by the tests |
| HP 34401A (`Hp_34401a`) | never by the Hardware test (2026-11-04: bench network unreachable, 4 of 4 Inconclusive); last in a bench report 2026-10-02 | `RealHardwareCliTests` | not run | network devices at 192.168.0.107 to .110 need the bench LAN |
| Tektronix 2230 (`Tektronix_2230`) | last in a bench report 2026-10-02 | bench report | 2026-11-19 raw probe of 192.168.0.108 answered `ID TEK/2230,V81.1,VERS:13;`; the Hardware test failed (empty reply) | [2026-11-19 report](2026-11-19-10-32-39.md) | see report | settle time after connect |
| Delorme Earthmate BT-20 (`Delorme_EarthmateBt20`) | last in a bench report 2026-09-29 | bench report | not re-run since | Bluetooth LE; one report only |
| DE-5000 (`DerEe_De5000`) | no bench report mentions it | none | never tested on the bench | |
| K8055 (`Velleman_K8055`) | 2026-11-04 | Hardware tests | 3 passed, 9 skipped | only one board is plugged in; the other boards work but skip as expected (owner) |
| Rigol DS1102E, DM3058E, DG1022, DG1062Z (`Rigol_*`) | last in bench reports 2026-10-09 | bench report | not re-run since | |
| Tektronix TDS2024 (`Tektronix_Tds2024`) | last in a bench report 2026-10-09 | bench report | not re-run since | |
| Aeotec Z-Stick S2 (`Aeotec_ZStickS2`) | 2026-11-04 | Hardware tests over `tcp://overwatch.local:4000` | 3 passed, relay toggled | serial path last 2026-10-26 |
| Radex One (`Radex_One`) | 2026-11-04 | Hardware tests, COM8 | 3 passed | |
| Zoom H4n (`Zoom_H4n`) | 2026-11-04 | Hardware tests, COM3 | 1 passed | |
| NMEA GPS | 2026-11-04 | Hardware tests | 1 passed | |
| Kuando Busylight (`Kuando_Busylight`) | 2026-11-04 | Hardware tests | 3 passed (after the HID read fix) | |

The "last in a bench report" dates come from a text search of this folder (a device named in a report), not from a verified pass; open the report before relying on one. Saint George and Favaro scoring machines are verified by tests only (owner: no access to that code).
