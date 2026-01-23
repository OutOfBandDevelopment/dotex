# Design Documentation - Communications Platform

**Epic:** 2 - Communications Platform
**Status:** 📝 DESIGN PHASE (Replaces code migration)
**Priority:** HIGH
**Strategy:** Design-first approach with comprehensive documentation before implementation

---

## Overview

**Strategic Change (2026-01-22):** Instead of migrating code from SharedFramework, we are creating comprehensive design documentation following the Epic 11 pattern. This ensures:
- Clean architecture from first principles
- Modern .NET 10.0 patterns throughout
- Proper integration with IDataContainer, schema discovery, and path translation
- No technical debt from legacy code
- Complete test coverage from day one

---

## Documentation Tasks

### ✅ Completed
- Architecture captured in `Features/Proposals/REVISIONS_SUMMARY.md` (Revision 6: Channel Abstraction)
- High-level design in `Features/Proposals/CONSOLIDATED_DESIGN.md` (Epic 2)

### 🔄 In Progress
Creating detailed 4-document sets for each feature:

**Feature 1: Channel Abstraction** (4 documents)
- [ ] `Features/Proposals/02-Communications/ChannelAbstraction/requirements.md`
- [ ] `Features/Proposals/02-Communications/ChannelAbstraction/architecture.md`
- [ ] `Features/Proposals/02-Communications/ChannelAbstraction/api-design.md`
- [ ] `Features/Proposals/02-Communications/ChannelAbstraction/testing-strategy.md`

**Feature 2: Send & Receive** (4 documents)
- [ ] `Features/Proposals/02-Communications/SendReceive/requirements.md`
- [ ] `Features/Proposals/02-Communications/SendReceive/architecture.md`
- [ ] `Features/Proposals/02-Communications/SendReceive/api-design.md`
- [ ] `Features/Proposals/02-Communications/SendReceive/testing-strategy.md`

**Feature 3: User Preferences** (4 documents)
- [ ] `Features/Proposals/02-Communications/UserPreferences/requirements.md`
- [ ] `Features/Proposals/02-Communications/UserPreferences/architecture.md`
- [ ] `Features/Proposals/02-Communications/UserPreferences/api-design.md`
- [ ] `Features/Proposals/02-Communications/UserPreferences/testing-strategy.md`

**Feature 4: Multi-Channel Routing** (4 documents)
- [ ] `Features/Proposals/02-Communications/MultiChannelRouting/requirements.md`
- [ ] `Features/Proposals/02-Communications/MultiChannelRouting/architecture.md`
- [ ] `Features/Proposals/02-Communications/MultiChannelRouting/api-design.md`
- [ ] `Features/Proposals/02-Communications/MultiChannelRouting/testing-strategy.md`

---

## Key Architectural Decisions

**From REVISIONS_SUMMARY.md:**

### Channel Abstraction (Revision 6)
- `IChannel` base interface (Email, SMS, Push, Slack, Teams, etc.)
- `IChannelProvider` for pluggable implementations
- `IChannelRegistry` for runtime discovery
- `IMessage` base with channel-specific implementations
- Provider pattern for extensibility

### Integration Points
- **IDataContainer**: Message templates use container for data binding
- **Template Engine**: Handlebars/custom templates for message composition
- **Validation**: Injectable validation for message content
- **Background Tasks**: Platform-agnostic sending (Azure Functions, Lambda, etc.)

---

## Architecture Highlights

**Key Interfaces:**
```csharp
// Base channel abstraction
public interface IChannel
{
    string ChannelType { get; }
    Task<SendResult> SendAsync(IMessage message, CancellationToken ct);
    Task<MessageStatus> GetStatusAsync(string messageId, CancellationToken ct);
}

// Channel providers (Email, SMS, Push, etc.)
public interface IEmailChannel : IChannel
{
    Task<SendResult> SendEmailAsync(IEmailMessage message, CancellationToken ct);
}

// Channel registry
public interface IChannelRegistry
{
    void RegisterChannel(IChannel channel);
    IChannel? GetChannel(string channelType);
    IEnumerable<IChannel> GetAllChannels();
}

// Message composition with templates
public interface IMessageComposer
{
    Task<IMessage> ComposeAsync(string templateId, IDataContainer data, CancellationToken ct);
}

// User preferences
public interface IUserPreferences
{
    Task<ChannelPreference[]> GetPreferencesAsync(Guid userId, CancellationToken ct);
    Task SetPreferenceAsync(Guid userId, ChannelPreference preference, CancellationToken ct);
}

// Multi-channel routing
public interface IChannelRouter
{
    Task<SendResult[]> SendToPreferredChannelsAsync(Guid userId, IMessage message, CancellationToken ct);
}
```

---

## Implementation Strategy

**Phase 1: Design Documentation (Current)**
1. Complete all 16 design documents (4 per feature)
2. Review and validate designs
3. Get stakeholder approval

**Phase 2: Implementation (Future)**
1. Implement based on approved designs
2. Follow provider/factory pattern consistently
3. Write tests alongside implementation (TDD)
4. Target 85-90% test coverage
5. Integrate with existing MailKit provider

**Phase 3: Migration Path (Future)**
1. Create adapters for existing MailKit integration
2. Add new channel providers (SMS, Push, Slack, Teams)
3. Implement user preferences storage
4. Add multi-channel routing logic
5. Preserve backward compatibility where needed

---

## Reference Materials

**Design Documents:**
- `Features/Proposals/REVISIONS_SUMMARY.md` - Revision 6: Channel Abstraction
- `Features/Proposals/CONSOLIDATED_DESIGN.md` - Epic 2: Communications Platform
- `Features/Proposals/11-DataEnhancement/` - Pattern reference (Epic 11)

**Existing Implementation:**
- `Framework/OoBDev.Communications.Abstractions/` - Current stubs (16 LOC)
- `ExternalServices/MailKit/OoBDev.MailKit/` - Email implementation to adapt

---

## Success Criteria

- ✅ 16 comprehensive design documents created (requirements, architecture, api-design, testing-strategy)
- ✅ All architectural patterns consistent with Epic 11
- ✅ Integration points with IDataContainer, templates, validation defined
- ✅ Provider pattern for all channel types
- ✅ Test coverage targets defined (85-90%)
- ✅ Migration path from current MailKit implementation planned

---

## Notes

**Why Design-First?**
- Avoids technical debt from legacy SharedFramework code
- Ensures modern .NET 10.0 patterns throughout
- Integrates cleanly with new Epic 11 features (IDataContainer, path translation, schema discovery)
- Enables comprehensive test planning upfront
- Documents intent before implementation

**Benefits:**
- Clean architecture from scratch
- No namespace migrations needed
- Proper dependency injection from start
- Built-in extensibility with provider pattern
- Complete test coverage planned upfront

---

**See Also:**
- [Design Progress](Features/Proposals/DOCUMENTATION_PROGRESS.md)
- [Epic 11 Documentation](Features/Proposals/11-DataEnhancement/) - Reference implementation
