# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-10-29, DevTerm panels): where do the panel contracts move so device libraries can own their panels?

Owner answers: K8055 and Busylight use the generic `UiDefinition` and live with their protocols, and device-dependent configuration and options (settings models, config editors, option lists) do the same. `IDeviceConfigEditor` and `DeviceConfigTarget` then also need a host-free home (same option as the panel contracts). That needs `IControlSurface`, `ICommandPreview` and `IStructuredPresenter` (now in `OoBDev.DevTerm.Core`) in a place the device libraries can reference. The surfaces also stop taking a DevTerm `Session` and send through a `Func<ReadOnlyMemory<byte>, CancellationToken, Task>`.

| Item | Now | Target |
|------|-----|--------|
| `IControlSurface`, `ICommandPreview`, `CommandPreviewFormat` | `OoBDev.DevTerm.Core` | Option A: `OoBDev.UiDefinitions`, namespace `OoBDev.UiDefinitions` |
| `IStructuredPresenter` | `OoBDev.DevTerm.Core` | same project; the K8055 and Busylight decoders implement it |
| `K8055UiDefinition`, `K8055ControlSurface` | not ported (dev-term `DevTerm.Devices.K8055`) | `OoBDev.Velleman.K8055` |
| `BusylightUiDefinition`, `BusylightControlSurface` | not ported | `OoBDev.Kuando.Busylight` |
| `IDevicePanelContribution` and a generic vendor and product matcher | `OoBDev.DevTerm.Core` | stays; the device libraries add no DevTerm reference |

**Options**
- A (recommended): move the three contracts into `OoBDev.UiDefinitions` (host-free Framework, already referenced by the device libraries). No new project; the panel renderers already depend on it. Your rule is that reusable means Framework.
- B: a new Framework library `OoBDev.Devices.Control` for the contracts. Cleaner name, one more project pair.
- C: leave the contracts in DevTerm Core and put the panels there too (the earlier default). Fewer moves, but Core references every device library and the protocols are split from their panels.

**Default:** A.

**Your answer:**
