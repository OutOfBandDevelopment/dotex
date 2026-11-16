# Session channel

Sharing a live session across processes over a named pipe. Moved from the DevTerm design folder on 2026-11-04. Application: [DevTerm](../../applications/devterm/README.md).

**Status:** Built: `SessionPipeServer`, `SessionPipeClient` and the control-channel pair in `OoBDev.DevTerm.Core`; the read-write control channel for out-of-process plugins is still open (BACKLOG, DevTerm). (checked against the code 2026-11-16)

- [cross-process-control-channel.md](cross-process-control-channel.md) - pipe server and client, attach mode and status
