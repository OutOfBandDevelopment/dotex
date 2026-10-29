# Service Bus emulator coverage (2026-10-29)

The Service Bus Integration tests could not reach the emulator: the connection string lacked `UseDevelopmentEmulator=true` (the SDK then dials the TLS port 5671 instead of 5672). Fixed in `src/.runsettings` and `CONFIGURATION_SETTINGS.md`.

Added: a session queue (`integration-test-session-queue`) in `containers/testing/servicebus-config/Config.json`; the topic test now receives from `test-subscription`; the session test runs on the emulator (it no longer skips on `localhost`) and checks session id and correlation id on receipt. Variables are listed in `TEST_VARIABLES.md`.

Verified against the running emulator: 3 passed, 2 skipped (the management API tests, which need real Azure). Still open: a Service Bus health check in the test stack. The emulator stays down if its SQL Server is recreated after it started; restarting the emulator container fixes that.
