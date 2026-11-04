# DevTerm CLI test against the loopback-device container (2026-11-04)

`LoopbackDeviceCliIntegrationTests` (`OoBDev.DevTerm.Tui.Tests`, category `Integration`) runs the real command-line mode over the TCP transport against the stack's `loopback-device` (`hello`, `STATUS?`, an unknown command). Endpoint from `LOOPBACK_DEVICE_HOST` and `LOOPBACK_DEVICE_PORT` in `src/.runsettings`; Inconclusive when the container is unreachable.

Verified: 3 passed against the running container. The serial Hardware tests are still open (they need the bench).
