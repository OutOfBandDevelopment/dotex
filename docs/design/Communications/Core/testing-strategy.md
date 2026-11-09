# Communications Core - Testing Strategy

**Epic:** 2 - Communications Platform
**Feature:** Core (facade, central processor, composition, preferences, deferral)
**Last Updated:** 2026-11-05
**Coverage target:** 85% line coverage (requirements NFR-4 asks for 80% at least)

Tiers follow the repository testing rules: `Unit`, `Simulate`, `Integration`, `LiveIntegration`. Method names are `Method_Scenario_Expected`; time comes from a fake `TimeProvider`; ids and correlation ids from a fake generator so output is deterministic.

---

## Unit tests (`OoBDev.Communications.Tests`, tier `Unit`)

Each functional requirement of [requirements.md](requirements.md) maps to named tests.

### FR-1 Send request (`CommunicationProvider`)
- `SendAsync_ValidRequest_ReturnsCorrelationIdAndHandsOffToProcessor`
- `SendAsync_UnknownTargetPerson_ThrowsArgumentException`
- `SendAsync_EmptyMessageType_ThrowsArgumentException`
- `SendAsync_NullData_UsesEmptyDataAndContinues`
- `SendAsync_NoPreferences_LogsWarningAndReturnsCorrelationId` (no delivery)
- `SendAsync_CustomHeaders_ReachTheProcessor`

### FR-2 Channel routing (`CommunicationCentralProcessor`)
- `HandleRequestAsync_SingleChannelPreference_SendsOnThatChannelOnly`
- `HandleRequestAsync_TwoChannels_SendsBothInParallel` (a barrier proves both composers run before either finishes)
- `HandleRequestAsync_ComposerMissingForChannel_SkipsWithWarningOthersSucceed`
- `HandleRequestAsync_OneChannelThrows_OtherChannelStillDelivers`

### FR-3 Data enhancement (`DataEnhancementManager`)
- providers run in declared order, each sees the previous result;
- a provider that fails is logged and skipped when marked optional, aborts the request when marked required;
- a provider is only called for the message types it declares;
- the enhanced data does not mutate the caller's `IMessageData` (the request object stays unchanged).

### FR-4 Composition
- template found by message type, culture and channel; culture falls back (`fr-CA` -> `fr` -> default) with a warning;
- missing template: error with the template key in the message;
- `NullMessageComposer` produces no side effect and is the default when no composer is registered;
- composed email carries text and HTML parts, composed SMS stays within the segment limit or fails with a clear error.

### FR-5 Deferral (`DeferralManager`)
- `DeferAsync_FutureTime_StoresRequestAndDoesNotSend`
- `Release_DueRequests_AreHandledOnceInDueOrder`
- poison request (fails to deserialize) is moved aside and does not block the queue;
- retry of a transient failure re-queues with a later due time up to the configured limit.

### FR-6 Priority
- low-priority requests are dropped or held according to the preference's minimum priority; the filter is logged;
- `Critical` ignores quiet hours and the minimum priority.

### FR-7 Correlation
- one correlation id per request across all channels and log entries (log capture);
- the same caller-supplied id twice delivers once (idempotency).

### FR-8 Errors
- provider exceptions are logged with the correlation id and do not escape `SendAsync` after hand-off;
- configuration errors (no `ITargetPreferenceManager`) fail at start-up validation, not at the first send.

### Registration and options
- `TryAddCommunications` registers every service once; a pre-registered custom service wins (`TryAdd*`);
- options bind from `OoBDev:Communications` and fail validation for bad values.

---

## Simulate tests (tier `Simulate`)

Full stack with in-memory preference store, template provider, deferral store and a recording channel provider:

1. Request with two preferred channels -> two recorded messages with the same correlation id and rendered template content.
2. Enhancement adds data from a fake source and the template shows it.
3. Deferred request is released by advancing the fake clock and delivers exactly once.
4. A provider outage on one channel while the other succeeds: the circuit breaker opens after the configured failures and half-opens after the interval (NFR-2).
5. 1 000 concurrent requests: no lost or duplicated message, and the results are checked for the throughput target on the in-memory stack (100 requests per second, under 500 ms processing at the 95th percentile, NFR-1).

## Integration tests (tier `Integration`, Docker)

| Scenario | Services |
|----------|----------|
| Email delivery through SMTP and read back | smtp4dev |
| Deferral store and preferences persisted in SQL Server; release after a host restart | SQL Server |
| Deferred queue over a broker, one delivery after a consumer restart | RabbitMQ |
| Templates loaded from the database source (Epic 10) | SQL Server |

Cloud vendors are `LiveIntegration` (manual, throwaway accounts).

## Contract tests

A shared abstract test class (`ChannelProviderContractTests`) runs against every channel provider and composer: required message fields, cancellation, null handling and thread safety. A new provider inherits it and is not complete until it passes.

## Observability checks

- every log entry in the send path carries the correlation id (log capture over the Simulate scenarios);
- spans `communications.send` and `communications.channel` exist and nest; no message content in tags.

## Coverage and CI

`dotnet test src/ --filter "TestCategory=Unit|TestCategory=Simulate" --collect:"XPlat Code Coverage"` shows at least 85% for the library. The Integration tier runs locally against `containers/testing`.

## Related Documents

- [Requirements](requirements.md)
- [Architecture](architecture.md)
- [API Design](api-design.md)
- [Send & Receive testing strategy](../SendReceive/testing-strategy.md)
