# Role & Claims Management - Requirements

**Epic:** 07 - Identity & Session Management
**Feature:** Role & Claims Management
**Priority:** HIGH (Security Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~350

---

## Overview

Role-based access control (RBAC) and claims-based authorization supporting role hierarchies, dynamic claim enhancement, and permission checking via IUserRights and IClaimsEnhancer interfaces.

---

## Business Requirements

### BR-1: Role Management
**As a** system administrator
**I want** to create and manage roles with hierarchies
**So that** I can organize permissions efficiently

**Acceptance Criteria:**
- Create roles with name and description
- Assign roles to accounts
- Remove roles from accounts
- Role hierarchies (e.g., Admin inherits Manager permissions)
- Check if account has specific role
- Get all roles for an account

---

### BR-2: Claims Management
**As a** system
**I want** to manage claims for accounts and roles
**So that** I can implement fine-grained authorization

**Acceptance Criteria:**
- Add claims to accounts (user-specific permissions)
- Add claims to roles (role-based permissions)
- Remove claims from accounts/roles
- Get all claims for account (including role claims)
- Check if account has specific claim

---

### BR-3: Permission Checking (IUserRights)
**As a** developer
**I want** a simple interface to check user permissions
**So that** I can implement authorization logic

**Acceptance Criteria:**
- Check if user has permission (by name)
- Check if user has any of multiple permissions (OR logic)
- Check if user has all of multiple permissions (AND logic)
- Performance: < 10ms for permission check
- Cache permission results

---

### BR-4: Dynamic Claims Enhancement (IClaimsEnhancer)
**As a** system
**I want** to dynamically enhance claims at runtime
**So that** claims reflect current state (e.g., account status, tenant membership)

**Acceptance Criteria:**
- Register multiple claims enhancers
- Execute enhancers in order
- Add claims based on account properties
- Add claims based on external data (database, API)
- Performance: < 50ms total enhancement time

---

### BR-5: Role Hierarchies
**As a** system administrator
**I want** role hierarchies to simplify permission management
**So that** higher roles automatically include lower role permissions

**Acceptance Criteria:**
- Define parent-child role relationships
- Accounts with parent role have child role permissions
- Hierarchy depth limited to 5 levels
- Circular dependencies prevented

**Example:**
```
Administrator
├── Manager
│   ├── Supervisor
│   │   └── Employee
│   └── Support
└── Developer
```

---

### BR-6: Custom Claim Types
**As a** developer
**I want** to define custom claim types beyond standard claims
**So that** I can implement domain-specific authorization

**Acceptance Criteria:**
- Standard claims: role, email, name, sub (subject ID)
- Custom claims: tenant, department, permission, feature_flag
- Claim values can be strings, numbers, or booleans
- Multiple claims of same type supported

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IRoleService
{
    Task<Role> CreateRoleAsync(string roleName, string? description = null, CancellationToken ct = default);
    Task AssignRoleAsync(Guid accountId, string roleName, CancellationToken ct = default);
    Task RemoveRoleAsync(Guid accountId, string roleName, CancellationToken ct = default);
    Task<IEnumerable<Role>> GetAccountRolesAsync(Guid accountId, CancellationToken ct = default);
    Task<bool> HasRoleAsync(Guid accountId, string roleName, CancellationToken ct = default);
    Task SetRoleHierarchyAsync(string parentRole, string childRole, CancellationToken ct = default);
}

public interface IClaimsService
{
    Task AddClaimAsync(Guid accountId, Claim claim, CancellationToken ct = default);
    Task RemoveClaimAsync(Guid accountId, string claimType, string claimValue, CancellationToken ct = default);
    Task<IEnumerable<Claim>> GetAccountClaimsAsync(Guid accountId, CancellationToken ct = default);
    Task<bool> HasClaimAsync(Guid accountId, string claimType, string claimValue, CancellationToken ct = default);
    Task<string?> GetClaimValueAsync(Guid accountId, string claimType, CancellationToken ct = default);
}

public interface IUserRights
{
    Task<bool> HasPermissionAsync(Guid accountId, string permission, CancellationToken ct = default);
    Task<bool> HasAnyPermissionAsync(Guid accountId, string[] permissions, CancellationToken ct = default);
    Task<bool> HasAllPermissionsAsync(Guid accountId, string[] permissions, CancellationToken ct = default);
}

public interface IClaimsEnhancer
{
    string Name { get; }
    int Order { get; }
    Task<IEnumerable<Claim>> EnhanceClaimsAsync(ClaimsPrincipal principal, CancellationToken ct = default);
}
```

---

### TR-2: Models
```csharp
public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid? ParentRoleId { get; set; }
    public Role? ParentRole { get; set; }

    public ICollection<Claim> Claims { get; set; } = new List<Claim>();
}

public class Claim
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}
```

---

### TR-3: Performance Requirements
- **Permission check:** < 10ms (cached)
- **Claims enhancement:** < 50ms (all enhancers)
- **Role assignment:** < 100ms (with hierarchy resolution)
- **Get account claims:** < 20ms (cached, includes role claims)

---

### TR-4: Caching Strategy
- Account roles cached for 15 minutes
- Account claims cached for 10 minutes
- Role hierarchies cached indefinitely (invalidate on change)
- Permission checks cached for 5 minutes

---

## Non-Functional Requirements

### NFR-1: Security
- Role assignments audited
- Claim changes audited
- Permission checks not audited (too high volume)
- Prevent privilege escalation via circular hierarchies

### NFR-2: Scalability
- Support 10,000+ roles
- Support 1,000+ claims per account (including role claims)
- Distributed caching for multi-instance deployments

### NFR-3: Extensibility
- Custom claims enhancers via IClaimsEnhancer
- Custom permission evaluation logic
- Plugin-based claim providers

---

## Constraints

### C-1: Role Constraints
- Role names unique per tenant
- Role names case-insensitive
- Hierarchy depth limited to 5 levels
- No circular dependencies

### C-2: Claim Constraints
- Claim type maximum length: 100 characters
- Claim value maximum length: 500 characters
- Maximum 100 claims per account (direct, not including role claims)

---

## Success Criteria

- ✅ Create and manage roles with hierarchies
- ✅ Assign/remove roles from accounts
- ✅ Add/remove claims from accounts and roles
- ✅ Permission checking via IUserRights (< 10ms)
- ✅ Dynamic claims enhancement via IClaimsEnhancer (< 50ms)
- ✅ Role hierarchy resolution (5 levels max)
- ✅ 85%+ test coverage

---

## Out of Scope

- ❌ Attribute-based access control (ABAC) - future enhancement
- ❌ Policy-based authorization (use ASP.NET Core policies)
- ❌ Dynamic role creation by users (admin-only)

---

## Dependencies

### Internal
- **OoBDev.System.Identity.Abstractions** - Core identity interfaces
- **OoBDev.System.Security** - Authorization abstractions

### External
- **System.Security.Claims** - Standard claims infrastructure
- **Microsoft.Extensions.Caching.Abstractions** - Caching

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
