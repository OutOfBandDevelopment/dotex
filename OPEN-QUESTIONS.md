# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 · 2026-11-04 · Ser2NetWin migration layering

Where does Ser2NetWin go, and may it bring `Makaretu.Dns.Multicast.New`, `Spectre.Console` and `Microsoft.Extensions.Hosting.WindowsServices` into the central packages? Plan: [docs/design/Ser2NetWin/migration-plan.md](docs/design/Ser2NetWin/migration-plan.md).

**Options**
- A (recommended): bridge core as Framework library `OoBDev.System.IO.Ports.Bridge`, CLI as application `src/Applications/Ser2NetWin`; add the three packages. Reusable code stays reusable (rule `layering-and-migration-plans`); the cost is three new third-party dependencies, two of them for the CLI only. Spectre.Console is not on the rejected list.
- B: everything under `src/Applications/Ser2NetWin` (two projects, as in the source). Simplest move, but the RFC 2217 and bridge code cannot be reused by DevTerm tests without an application reference, which the layering forbids.
- C: A, but the mDNS advertiser goes behind an interface in the core with the package only in a separate `OoBDev.Net.Mdns` project, so the core keeps to Abstractions. More projects, cleanest layering.

**Default:** A, not started; the plan waits for the answer.

**Your answer:**

### Q2 · 2026-11-04 · Example panels per device family and control-example apps

You decided each device family gets an explicit Example panel and each control set (Blazor, WPF, Tui) gets a small control-example app. The design does not yet say where these live or what a device-family Example panel is. Detail: [ui-language-later.md](docs/design/DevTermParity/ui-language-later.md), [UiDefinitions](docs/design/UiDefinitions/README.md).

**Options**
- A (recommended): one `UiDefinition` sample per device family inside each device project's tests/samples folder (panel definition plus a fake device, no app), and one control-example app per host in `src/Examples/OoBDev.Examples.Controls.<Host>` showing every control with canned values. Smallest, keeps device projects free of UI references (layering), and the generated guides can screenshot the control apps.
- B: an Example panel project per device family under `src/HardwareDevices`, referencing `OoBDev.UiDefinitions`. Closer to "explicit panel per device", but device libraries gain a UI-definition dependency and about ten new projects.
- C: only the DevTerm reference panels now; control-example apps later. Least work; leaves the decision half done.

**Default:** none started; the item waits for the answer (BACKLOG section 1).

**Your answer:**
