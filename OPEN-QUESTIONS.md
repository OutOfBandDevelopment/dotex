# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q33 — 2026-11-23 — Bench-hardware items: who runs them?

**Question:** These need a person at the bench or a captured trace, which I cannot supply: DE-5000 BLE Hardware test (radio not fitted), Z-Wave include/exclude/reset pass, the silent HP 34401A and TDS2024 TCP bridges, the Ebyte and USR UDP config capture (Ebyte `IDeviceConfigEditor`), HP-GL scope and XBee bench checks, the Greaseweazle USB pass for Ser2NetWin, and the BlueZ (Linux) BLE backend, which needs a Linux machine with a radio. When you are at the bench, do you want me to run each pass through the `hardware-test` skill, or to leave them off the backlog?

**Options:**
- A (recommended): keep them listed; run them when you say the bench is ready.
- B: drop the ones with no hardware in sight and re-add on demand.

**Default:** A.

**Your answer:**



### Q36 — 2026-11-23 — Oracle driver for the desired-state executor

**Question:** The Oracle `IScriptExecutor` (run the hand-written repeatable scripts, Q35 A) needs an Oracle ADO.NET driver, which is not in `Directory.Packages.props`. Which one, and may the test stack gain an Oracle container (`gvenzl/oracle-free`, about 2 GB image, slow start)?

**Options:**
- A (recommended): `Oracle.ManagedDataAccess.Core` (free, official, managed) in a new adapter project `OoBDev.Data.DesiredState.Oracle` under ExternalServices, plus an `oobd-test-oracle` container for Integration tests. Cost: a vendor package and a heavy container.
- B: no Oracle executor; the scripts run through the owner's own tools and the provider only plans. Cost: Q35 A is only half served.

**Default:** A is not started until you answer; the executors for Postgres, SQL Server and SQLite (drivers already present) are built meanwhile.

**Your answer:**
