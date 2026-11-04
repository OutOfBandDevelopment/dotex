# DevTerm containers wired into the integration stack (2026-11-04)

dev-term's disposable peers moved from `containers/devterm` into `containers/testing` (`devterm/` subfolder).

- **Compose:** new `mosquitto` (1883), `ser2net` (2217) and `loopback-device` (7778) services; `rabbitmq` also publishes STOMP 21613, AMQPS 5671 and STOMP TLS 21614 with the DevTerm config and a generated test CA (`devterm/make-test-certs.sh`, output git-ignored).
- **Scripts:** `integration-up` and `wait-for-services` (sh and bat) start and wait for the new services.
- **Settings:** `MOSQUITTO_*`, `SER2NET_*`, `LOOPBACK_DEVICE_*`, `RABBITMQ_STOMP*` in `src/.runsettings` and `.env.integration`; documented in `TEST_VARIABLES.md`, `containers/testing/README.md` and `docs/architecture/testing/integration/`.
- **Tests:** Integration tests read their endpoints through `TestContext` (not hard-coded) in `OoBDev.System.IO.{Mqtt,Brokers,Tcp}.Tests` and the DevTerm bridge tests; `OoBDev.DevTerm.Core.Tests` references `OoBDev.TestUtilities`.
- **Docs and messages:** `ConnectionErrorMessages` hints, the observability design and the connecting guide point at the new compose file (the dashboard is the stack's `otel-dashboard`); `containers/devterm` is deleted.

Verified: Integration filter against the running containers passed (Brokers 6, Mqtt 1, Tcp 1, DevTerm Core 5). Not run: the full stack up and down from scratch, and a loopback-device Integration test (none written).
