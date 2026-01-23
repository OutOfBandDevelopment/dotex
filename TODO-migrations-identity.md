# Design Documentation - Identity & Session Management

**Epic:** 7 - Identity & Session Management
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
- Architecture captured in `Features/Proposals/REVISIONS_SUMMARY.md` (Revision 13: Claims & Rights)
- High-level design in `Features/Proposals/CONSOLIDATED_DESIGN.md` (Epic 7)

### 🔄 In Progress
Creating detailed 4-document sets for each feature:

**Feature 1: Claims Enhancement Framework** (4 documents)
- [ ] `Features/Proposals/07-Identity/ClaimsEnhancement/requirements.md`
- [ ] `Features/Proposals/07-Identity/ClaimsEnhancement/architecture.md`
- [ ] `Features/Proposals/07-Identity/ClaimsEnhancement/api-design.md`
- [ ] `Features/Proposals/07-Identity/ClaimsEnhancement/testing-strategy.md`

**Feature 2: Rights Management System** (4 documents)
- [ ] `Features/Proposals/07-Identity/RightsManagement/requirements.md`
- [ ] `Features/Proposals/07-Identity/RightsManagement/architecture.md`
- [ ] `Features/Proposals/07-Identity/RightsManagement/api-design.md`
- [ ] `Features/Proposals/07-Identity/RightsManagement/testing-strategy.md`

**Feature 3: Session Management** (4 documents)
- [ ] `Features/Proposals/07-Identity/SessionManagement/requirements.md`
- [ ] `Features/Proposals/07-Identity/SessionManagement/architecture.md`
- [ ] `Features/Proposals/07-Identity/SessionManagement/api-design.md`
- [ ] `Features/Proposals/07-Identity/SessionManagement/testing-strategy.md`

**Feature 4: Azure B2C & Graph Integration** (4 documents)
- [ ] `Features/Proposals/07-Identity/AzureB2CIntegration/requirements.md`
- [ ] `Features/Proposals/07-Identity/AzureB2CIntegration/architecture.md`
- [ ] `Features/Proposals/07-Identity/AzureB2CIntegration/api-design.md`
- [ ] `Features/Proposals/07-Identity/AzureB2CIntegration/testing-strategy.md`

---

## Key Architectural Decisions

**From REVISIONS_SUMMARY.md:**

### Claims Enhancement (Revision 13)
- `IClaimsEnhancer` for runtime claims augmentation
- `IClaimsProvider` for pluggable claims sources
- Azure B2C integration (AzB2cClaims)
- Extended claim types beyond standard .NET claims
- Attribute-based claims enhancement

### Rights Management System
- `IUserRights` for permission management
- `IRightsProviderFactory` for pluggable rights providers
- Fine-grained authorization beyond role-based
- Integration with claims system

### Session Management
- Distributed session support
- Session state providers (Redis, SQL, in-memory)
- Session timeout and renewal
- Cross-device session tracking

### Integration Points
- **IDataContainer**: User context uses container for dynamic data
- **Azure B2C**: Native integration with Azure AD B2C
- **Microsoft Graph**: User management via Graph API
- **Distributed Cache**: Session state storage

---

## Architecture Highlights

