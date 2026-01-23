# Requirements: Communications Core Orchestration

**Feature:** Communications Core
**Epic:** Communications Platform
**Last Updated:** 2026-01-22

---

## Functional Requirements

### FR-1: Send Message Request
**Priority:** MUST HAVE
**User Story:** As an application service, I want to send a message to a user so that they receive timely notifications.

**Acceptance Criteria:**
1. GIVEN a valid send request with TargetPersonId, MessageType, and Data
   WHEN I call `ICommunicationProvider.SendAsync(request)`
   THEN system returns a correlation ID for tracking
   AND system delivers message to user's preferred channels

2. GIVEN send request with invalid TargetPersonId
   WHEN I call SendAsync
   THEN system throws ArgumentException with clear error message

3. GIVEN send request with null or empty MessageType
   WHEN I call SendAsync
   THEN system throws ArgumentException

4. GIVEN send request with null Data
   WHEN I call SendAsync
   THEN system uses empty JObject and continues processing

5. GIVEN user has no configured channel preferences
   WHEN I call SendAsync
   THEN system logs warning and returns correlation ID (no delivery)

**Dependencies:**
- User preference lookup service
- At least one channel provider (Email or SMS)

---

### FR-2: Channel Routing
**Priority:** MUST HAVE
**User Story:** As a user, I want to receive messages on my preferred channels only.

**Acceptance Criteria:**
1. GIVEN user preferences specify ["Email"]
   WHEN system processes send request
   THEN message sent via Email channel only

2. GIVEN user preferences specify ["Email", "SMS"]
   WHEN system processes send request
   THEN message sent via BOTH Email and SMS channels in parallel

3. GIVEN user preferences specify ["Email", "SMS", "Push"]
   AND Push channel composer not registered
   WHEN system processes request
   THEN Email and SMS sent successfully
   AND system logs warning about missing Push composer

4. GIVEN user preferences specify [] (empty array)
   WHEN system processes request
   THEN no messages sent
   AND system logs info message

5. GIVEN channel composer throws exception
   WHEN system processes request
   THEN exception logged but doesn't affect other channels
   AND other channels complete successfully

**Dependencies:**
- ITargetPreferenceManager
- IMessageComposerFactory
- At least one IMessageComposer implementation

---

### FR-3: Data Enhancement
**Priority:** MUST HAVE
**User Story:** As a domain service, I want to inject contextual data into messages without the sender needing to load it.

**Acceptance Criteria:**
1. GIVEN OrderEnhancementProvider registered with [Communication(MessageType="order.confirmation")]
   AND send request with MessageType="order.confirmation"
   AND Data contains only OrderId
   WHEN system processes request
   THEN OrderEnhancementProvider.EnhanceAsync() called
   AND provider loads full order details
   AND enhanced data includes LineItems, Total, ShippingAddress

2. GIVEN multiple enhancement providers for same MessageType
   WHEN system processes request
   THEN ALL providers execute in sequence
   AND each provider receives output of previous provider

3. GIVEN enhancement provider throws exception
   WHEN system processes request
   THEN exception logged and bubbled up
   AND message NOT sent (fail-fast on enhancement errors)

4. GIVEN no enhancement providers for MessageType
   WHEN system processes request
   THEN system uses original Data as-is
   AND continues with message composition

5. GIVEN enhancement provider returns null
   WHEN system processes request
   THEN system throws InvalidOperationException

**Dependencies:**
- IDataEnhancementManager
- Attribute-based provider discovery

---

### FR-4: Message Composition
**Priority:** MUST HAVE
**User Story:** As a content manager, I want messages generated from templates so I can change content without code changes.

**Acceptance Criteria:**
1. GIVEN template exists for "order.confirmation" + "en-US"
   AND template contains "Hi {{FirstName}}, order #{{OrderId}}"
   AND enhanced data contains { FirstName: "John", OrderId: 12345 }
   WHEN Email composer generates message
   THEN email subject/body contains "Hi John, order #12345"

2. GIVEN template exists for MessageType but not for user's culture
   WHEN composer loads template
   THEN system falls back to default culture (en-US)

3. GIVEN no template exists for MessageType
   WHEN composer loads template
   THEN system throws TemplateNotFoundException

4. GIVEN template variable {{MissingField}} not in data
   WHEN composer generates message
   THEN variable replaced with empty string
   AND system logs warning

