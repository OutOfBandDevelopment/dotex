# Feature request: DevTerm out-of-process plugins and read-write control channel

- **Status:** done (process plugins with approval, read-write control channel, sandboxed launch and the Blazor `/plugins` panel with a Playwright journey, built by 2026-11-23)
- **Requested by:** owner (retire dev-term, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Applications/DevTerm` (plugins), see the PluginSandbox design

## Need

Plugins load in process. A read-write control channel over a named pipe and a localhost web service, and plugins that run out of process, were decided on 2026-10-07. Transport and device-module plugin variants are still design work.

## Proposed behavior

- A control channel (named pipe and localhost web service) for reading and driving a running DevTerm.
- Out-of-process plugins, installed from a `.zip` with approval at install (the installer exists; check the approval step).

## Source (read-only, to be retired)

dev-term `docs/design/proposals/cross-process-control-channel.md` and `out-of-process-plugins.md`; dotex: [PluginSandbox](../../design/PluginSandbox/README.md) and [Plugins](../../design/Plugins/README.md).

## Acceptance criteria

- Given a plugin process, then a crash does not stop DevTerm.
- Given a control-channel client without the token, then the request is refused.
- Given a plugin package, then install asks for approval first.

## Existing code checked

`PluginInstaller`, `PluginLoader`; the PluginSandbox design and spike result. No named-pipe control server found.

## Security and operations

A sandbox and threat model exist ([threat model](../../design/PluginSandbox/threat-model.md)). Channels bind loopback and require a token.

## Priority

- **Impact:** safe third-party plugins; automation.
- **Needed by:** no date
- **Workaround today:** in-process plugins

## Open questions

**Answered (checked 2026-11-23):** the PluginSandbox design and requirements are done and the launchers built; the remaining follow-up is owner question Q31.

The PluginSandbox layering question and sign-off of its requirements are already open; this request waits on them.

## Triage notes

Origin: dev-term BACKLOG (Tooling; Proposals tracker).

2026-11-11: triaged. Out-of-process plugins are the PluginSandbox feature (threat model, protocol and manifest design, launchers built; follow-ups in BACKLOG). The read-write control channel is part of that item. Design: [PluginSandbox](../../design/PluginSandbox/README.md), [SessionChannel](../../design/SessionChannel/README.md). Backlog: BACKLOG.md, security.
