# Migration TODO - Message Queue Providers

**Projects:** AWS SQS, Azure Service Bus
**Source:** Incoming/SharedFramework/
**Status:** ✅ SAFE - Main has abstractions + RabbitMQ, adding cloud providers
**Priority:** HIGH

---

## Tasks

### Phase 1: Amazon SQS (183 LOC - NEW)
- [ ] Create `src/ExternalServices/Amazon/OoBDev.Amazon.Sqs/`
- [ ] Copy source files from SF
- [ ] Add `AWSSDK.SQS` NuGet package
- [ ] Update namespace to `OoBDev.Amazon.Sqs`
- [ ] Reference OoBDev.MessageQueueing.Abstractions
- [ ] Add ServiceCollectionExtensions with TryAddAmazonSqs()
- [ ] Create README with AWS credentials setup
- [ ] Add to solution

### Phase 2: Azure Service Bus (114 LOC - NEW)
- [ ] Create `src/ExternalServices/Microsoft/OoBDev.Microsoft.Azure.ServiceBus/`
- [ ] Copy source files from SF (already renamed in Phase 0)
- [ ] Add `Azure.Messaging.ServiceBus` NuGet package
- [ ] Update namespace (already done in Phase 0)
- [ ] Reference OoBDev.MessageQueueing.Abstractions
- [ ] Verify topics, sessions, dead-letter queue support
- [ ] Add ServiceCollectionExtensions
- [ ] Create README with Azure connection string setup
- [ ] Add to solution

### Phase 3: Testing
- [ ] Migrate Amazon.Sqs.Tests
- [ ] Migrate Azure.ServiceBus.Tests
- [ ] Add integration tests (LocalStack for AWS, Azurite limitations for Service Bus)
- [ ] Test with existing MessageQueueing framework
- [ ] Target 80%+ coverage

### Phase 4: Documentation
- [ ] Document SQS provider configuration
- [ ] Document Service Bus provider configuration (topics, sessions, DLQ)
- [ ] Add usage examples for both
- [ ] Update message queueing architecture docs
- [ ] Add cloud provider comparison table

### Phase 5: Integration
- [ ] Verify compatibility with OoBDev.MessageQueueing
- [ ] Verify compatibility with OoBDev.Communications.MessageQueueing
- [ ] Test with RabbitMQ provider (no conflicts)
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md

---

## Project Structure

```
src/ExternalServices/
├── Amazon/
│   ├── OoBDev.Amazon.Sqs/          # NEW - AWS SQS provider
│   └── OoBDev.Amazon.Sqs.Tests/    # NEW - Tests
└── Microsoft/
    ├── OoBDev.Microsoft.Azure.ServiceBus/       # NEW - Azure provider
    └── OoBDev.Microsoft.Azure.ServiceBus.Tests/ # NEW - Tests
```

---

## Existing Infrastructure

**Already in Main:**
- ✅ OoBDev.MessageQueueing.Abstractions
- ✅ OoBDev.MessageQueueing (core implementation)
- ✅ OoBDev.MessageQueueing.Hosting (background services)
- ✅ OoBDev.RabbitMQ (existing provider)

**Adding:**
- ➕ AWS SQS provider (for AWS deployments)
- ➕ Azure Service Bus provider (for Azure deployments)

---

## Key Features

**AWS SQS:**
- Standard and FIFO queues
- Message attributes
- Batch operations
- Dead-letter queues

**Azure Service Bus:**
- Topics and subscriptions
- Sessions
- Dead-letter queues
- Message scheduling
- Transactions

---

## LOC Summary

- AWS SQS: 183 LOC
- Azure Service Bus: 114 LOC
- **Total:** ~300 LOC

---

**Effort:** 1-2 days
**Risk:** LOW - Adds to existing framework, no conflicts
