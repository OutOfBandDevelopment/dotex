# Send & Receive - Testing Strategy

**Epic:** 2 - Communications Platform
**Feature:** Send & Receive
**Last Updated:** 2026-11-05
**Coverage target:** 85% line coverage for `OoBDev.Communications` (Framework layer minimum is 80%)

Tiers follow the repository testing rules: `Unit`, `Simulate`, `Integration`, `LiveIntegration`. Method names are `Method_Scenario_Expected` with Stage / Mock / Test / Assert / Verify comments. Time comes from a fake `TimeProvider`, so retry delays cost no real time.

---

## Unit tests (`OoBDev.Communications.Tests`, tier `Unit`)

### SendService
| Test | Checks |
|------|--------|
| `SendAsync_UnknownChannel_ThrowsChannelNotFound` | configuration error is an exception |
| `SendAsync_UnknownProvider_ThrowsProviderNotFound` | same |
| `SendAsync_ProviderCannotSend_ReturnsRejected` | no provider call |
| `SendAsync_Success_RecordsSentAndReturnsIds` | tracker called once with the result |
| `SendAsync_TransientThenSuccess_RetriesWithBackoff` | delays 30 s, 60 s on the fake clock |
| `SendAsync_PermanentFailure_DoesNotRetry` | one provider call |
| `SendAsync_RateLimitedWithRetryAfter_WaitsRetryAfter` | the longer of backoff and `RetryAfter` |
| `SendAsync_RetriesExhausted_ReturnsLastFailure` | `Success = false`, attempts = count + 1 |
| `SendAsync_BackoffCapped_NeverExceedsMaxRetryDelay` | cap holds for large attempt numbers |
| `SendAsync_Cancelled_StopsAndThrows` | no further attempts |
| `SendAsync_RepeatedMessageId_ReturnsStoredResult` | idempotency, no second provider call |
| `SendAsync_OptionsOverrideConfiguredRetry` | per-call `RetryCount` wins |
| `SendToMultipleChannelsAsync_OneFails_OthersSucceedInOrder` | results ordered as input |
| `SendToMultipleChannelsAsync_RespectsMaxParallelSends` | concurrency probe never above the limit |
| `SendToUserAsync_NoPreference_ReturnsRejected` | |
| `SendAsync_ScheduledTime_QueuesAndReturnsWithoutProviderCall` | deferral manager called |

### ReceiveService
| Test | Checks |
|------|--------|
| `HandleWebhookAsync_UnknownChannel_ReturnsFailureWithoutThrowing` | |
| `HandleWebhookAsync_InvalidSignature_RunsNoHandler` | security |
| `HandleWebhookAsync_StaleTimestamp_Rejected` | clock tolerance |
| `HandleWebhookAsync_MessageAndStatusEvents_DispatchedToHandlers` | counts and ids in the result |
| `HandleWebhookAsync_HandlerThrows_OthersStillRun` | failure isolation |
| `HandleWebhookAsync_HandlersRunInRegistrationOrder` | |
| `PollAsync_MarksReadOnlyAfterHandlers` | no loss when a handler throws |
| `PollAsync_RespectsMaxMessages` | |

### DeliveryTracker (in-memory implementation)
| Test | Checks |
|------|--------|
| `UpdateStatusAsync_ForwardTransition_SetsTimestamps` | `DeliveredAt`, `FailedAt` from the fake clock |
| `UpdateStatusAsync_StaleEvent_Ignored` | `Delivered` is not overwritten by `Sent` |
| `UpdateStatusAsync_UnknownMessage_DoesNotCreateRecord` | |
| `GetByCorrelationIdAsync_ReturnsOrderedByQueuedAt` | |
| `UpdateStatusAsync_ConcurrentUpdates_LastValidTransitionWins` | 100 parallel updates, no exception, final state valid |

### Options and registration
- `SendReceiveOptions` validation: negative retry count, zero delay and factor below 1 each fail at start-up.
- `TryAddSendReceive_CalledTwice_RegistersOnce`; a registered custom tracker is not replaced.
- Webhook processors resolve by provider key; an unknown key returns null.

---

## Provider adapter tests (per vendor package, tier `Unit`)

Each webhook processor has fixture payloads checked into the test project (recorded from the vendor documentation, with fake addresses and ids): valid signature, tampered body, missing header, every event type the vendor sends mapped to `DeliveryStatus`. The sender is tested against `HttpMessageHandler` fakes for 2xx, 4xx, 429 with `Retry-After` and 5xx.

---

## Simulate tests (`OoBDev.Communications.Tests`, tier `Simulate`)

Full stack with the in-memory channel provider, tracker and a fake clock:

1. Send email, receive a `Delivered` webhook, the status record shows `Queued -> Sent -> Delivered`.
2. Send to two channels with one correlation id; `GetByCorrelationIdAsync` returns both in order.
3. A flaky provider (two transient failures) delivers on the third attempt; the tracker holds one record.
4. A bounce webhook followed by a late `Sent` event leaves the record at `Bounced`.
5. 1 000 concurrent webhook calls produce 1 000 tracker updates and no lost handler call.

## Integration tests (tier `Integration`, Docker)

| Scenario | Services |
|----------|----------|
| Email send reaches an SMTP server and the message is readable through its API | smtp4dev |
| Delivery records persist and survive a restart of the host | SQL Server |
| A webhook posted to the host is verified, dispatched and visible through the status endpoint | host in-process, SQL Server |
| Queued sends survive a broker restart and are delivered once | RabbitMQ |

SMS, push, Slack and Teams have no local emulator; their vendor calls are covered by the adapter unit tests, and a `LiveIntegration` test per vendor (manual, throwaway account, never production data) proves one real send and one real webhook.

## Performance tests (tier `Simulate`, marked `Performance`)

| Gate | Target |
|------|--------|
| Send overhead excluding the provider | under 200 ms at the 95th percentile (requirements TR-7) |
| Webhook processing | under 100 ms at the 95th percentile |
| 100 parallel sends | complete without exceeding `MaxParallelSends` |
| 1 000 concurrent webhooks | no failures, no lost events |

A request over the target is logged and a request over 150 % of it fails the test.

## Security tests

- A webhook with a missing, malformed or wrong signature never reaches a handler (every vendor fixture).
- A replayed webhook older than the tolerance is rejected.
- Message content never appears in logs or span tags (log capture assertion).
- The webhook route accepts anonymous calls but no other route does (host test).

## Coverage and CI

`dotnet test src/ --filter "TestCategory=Unit|TestCategory=Simulate" --collect:"XPlat Code Coverage"` must show at least 85% for the library; the Integration tier runs locally against `containers/testing`. A new vendor adapter is not complete without its fixture set.

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Channel Abstraction testing strategy](../ChannelAbstraction/testing-strategy.md)