5. GIVEN Email template has both HTML and Text versions
   WHEN Email composer generates message
   THEN IEmailMessage contains BOTH HtmlContent and TextContent

**Dependencies:**
- ITemplateProvider
- IStringFormatter (variable substitution)
- Template storage (DB/files)

---

### FR-5: Deferred Delivery
**Priority:** SHOULD HAVE
**User Story:** As a marketing service, I want to schedule messages for future delivery.

**Acceptance Criteria:**
1. GIVEN send request with DeferUntil = tomorrow 9 AM
   WHEN I call `ICommunicationProvider.DeferAsync(request, deferUntil)`
   THEN system stores deferred request
   AND does NOT send immediately
   AND returns correlation ID

2. GIVEN deferred request with HoldUntil in past
   WHEN deferral processor runs
   THEN request processed immediately

3. GIVEN deferred request with HoldUntil = now + 1 hour
   WHEN deferral processor runs before HoldUntil
   THEN request NOT processed yet

4. GIVEN deferred request with HoldUntil = now + 1 hour
   WHEN deferral processor runs after HoldUntil
   THEN request processed through normal flow
   AND user preferences re-evaluated (may have changed)

5. GIVEN user in quiet hours (10 PM - 8 AM)
   WHEN send request arrives at 11 PM
   THEN system automatically defers until 8 AM
   AND logs deferral reason

**Dependencies:**
- IDeferralManager
- Deferral storage (queue/DB)
- Background processor for triggering deferred messages

---

### FR-6: Priority Handling
**Priority:** SHOULD HAVE
**User Story:** As a user, I want to receive only high-priority messages if I've opted out of normal notifications.

**Acceptance Criteria:**
1. GIVEN user preference MinimumPriority = High
   AND send request Priority = Normal
   WHEN system processes request
   THEN no messages sent
   AND system logs info about priority filter

2. GIVEN user preference MinimumPriority = High
   AND send request Priority = Critical
   WHEN system processes request
   THEN message sent successfully

3. GIVEN user preference MinimumPriority = Normal (default)
   AND send request Priority = Normal
   WHEN system processes request
   THEN message sent successfully

4. GIVEN user preference MinimumPriority = Critical
   AND send request Priority = High
   WHEN system processes request
   THEN no messages sent

5. GIVEN user has no MinimumPriority set (null)
   WHEN system processes request
   THEN all priorities allowed (default behavior)

**Dependencies:**
- ITargetPreference.MinimumPriority
- RequestPriorities enum

---

### FR-7: Correlation Tracking
**Priority:** SHOULD HAVE
**User Story:** As a support engineer, I want to track messages across channels using correlation IDs.

**Acceptance Criteria:**
1. GIVEN send request
   WHEN I call SendAsync
   THEN system generates unique Guid correlation ID
   AND returns correlation ID to caller

2. GIVEN send request routed to [Email, SMS]
   WHEN system sends to both channels
   THEN SAME correlation ID used for both Email and SMS
   AND correlation ID in IEmailMessage.RequestId
   AND correlation ID in ISmsMessage.RequestId

3. GIVEN correlation ID = X
   WHEN system logs processing steps
   THEN all log messages include correlation ID X
   AND logs filterable by correlation ID

4. GIVEN deferred request with correlation ID = X
   WHEN request reprocessed after delay
   THEN SAME correlation ID X used
   AND correlation chain preserved

5. GIVEN send request with custom headers
   WHEN system enhances data
   THEN headers included in seed data as "Request-Headers"
   AND headers accessible to enhancement providers

**Dependencies:**
- Guid generation
- Structured logging with correlation IDs

---

### FR-8: Error Handling
**Priority:** MUST HAVE
**User Story:** As a developer, I want clear errors when requests fail.

**Acceptance Criteria:**
1. GIVEN Email channel provider throws SendGrid API exception
   AND user also has SMS channel
   WHEN system processes request
   THEN Email failure logged but doesn't throw
   AND SMS still sent successfully
   AND correlation ID returned

2. GIVEN Data enhancement provider throws exception
   WHEN system processes request
   THEN exception bubbled up to caller
   AND message NOT sent (fail-fast on enhancement)

3. GIVEN Template not found for MessageType
   WHEN composer loads template
   THEN TemplateNotFoundException thrown
   AND exception includes MessageType and Culture

4. GIVEN user preference lookup service unavailable
   WHEN system processes request
   THEN exception bubbled up to caller
   AND caller can implement retry logic

