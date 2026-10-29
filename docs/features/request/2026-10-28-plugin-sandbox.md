# Feature request: Sandboxed plugin host

- **Status:** triaged
- **Requested by:** owner (filed from the notes repository, `projects/dotex-consolidation/plugin-sandbox-project/README.md`)
- **Date:** 2026-10-28
- **Area:** unknown (a new package; no existing dotex area was identified)

## Need

A .NET 10 host that runs plugins written in any language in a sandbox, with permissions and approval, brokered access to external resources, network isolation and resource
limits. The design exists in the notes repository (12 topic files, a decision log and use cases). A proof of concept exists in `proving-grounds`, `examples/plugin-sandbox-poc`
(AppContainer on Windows, bubblewrap and seccomp on Linux, signed packages, escape tests; last commit 2026-10-08, from the dotex harvest list). The reference code in the
notes was never compiled or tested.

## Proposed behavior

A host loads a signed plugin package, starts it in an isolated process with only the permissions the user approved, and talks to it over a defined protocol. A call from the
plugin to an external resource goes through the host's broker. The wire protocol, the Linux shim and the SDKs were never written, so the example below is only the intent:
install a plugin that asks for one network host, approve it, and watch a request to any other host fail.

## Acceptance criteria

- Given an approved plugin, when it calls an allowed resource through the broker, then the call succeeds.
- Given the same plugin, when it tries a resource it was not granted, then the call is refused and logged.
- Given a plugin that exceeds its CPU, memory or time limit, when it runs, then the host stops it and reports why.
- Given a package with a bad or missing signature, when loaded, then the host refuses it.
- Given the escape tests from the proof of concept, when run on Windows and Linux, then none escapes.

## Existing code checked

- `docs/harvest-candidates.md` lists the proving-grounds proof of concept; the recommendation plan (D7) puts it on the evaluate-later list in `BACKLOG.md`.
- The CGI middleware (`OoBDev.AspNetCore.Cgi`) runs child processes with a timeout, a concurrency cap and a process-tree kill, but its non-goals exclude sandboxing beyond OS user and resource limits, so it does not cover this.
- Not checked: an existing dotex package for plugins or process isolation. I did not search for one by code.

## Security and operations

The whole feature is a security boundary. Sections 10 to 13 of the design (permissions and approval, brokered access, network isolation, resource limits) are proposals for
review, not settled decisions. It starts processes, uses OS isolation features (AppContainer, job objects, bubblewrap, seccomp) and needs Windows and Linux test hosts. macOS
is unaddressed.

## Priority

- **Impact:** nothing is blocked.
- **Needed by:** no date.
- **Workaround today:** none that isolates; run plugins as normal processes under a low-privilege account.

## Open questions

- Is this a dotex package, or does it stay a separate project that dotex could adopt?
- Which of the design's open questions 6 to 15 (`docs/decision-log.md` in the notes) must be answered first?
- Is Windows plus Linux enough, or does macOS matter?

## Triage notes

2026-10-29: accepted as a phased design that begins with a threat model; no isolation code before it is reviewed. Home and platforms are owner question Q6 in `OPEN-QUESTIONS.md` (default: dotex package family, Windows and Linux). Design: [docs/design/PluginSandbox](../../design/PluginSandbox/README.md). Backlog: BACKLOG.md, security.

2026-10-29: phase 0 done: [threat model and PoC review](../../design/PluginSandbox/threat-model.md). Status stays `triaged` until the launchers ship.
