# Migration TODO - Communications

**Projects:** OoBDev.Communications + OoBDev.Communications.Abstractions
**Source:** Incoming/SharedFramework/
**Status:** 🔥 CRITICAL - Main has 16 LOC stub vs 1,145 LOC (71x difference!)
**Priority:** IMMEDIATE

---

## Tasks

### Phase 1: Preparation
- [ ] Create backup branch: `backup/communications-before-sf-merge`
- [ ] Document current MailKit integration points
- [ ] Design adapter for `ICommunicationSender<T>` ↔ `ISendEmailProvider`
- [ ] List all projects referencing OoBDev.Communications

### Phase 2: Communications.Abstractions (47 files, 695 LOC)
- [ ] Backup main's Communications.Abstractions
- [ ] Copy SF structure (Channels/, Composers/, DeliveryLog/, Handler/, Models/, Services/)
- [ ] Keep main's `ICommunicationSender<T>` for backward compatibility
- [ ] Merge SF's channel-specific interfaces (ISendEmailProvider, ISendSmsProvider)
- [ ] Update all project references
- [ ] Verify MailKit builds

### Phase 3: Communications Implementation (20+ files, 1,145 LOC)
- [ ] Replace main's empty ServiceCollectionExtensions
- [ ] Copy SF's Composers/ (Email/SMS with templates)
- [ ] Copy SF's Handler/ (processor, preferences, enhancement)
- [ ] Copy SF's Provider/ (email and SMS providers)
- [ ] Copy SF's Services/ (orchestration)
- [ ] Preserve Communications.MessageQueueing integration

### Phase 4: MailKit Integration Update
- [ ] Add `ISendEmailProvider` interface to MailKitProvider
- [ ] Implement adapter methods (scheduling may throw NotSupported for SMTP)
- [ ] Keep existing `ICommunicationSender<EmailMessageModel>` implementation
- [ ] Test both interface patterns work
- [ ] Verify health checks still function

### Phase 5: Testing
- [ ] Migrate SF Communications tests
- [ ] Test MailKit with both interfaces
- [ ] Test MessageQueueing integration
- [ ] Test multi-channel routing
- [ ] Test preference management
- [ ] Target 80%+ coverage

### Phase 6: Documentation
- [ ] Create README with architecture overview
- [ ] Document provider patterns
- [ ] Add usage examples for each provider
- [ ] Create migration guide for users
- [ ] Add XML documentation

### Phase 7: Cleanup
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md, CLAUDE.md
- [ ] Delete backed-up files

---

## Key Files

**Main (src/):**
- `Framework/OoBDev.Communications.Abstractions/` - 5 files, 125 LOC (basic)
- `Framework/OoBDev.Communications/` - 1 file, 16 LOC (stub!)
- `Framework/OoBDev.Communications.MessageQueueing/` - Keep separate
- `ExternalServices/MailKit/OoBDev.MailKit/` - Needs adapter update

**SharedFramework (Incoming/):**
- `OoBDev.Communications.Abstractions/` - 47 files, 695 LOC
- `OoBDev.Communications/` - 20 files, 1,145 LOC

---

## Integration Strategy

**Keep Both Interface Patterns:**
```csharp
// Main's generic (keep for MailKit compatibility)
public interface ICommunicationSender<TMessageType>
{
    Task<string> SendAsync(TMessageType message);
}

// SF's channel-specific (add for advanced features)
public interface ISendEmailProvider
{
    Task<string?> ScheduleSendMessageAsync(IEmailMessage message, string? messageId = null);
    Task SendMessageAsync(IEmailMessage message);
}

// MailKit implements both
public class MailKitProvider :
    ICommunicationSender<EmailMessageModel>,
    ISendEmailProvider
```

---

## Dependencies

**Enables:**
- Twilio SendGrid migration (TODO-migrations-twilio.md)
- Twilio SMS migration (TODO-migrations-twilio.md)
- Multi-channel communications

**Blocks:**
- Advanced communications features
- Channel routing implementations

---

**Related:** [MailKit Integration Analysis](docs/migration/mailkit-communications-integration-analysis.md)
