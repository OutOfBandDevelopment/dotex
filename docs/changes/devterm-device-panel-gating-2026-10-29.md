# DevTerm Core drops the K8055 and Busylight gating (2026-10-29)

`DevicePanels.IsAvailable` no longer knows the K8055 and Busylight vendor and product ids, and `DevicePanel` no longer has `K8055` and `Busylight` members; `BuiltInPanelIds` (unused) is removed. Core must not know device libraries (owner, 2026-10-29): each panel's `IsAvailable` in `OoBDev.Velleman.K8055` and `OoBDev.Kuando.Busylight` already carries the gating and has its own tests. The gating tests in `MainWindowStateTests` were removed with it.

Verified: `MainWindowStateTests` 13 passed; the Tui.Tests and Wpf projects build.
