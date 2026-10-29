# DevTerm session servers and clients

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- `OoBDev.DevTerm.Core` gains the observers that publish a session to other processes (`SessionPipeServer`, `SessionControlPipeServer`, `SessionTcpShareServer`, `SessionHttpControlServer`), the clients (`SessionPipeClient`, `SessionControlClient`) and `SessionCommands`.
- 13 ported tests; `OoBDev.DevTerm.Core.Tests` 223 passing, 5 inconclusive (Java/Go not installed).
- Not done: the reverse adapter, logging and configuration, wrapping serial/TCP/HID adapters as transports.
