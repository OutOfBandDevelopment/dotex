# AsyncAPI and message resolver unit tests

**Date:** 2026-10-22

## Done

- `OoBDev.AsyncApi.Tests` (new, 9 Unit tests): the document builder with a mocked `IMessagePropertyResolver`.
- `SqsAsyncApiContributorTests` (5), `ServiceBusAsyncApiContributorTests` (3), `RabbitMQAsyncApiContributorTests` (3): server, address, bindings and a secrets check per contributor.
- `MessagePropertyResolverTests` (10 Unit tests, `OoBDev.MessageQueueing.Tests`): section order (`{channel}:{message}`, `{message}`, `{channel}`, `Default`), `[MessageQueue]` name override, `object` channel, missing configuration, message ids.
- `OoBDev.Microsoft.Azure.ServiceBus.Tests` compiles again (missing `OoBDev.MessageQueueing` reference and an outdated setup in the Integration tests; not run, they need the emulator).

## Bug fixed

`MessagePropertyResolver.ConfigurationSafe` never fell back to the queue section when it had no `Config` child, because `GetSection` never returns null. Flat queue configuration therefore resolved to an empty section. It now falls back when `Config` has no children.

## Verification

All four affected Unit projects pass. Docs tests run on `docs/design/AsyncApi`.
