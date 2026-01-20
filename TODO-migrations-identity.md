# Migration TODO - Identity / IdentityModel

**Projects:** IdentityModel.Abstractions (291 LOC), IdentityModel.Extensions (204 LOC)
**Source:** Incoming/SharedFramework/
**Status:** ⚠️ MERGE REQUIRED - Main has 125 LOC basic, SF has 495 LOC enhanced
**Priority:** HIGH

---

## Tasks

### Phase 1: Analysis & Planning
- [ ] Compare main's `OoBDev.Identity.Abstractions` with SF's `IdentityModel.Abstractions`
- [ ] Identify overlapping models (UserCreateModel, UserCreatedModel)
- [ ] Design merge strategy (enhance vs replace)
- [ ] Plan namespace: Keep `OoBDev.Identity` or rename to `IdentityModel`?
- [ ] Document migration path for existing consumers

### Phase 2: Identity.Abstractions Merge (291 LOC)
- [ ] Backup main's Identity.Abstractions
- [ ] Decision: Keep `OoBDev.Identity` namespace (RECOMMENDED)
- [ ] Merge SF's Claims/ folder:
  - [ ] AzB2cClaims.cs (Azure B2C claims support)
  - [ ] ClaimTypesExtended.cs
  - [ ] ClaimsEnhancerAttribute.cs
  - [ ] ClaimsExtensions.cs
  - [ ] IClaimsEnhancer.cs
- [ ] Merge SF's Handlers/ folder:
  - [ ] IClaimsProvider.cs
  - [ ] IRightsProviderFactory.cs (rights management)
- [ ] Merge SF's Models/ folder:
  - [ ] BuildInviteRequestModel.cs
  - [ ] IExtendedProperties.cs
  - [ ] IExtendedProperty.cs
  - [ ] IUserRights.cs (rights system)
  - [ ] PropertyModel.cs
  - [ ] Reconcile UserCreateModel/UserCreatedModel (merge or replace)
- [ ] Merge SF's Providers/ folder:
  - [ ] IManageGraphUser.cs (Microsoft Graph API)
  - [ ] Keep main's IUserManagementProvider (merge features)
- [ ] Update all references

### Phase 3: Identity.Extensions Migration (204 LOC - NEW)
- [ ] Create `src/Framework/OoBDev.Identity.Extensions/`
- [ ] Copy authorization services
- [ ] Copy globalization support
- [ ] Copy extended claims processing
- [ ] Add ServiceCollectionExtensions
- [ ] Create README
- [ ] Add to solution

### Phase 4: Update Existing Identity Projects
- [ ] Update `OoBDev.Identity/` to use enhanced abstractions
- [ ] Update `OoBDev.AspNetCore.JwtAuthentication` if needed
- [ ] Verify Azure B2C integration compatibility
- [ ] Test Graph API integration if used

### Phase 5: Testing
- [ ] Migrate IdentityModel tests
- [ ] Update existing Identity tests
- [ ] Test Azure B2C claims enhancement
- [ ] Test rights management system
- [ ] Test extended properties
- [ ] Test Graph API integration
- [ ] Target 80%+ coverage

### Phase 6: Documentation
- [ ] Document enhanced identity architecture
- [ ] Document Azure B2C integration
- [ ] Document rights management system
- [ ] Document claims enhancement
- [ ] Document Graph API usage
- [ ] Create migration guide for users
- [ ] Add usage examples

### Phase 7: Integration
- [ ] Verify compatibility with existing auth systems
- [ ] Test with Azure B2C if available
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md

---

## Project Structure

```
src/Framework/
├── OoBDev.Identity.Abstractions/     # ENHANCED - Add SF features
│   ├── Claims/                       # NEW from SF
│   ├── Handlers/                     # NEW from SF
│   ├── Models/                       # MERGE with existing
│   ├── Providers/                    # MERGE with existing
│   └── IIdentityManager.cs           # Keep from main
├── OoBDev.Identity/                  # UPDATE to use enhanced abstractions
├── OoBDev.Identity.Extensions/       # NEW - Auth services, globalization
├── OoBDev.Identity.Tests/            # ENHANCED - Add SF tests
└── OoBDev.AspNetCore.JwtAuthentication/ # UPDATE if needed
```

---

## Key Enhancements

**From Main (Keep):**
- ✅ IIdentityManager
- ✅ IUserManagementProvider
- ✅ Basic UserIdentityModel

**From SharedFramework (Add):**
- ➕ Azure B2C claims support
- ➕ Claims enhancement framework (IClaimsEnhancer)
- ➕ Rights management (IUserRights, IRightsProviderFactory)
- ➕ Extended properties (IExtendedProperties)
- ➕ Microsoft Graph API integration (IManageGraphUser)
- ➕ Authorization services
- ➕ Globalization support
- ➕ User invitation system (BuildInviteRequestModel)

---

## Namespace Decision

**Option A: Keep OoBDev.Identity** ⭐ RECOMMENDED
- Maintains consistency with main
- Less breaking changes
- Clear evolution of existing identity

**Option B: Rename to OoBDev.IdentityModel**
- Matches SF naming
- Breaking change for existing code
- More work to migrate

**Recommendation:** Option A

---

## LOC Summary

- Main Identity.Abstractions: 125 LOC
- SF IdentityModel.Abstractions: 291 LOC (2.3x larger)
- SF IdentityModel.Extensions: 204 LOC
- **Total Enhancement:** +370 LOC

---

**Effort:** 2-3 days
**Risk:** MEDIUM - Requires careful merge, some breaking changes possible