5. GIVEN null/empty TargetPersonId
   WHEN caller calls SendAsync
   THEN ArgumentException thrown immediately

**Dependencies:**
- Comprehensive exception handling
- Structured logging

---

## Non-Functional Requirements

### NFR-1: Performance
**Priority:** MUST HAVE

**Requirements:**
1. **Throughput:** Support 100+ messages/second on standard hardware
2. **Latency:** Process send request in < 500ms (excluding provider API calls)
3. **Parallel Channels:** Send to multiple channels concurrently using Task.WhenAll
4. **Template Caching:** Cache loaded templates to avoid repeated file/DB access
5. **Async Throughout:** All I/O operations use async/await (no blocking calls)

**Measurement:**
- Load testing with 1,000 concurrent requests
- < 500ms average processing time
- > 100 req/sec sustained throughput

---

### NFR-2: Reliability
**Priority:** MUST HAVE

**Requirements:**
1. **Idempotency:** Sending same request multiple times produces one message (via correlation ID deduplication)
2. **Graceful Degradation:** If one channel fails, others succeed
3. **Retry Logic:** Deferral manager supports retry on transient failures
4. **Circuit Breaker:** Prevent cascading failures from provider outages
5. **Poison Message Handling:** Invalid deferred requests don't block queue

**Measurement:**
- 99.9% success rate under normal conditions
- Zero cascading failures in channel provider outages

---

### NFR-3: Observability
**Priority:** MUST HAVE

**Requirements:**
1. **Structured Logging:** All operations log with correlation ID
2. **Log Levels:**
   - INFO: Request received, channels processed, message sent
   - WARN: Template fallback, missing composer, priority filtered
   - ERROR: Enhancement failure, provider exception, template not found
3. **Metrics:** Track message volume per MessageType, channel, and priority
4. **Tracing:** Support distributed tracing (OpenTelemetry compatible)

**Measurement:**
- 100% of operations logged
- Correlation IDs in all log entries
- Log aggregation and filtering functional

---

### NFR-4: Testability
**Priority:** MUST HAVE

**Requirements:**
1. **80%+ Code Coverage:** All core components unit tested
2. **Mock Providers:** NullMessageComposer for testing without real providers
3. **Integration Tests:** Test with real providers (Docker for local, LiveIntegration for cloud)
4. **Simulation Tests:** Full flow with in-memory providers
5. **Test Categories:** Unit, Simulate, Integration, LiveIntegration

**Measurement:**
- dotnet test --collect:"XPlat Code Coverage" shows > 80%
- All test categories pass

---

### NFR-5: Extensibility
**Priority:** SHOULD HAVE

**Requirements:**
1. **New Channels:** Add Push/WhatsApp without modifying core
2. **Custom Providers:** Replace SendGrid with custom SMTP implementation
3. **Custom Enhancement:** Register domain-specific enhancement providers
4. **Template Storage:** Swap DB templates for file-based or embedded resources
5. **Preference Sources:** Multiple preference sources (DB, API, cache)

**Measurement:**
- Add new channel without core changes
- Swap providers via configuration only

---

### NFR-6: Security
**Priority:** MUST HAVE

**Requirements:**
1. **API Key Protection:** Provider API keys in configuration (IOptions), not hardcoded
2. **PII Logging:** Never log user email addresses or phone numbers
3. **Data Validation:** Sanitize MessageType and Data to prevent injection attacks
4. **Template Safety:** Prevent template injection vulnerabilities
5. **Audit Trail:** Log who sent what message when (correlation ID + headers)

**Measurement:**
- Security audit passes
- No secrets in code or logs

---

### NFR-7: Maintainability
**Priority:** SHOULD HAVE

**Requirements:**
1. **README per Project:** Clear setup and usage instructions
2. **XML Documentation:** All public APIs documented
3. **SOLID Principles:** Single responsibility, dependency inversion
4. **Minimal Dependencies:** Avoid unnecessary NuGet packages
5. **Code Comments:** Complex logic explained (enhancement discovery, template loading)

**Measurement:**
- Code review approval
- Documentation completeness check

---

## Constraints

### Technical Constraints
1. **Target Framework:** .NET 10.0
2. **Async/Await:** All I/O operations must be async
3. **Nullable Enabled:** Nullable reference types required
4. **Implicit Usings Disabled:** Explicit using statements
5. **Dependency Injection:** Use Microsoft.Extensions.DependencyInjection
6. **JSON Library:** Newtonsoft.Json for JObject (compatibility) OR System.Text.Json

