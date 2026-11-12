# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open



### Q-CLOUDEVENTS-EDGE — 2026-11-12 — CqrsEventSourcing (layering)

Where does the CloudEvents `POST /events` edge live? Plan: [cloudevents-edge-plan.md](docs/design/CqrsEventSourcing/cloudevents-edge-plan.md).

**Options**
- A (recommended): new `OoBDev.EventSourcing.Web` in `src/Extensions`, beside the other ASP.NET libraries; keeps Framework host-free.
- B: new project in `src/Framework` with an ASP.NET `FrameworkReference`; breaks the Framework-has-no-host rule.
- C: add the endpoint to an existing web library; mixes unrelated concerns.

**Default:** A (building nothing until confirmed).

**Your answer:**


### Q-ZWAVE-ENUMERATE — 2026-11-12 — Z-Wave panel (contract)

Enumerate and Describe node must read replies (the init-data node list, then each node's answers), but `IControlSurface` only sends and `IDeviceDecoder` only decodes, and the host creates them separately. How does a command see replies?

**Options**
- A (recommended): the panel shares one per-session `ZWaveNodeTable` between `CreateSurface` and `CreateDecoder` (the host calls both for one session); Enumerate waits on the table with a timeout. No contract change, but the panel instance becomes stateful.
- B: extend `IDevicePanel` with a session-scoped `CreateSession(DeviceSend)` returning the surface and decoder together; cleaner, changes the shared UiDefinitions contract and the three front ends.
- C: Enumerate sends GetNodeProtocolInfo for a typed node range (default 1-10) without reading replies; no state, but the user must know the range.

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-BASICSTAMP-FORMAT — 2026-11-12 — Basic Stamp tokenizer (scope and source)

The request asks for a Basic Stamp tokenizer and detokenizer but the repo holds no format reference, and Parallax does not publish the BS2 EEPROM token format (it is reverse-engineered in third-party tools such as the open-source `pbasic` and `stampy` projects). Which generation, and where does the format come from?

**Options**
- A (recommended): BS1 only first, from the published BS1 token tables in the Parallax manual; small, documented, testable with hand-built bytes. BS2 waits for a reference.
- B: BS2 family from a reference project you point to (add it to `docs/harvest-candidates.md`); larger, format is undocumented and needs real EEPROM dumps to verify.
- C: drop the item (no consumer, no hardware on the bench).

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**


### Q-MIDI-PANEL — 2026-11-12 — MIDI device panel for DevTerm (layering)

`OoBDev.Midi` (Extensions) is pure and references nothing. A DevTerm device panel (`IDevicePanel`, decoder, control surface, UI definition) needs `OoBDev.System.Abstractions` (`UiDefinitions`), as the ZWave and device projects do. MIDI runs on a plain serial line at 31250 baud (or USB-MIDI), so no new transport is needed. Where does the panel live?

**Options**
- A (recommended): a `Panels` folder in `OoBDev.Midi` with a reference to `OoBDev.System.Abstractions`, like `OoBDev.ZWave`; one project, same pattern as the other panels; the pure parser now carries a UiDefinitions reference.
- B: a separate `OoBDev.Midi.Panels` project; keeps `OoBDev.Midi` reference-free, one more project and package.
- C: no MIDI panel until there is a MIDI device on the bench (the request lists no consumer).

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**
