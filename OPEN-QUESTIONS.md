# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q33 — 2026-11-23 — Bench-hardware items: who runs them?

**Question:** These need a person at the bench or a captured trace, which I cannot supply: DE-5000 BLE Hardware test (radio not fitted), Z-Wave include/exclude/reset pass, the silent HP 34401A and TDS2024 TCP bridges, the Ebyte and USR UDP config capture (Ebyte `IDeviceConfigEditor`), HP-GL scope and XBee bench checks, and the Ser2NetWin bench pass with any COM-port device. When you are at the bench, do you want me to run each pass through the `hardware-test` skill, or to leave them off the backlog?

**Options:**
- A (recommended): keep them listed; run them when you say the bench is ready.
- B: drop the ones with no hardware in sight and re-add on demand.

**Default:** A.

**Your answer:**



### Q41 — 2026-11-25 — Remote USB devices for DevTerm: bridge client or the remote DevTerm web host?

**Question:** DevTerm on one machine needs raw HID and USBTMC devices that sit on another (Windows) machine. Today the built `OoBDev.System.IO.Usb.Bridge` carries them as a framed TCP stream with no authentication; DevTerm has no connection option for it yet. The owner (2026-11-25) leans to running the DevTerm web host on the remote machine and adding a client for it to the Tui and WPF apps. Facts: the web host already has a WebSocket tunnel, session and panel APIs, and a shared bearer token (`Web:Token`, optional read-only token, non-loopback bind needs an explicit token); it has no JWT, which the owner decisions require for services. The Greaseweazle is out of scope.

**Options:**
- A (recommended): DevTerm web host as the remote side. One authenticated surface for serial, HID, USBTMC and the other transports; the Tui and WPF clients get a "remote DevTerm" connection. Cost: a remote client for the session API, JWT support on the host (today a shared token), and the bridge stays for non-DevTerm clients only.
- B: add `usbtmc-remote` and `hid-remote` transports to DevTerm that speak the existing bridge protocol. Smallest change, no authentication until the bridge gets some, covers only these two transports.
- C: both; A for DevTerm, B kept as the lightweight route for other dotex code.

**Default:** A in design only; no code until answered. The bridge stays as built.

**Your answer:**
