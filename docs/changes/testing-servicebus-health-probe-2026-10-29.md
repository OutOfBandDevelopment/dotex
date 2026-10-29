# Service Bus emulator readiness probe (2026-10-29)

The Service Bus emulator image has no shell, so Docker cannot run a health check in it and `wait-for-services` counted it ready as soon as the container was running, before AMQP accepted connections.

`containers/testing/scripts/wait-for-services.sh` and `.bat` now also open a TCP connection to the AMQP host port (`TEST_PORT_SERVICEBUS`, default 5672) and report the container as starting until that succeeds.

Verified once with `integration-up.bat --wait`: the probe passed and the port was open. That start also hit the script's 120 s timeout on another service (14 of 15 healthy at the time, probably a slow model container) before the stack settled; which service was not identified.
