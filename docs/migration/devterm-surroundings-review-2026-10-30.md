# dev-term surroundings review (2026-10-30)

Owner rule (2026-10-30): when a project migrates, everything around the code is reviewed too. Source: `C:\repo\oobdev\dev-term` at `8c66e74`. Each item is migrated, merged into an existing item, or rejected with the reason. The code itself is tracked in the [parity status](../design/DevTermParity/status.md).

## Contents

- [Decisions](#decisions)
- [Future plans carried into the backlog](#future-plans-carried-into-the-backlog)
- [Left as they were](#left-as-they-were)

## Decisions

**Table 1 — Surroundings by item**

| dev-term item | Decision | Where it is now |
|---------------|----------|-----------------|
| `docs/design`, `specs`, `protocols`, `devices`, `user-guide`, `test`, `bugs`, `changes`, `coding-standards.md` (423 files) | Migrated unchanged so relative links hold; the content is source material that the per-feature documents here cite, not the current truth until reconciled | `docs/applications/devterm/` (copy); device manuals and `known-configuration.md` also live in `src/Extensions/OoBDev.Scpi/docs/` |
| `CLAUDE.md` (project guide, 636 lines incl. about 70 hard-won constraints for serial, HID, Terminal.Gui, WPF, test automation) | Migrated as a snapshot; its constraints stay valid for the ported code | `docs/applications/devterm/source/project-guide.md`; the rule `coding-standards` points there |
| `TODO.md`, `BACKLOG.md`, `OPEN_QUESTIONS.md`, `README.md` | Snapshots kept; open work carried into `BACKLOG.md` (next section); the open questions are all hardware or capture questions and are listed in the backlog item for each | `docs/applications/devterm/source/` |
| Skill `hardware-test` | Migrated, paths and commands adapted | `.claude/skills/hardware-test` |
| Skill `device-manual` | Migrated, output into the `OoBDev.Scpi` docs | `.claude/skills/device-manual` |
| Skill `docs-sync` | Merged: the rule "a user-facing screen or flow needs a spec and a user guide entry with real captured screenshots" is a line in the `work-tracking` rule; the rest is covered by `complete-work` and `reconcile-designs-and-code` | `.claude/rules/work-tracking.md` |
| Skill `work-docs-audit` | Rejected as a separate skill: the `tracking-hygiene` hook and the close-out audit of `work-backlog` cover the sweep | n/a |
| Skill `bug-report` | Rejected: this repository tracks findings in `BACKLOG.md` and fixes in change documents (rule `work-tracking`); the 72 closed reports are kept as history | `docs/applications/devterm/bugs/` |
| Hook `merge-gate.sh` (reminder before merge or push) | Rejected: the `work-tracking` hook reminds at commit time, which is earlier | snapshot in `source/` |
| `settings.json` attribution off | Already covered by `guard-git.mjs` | n/a |
| `devterm.runsettings` | Replaced by the Hardware section of `src/.runsettings` ([test hardware](../how-tos/test-hardware.md)); the old names survive in the Tui DevLocal tests until those move (backlog) | snapshot in `source/` |
| `tests/DevTerm.CodingStandards.Tests` (`DeviceDocsStandardsTests`, `TestCategoryStandardsTests`) | Kept as reference; port as tests (backlog) | snapshot in `source/` |
| `containers/` (mosquitto, rabbitmq with TLS, ser2net, loopback-device) | Migrated as is; wiring into the integration stack is a backlog item | `containers/devterm/` |
| `scripts/` (`image_diff.py`, `make_loopback_gif.py`, `radexone_probe.py`, `New-ZadigPreset.ps1`) | Migrated as is, for the owner's use; no Python for agent work | `scripts/devterm/` |
| `schemas/`, `examples/` | Already present | `schemas/devterm`, `examples/devterm` |
| `spikes/ScpiUsbtmcSpike` | Nothing tracked (only `bin` and `obj`) | n/a |
| `scratchpad/` (three TDS2024 hardcopy images) | Kept as bench evidence | `docs/applications/devterm/scratchpad/` |
| dev-term's own "consume dotex as NuGet packages" plan (BACKLOG note 2026-10-22) | Superseded: dev-term is merged into this repository | n/a |

## Future plans carried into the backlog

BLE backends for Linux and macOS, UDP transport (target and listener), rendering presenters (HP-GL, PostScript, PCL) and SVG in the Tui, the nine UX decisions of 2026-10-13, network device discovery and config editors (EByte E810-DTU, USR-TCP232-302), out-of-process plugins and the control channel (see the [plugin sandbox](../design/PluginSandbox/threat-model.md)), the Z-Wave management bytes check, and the research items (BYTECC BT-UP01, dockable MDI layout) are one backlog item each in `BACKLOG.md` section 1, linking the snapshot for detail. `DevTerm.Analyzers` is merged into the existing Roslyn analyzers backlog.

## Left as they were

Open hardware questions (DE-5000, EByte captures, Kuando, K8055, Radex, Zoom H4n, Tektronix 2230 remote setting) stay as questions about hardware, not for the owner; they wait for bench sessions.