**Key Interfaces:**
```csharp
// Claims enhancement
public interface IClaimsEnhancer
{
    Task<IEnumerable<Claim>> EnhanceClaimsAsync(ClaimsPrincipal principal, CancellationToken ct);
}

public interface IClaimsProvider
{
    Task<IEnumerable<Claim>> GetClaimsAsync(string userId, CancellationToken ct);
}

// Rights management
public interface IUserRights
{
    Task<bool> HasRightAsync(string userId, string rightName, CancellationToken ct);
    Task<IEnumerable<string>> GetUserRightsAsync(string userId, CancellationToken ct);
    Task GrantRightAsync(string userId, string rightName, CancellationToken ct);
    Task RevokeRightAsync(string userId, string rightName, CancellationToken ct);
}

public interface IRightsProviderFactory
{
    IRightsProvider CreateProvider(string providerType);
}

// Session management
public interface ISessionManager
{
    Task<ISession?> GetSessionAsync(string sessionId, CancellationToken ct);
    Task CreateSessionAsync(string userId, IDictionary<string, object> data, CancellationToken ct);
    Task UpdateSessionAsync(string sessionId, IDictionary<string, object> data, CancellationToken ct);
    Task InvalidateSessionAsync(string sessionId, CancellationToken ct);
}

// Azure B2C & Graph
public interface IManageGraphUser
{
    Task<GraphUser?> GetUserAsync(string userId, CancellationToken ct);
    Task<GraphUser> CreateUserAsync(UserCreateModel model, CancellationToken ct);
    Task UpdateUserAsync(string userId, UserUpdateModel model, CancellationToken ct);
    Task DeleteUserAsync(string userId, CancellationToken ct);
}

// Extended properties
public interface IExtendedProperties
{
    IDictionary<string, IExtendedProperty> Properties { get; }
    void SetProperty(string key, object? value, string? category = null);
    T? GetProperty<T>(string key);
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
5. Integrate with existing Azure B2C setup

**Phase 3: Migration Path (Future)**
1. Enhance existing OoBDev.Identity.Abstractions (125 LOC → 495+ LOC)
2. Add claims enhancement framework
3. Implement rights management system
4. Add session management with distributed cache
5. Integrate Azure B2C and Microsoft Graph
6. Preserve backward compatibility for existing consumers

---

## Reference Materials

**Design Documents:**
- `Features/Proposals/REVISIONS_SUMMARY.md` - Revision 13: Claims & Rights
- `Features/Proposals/CONSOLIDATED_DESIGN.md` - Epic 7: Identity & Session Management
- `Features/Proposals/11-DataEnhancement/` - Pattern reference (Epic 11)

**Existing Implementation:**
- `Framework/OoBDev.Identity.Abstractions/` - Current implementation (125 LOC)
- `Framework/OoBDev.Identity/` - Identity implementation to enhance
- `Framework/OoBDev.AspNetCore.JwtAuthentication/` - JWT integration to adapt

---

## Success Criteria

- ✅ 16 comprehensive design documents created (requirements, architecture, api-design, testing-strategy)
- ✅ All architectural patterns consistent with Epic 11
- ✅ Integration points with Azure B2C, Microsoft Graph, distributed cache defined
- ✅ Provider pattern for claims, rights, and session providers
- ✅ Test coverage targets defined (85-90%)
- ✅ Migration path from current Identity.Abstractions (125 LOC → 495+ LOC) planned
- ✅ Backward compatibility strategy for existing consumers

---

## Notes

**Why Design-First?**
- Avoids technical debt from legacy SharedFramework code
- Ensures modern .NET 10.0 patterns throughout
- Integrates cleanly with new Epic 11 features (IDataContainer)
- Enables comprehensive test planning upfront
- Documents intent before implementation
- Careful merge strategy for existing identity system

**Benefits:**
- Clean architecture from scratch
- Enhanced identity system (125 LOC → 495+ LOC)
- Proper dependency injection from start
- Built-in extensibility with provider pattern
- Complete test coverage planned upfront
- Azure B2C and Graph API integration
- Rights management beyond role-based auth

**Key Enhancements:**
- Azure B2C claims support (AzB2cClaims)
- Claims enhancement framework (IClaimsEnhancer)
- Rights management (IUserRights, IRightsProviderFactory)
- Extended properties (IExtendedProperties)
- Microsoft Graph API integration (IManageGraphUser)
- Session management with distributed cache
- User invitation system

---

**See Also:**
- [Design Progress](Features/Proposals/DOCUMENTATION_PROGRESS.md)
- [Epic 11 Documentation](Features/Proposals/11-DataEnhancement/) - Reference implementation