### Business Constraints
1. **Backward Compatibility:** Must work with existing main framework components
2. **Provider Costs:** SendGrid/Twilio require paid accounts (support free tier for testing)
3. **Rate Limits:** Respect provider rate limits (SendGrid: 100 emails/sec, Twilio: varies)
4. **Template Management:** Template storage solution TBD (DB vs files vs embedded)

### Architectural Constraints
1. **Layer Placement:** Core in Framework layer, providers in ExternalServices
2. **Provider Pattern:** All channels use provider/factory pattern
3. **OoBDev Standards:** Follow architectural-standards.md
4. **Test Coverage:** 80%+ for Framework layer components

---

## Acceptance Criteria (Overall Feature)

**Feature is COMPLETE when:**
1. ✅ All functional requirements (FR-1 through FR-8) implemented and tested
2. ✅ All non-functional requirements (NFR-1 through NFR-7) met
3. ✅ 80%+ test coverage with passing tests
4. ✅ Documentation complete (README, XML docs, usage examples)
5. ✅ Integration with at least one email provider (SendGrid OR SMTP)
6. ✅ Integration with at least one SMS provider (Twilio)
7. ✅ Configuration guide complete
8. ✅ Migration guide from main's 16 LOC stub

---

## Out of Scope (Future Enhancements)

The following are explicitly OUT OF SCOPE for initial implementation:

1. **Push Notifications** - Future channel (Apple APNS, Google FCM)
2. **WhatsApp/Telegram** - Future messaging channels
3. **Email Attachments** - Add in Phase 2
4. **Bulk Sending** - Batch API for 1,000+ recipients
5. **A/B Testing** - Split testing for templates
6. **Delivery Tracking** - Track open/click rates (provider-specific)
7. **Unsubscribe Management** - Preference UI and API
8. **Template Editor UI** - Admin interface for editing templates
9. **Message History** - Long-term storage of sent messages
10. **Analytics Dashboard** - Reporting on message volume/success

These may be added in future phases based on demand.

---

## Dependencies

### Upstream (Required)
- **OoBDev.Communications.Abstractions** - Interfaces and contracts
- **OoBDev.System** - IStringFormatter, ISelectedService<T>, IObjectSerializer
- **Microsoft.Extensions.Logging** - Logging framework
- **Microsoft.Extensions.DependencyInjection** - DI container
- **Newtonsoft.Json** - JObject for data enhancement

### Downstream (Consumers)
- **OoBDev.Twilio.SendGrid** - Email provider implementation
- **OoBDev.Twilio.SmsMessaging** - SMS provider implementation
- **Application Services** - Order service, user service, etc.

### External Services
- **User Preference Store** - Database or API for user channel preferences
- **Template Store** - Database, file system, or embedded resources
- **Deferral Queue** - Message queue (RabbitMQ, Azure Service Bus, SQS) or database

---

## Risks and Mitigations

### Risk 1: Performance Under Load
**Impact:** HIGH
**Probability:** MEDIUM

**Mitigation:**
- Load testing with realistic message volumes
- Template caching to reduce DB/file access
- Async/await throughout to avoid thread blocking
- Connection pooling for provider APIs

### Risk 2: Provider API Failures
**Impact:** MEDIUM
**Probability:** HIGH

**Mitigation:**
- Graceful degradation (other channels succeed)
- Retry logic via deferral manager
- Circuit breaker pattern for provider calls
- Fallback providers (SendGrid → SMTP)

### Risk 3: Template Management Complexity
**Impact:** MEDIUM
**Probability:** MEDIUM

**Mitigation:**
- Start with file-based templates (simple)
- Support DB templates in Phase 2
- Template validation on load
- Clear error messages for missing templates

### Risk 4: Enhancement Provider Discovery
**Impact:** MEDIUM
**Probability:** LOW

**Mitigation:**
- Use attribute-based discovery (proven pattern)
- Clear documentation on provider registration
- Throw early if no providers registered
- Example providers in tests

---

## Related Documentation

- [Architecture Design](./architecture.md)
- [API Design](./api-design.md)
- [Business Rules](./business-rules.md)
- [Configuration](./configuration.md)
- [Testing Strategy](./testing-strategy.md)
