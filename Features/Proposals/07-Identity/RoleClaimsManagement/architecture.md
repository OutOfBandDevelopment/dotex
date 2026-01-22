# Role & Claims Management - Architecture

**Epic:** 07 - Identity & Session Management
**Feature:** Role & Claims Management
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Role & Claims Management system implements a hierarchical RBAC (Role-Based Access Control) architecture with dynamic claims enhancement, permission checking, and efficient caching strategies.

```
┌─────────────────────────────────────────────────────────────────┐
│                      API / Application Layer                    │
│              (Controllers, Authorization Handlers)              │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────────┐
│                    Service Layer                                │
│  ┌────────────────┬──────────────────┬────────────────────┐    │
│  │ IRoleService   │ IClaimsService   │ IUserRights        │    │
│  │                │                  │                    │    │
│  │ - CreateRole   │ - AddClaim       │ - HasPermission    │    │
│  │ - AssignRole   │ - RemoveClaim    │ - HasAnyPermission │    │
│  │ - Hierarchy    │ - GetClaims      │ - HasAllPermissions│    │
│  └────────────────┴──────────────────┴────────────────────┘    │
└────────────────────┬────────────────────────────────────────────┘
                     │
         ┌───────────┼───────────┬─────────────┬──────────────┐
         ↓           ↓           ↓             ↓              ↓
┌─────────────┐ ┌─────────┐ ┌──────────┐ ┌──────────────┐ ┌─────────────┐
│IRoleRepo    │ │IClaimsRp│ │IClaimsEnh│ │IDistribCache │ │IEventPub    │
│             │ │         │ │(Multiple)│ │              │ │             │
│- GetRoles   │ │- GetClms│ │          │ │- Get/Set     │ │- Publish    │
│- SaveRole   │ │- AddClm │ │- Enhance │ │- Remove      │ │  Events     │
│- Hierarchy  │ │- RemClm │ │  Claims  │ │              │ │             │
└─────────────┘ └─────────┘ └──────────┘ └──────────────┘ └─────────────┘
       │             │
       ↓             ↓
┌─────────────────────────────────────────────────────────────────┐
│                   Data Layer (Database)                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │    Roles    │  │   Claims    │  │    Role     │            │
│  │             │  │             │  │  Hierarchy  │            │
│  │             │  │             │  │             │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Core Components

### 1. RoleService (Main Service)

**Responsibilities:**
- Role lifecycle management (create, update, delete)
- Role assignment to accounts
- Role hierarchy management
- Role-based permission resolution

**Key Design Decisions:**
- **Hierarchical roles** - Parent roles inherit child permissions
- **Cached resolution** - Role hierarchies cached for performance
- **Validation** - Circular dependency detection
- **Audit trail** - All role assignments logged

**Implementation Pattern:**
```csharp
public class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<RoleService> _logger;

    public async Task<Role> CreateRoleAsync(
        string roleName,
        string? description = null,
        CancellationToken ct = default)
    {
        // 1. Validate uniqueness
        var existing = await _repository.GetByNameAsync(roleName, ct);
        if (existing != null)
            throw new DuplicateRoleException($"Role '{roleName}' already exists");

        // 2. Create role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        // 3. Save to database
        await _repository.SaveAsync(role, ct);

        // 4. Publish event
        await _eventPublisher.PublishAsync(new RoleCreatedEvent(role.Id, role.Name), ct);

        _logger.LogInformation("Role created: {RoleId}, Name: {RoleName}", role.Id, role.Name);

        return role;
    }

    public async Task AssignRoleAsync(
        Guid accountId,
        string roleName,
        CancellationToken ct = default)
    {
        // 1. Get role
        var role = await _repository.GetByNameAsync(roleName, ct);
        if (role == null)
            throw new RoleNotFoundException($"Role '{roleName}' not found");

        // 2. Check if already assigned
        if (await _repository.HasRoleAsync(accountId, roleName, ct))
        {
            _logger.LogWarning("Role {RoleName} already assigned to account {AccountId}", roleName, accountId);
            return;
        }

        // 3. Assign role
        await _repository.AssignRoleAsync(accountId, role.Id, ct);

        // 4. Invalidate cache
        await InvalidateAccountCacheAsync(accountId);

        // 5. Publish event
        await _eventPublisher.PublishAsync(
            new RoleAssignedEvent(accountId, role.Id, roleName), ct);

        _logger.LogInformation(
            "Role {RoleName} assigned to account {AccountId}", roleName, accountId);
    }

    public async Task SetRoleHierarchyAsync(
        string parentRole,
        string childRole,
        CancellationToken ct = default)
    {
        // 1. Get roles
        var parent = await _repository.GetByNameAsync(parentRole, ct);
        if (parent == null)
            throw new RoleNotFoundException($"Parent role '{parentRole}' not found");

        var child = await _repository.GetByNameAsync(childRole, ct);
        if (child == null)
            throw new RoleNotFoundException($"Child role '{childRole}' not found");

        // 2. Validate no circular dependency
        if (await WouldCreateCircularDependencyAsync(parent.Id, child.Id, ct))
            throw new CircularRoleHierarchyException(
                $"Setting {parentRole} as parent of {childRole} would create circular dependency");

        // 3. Validate depth limit
        var depth = await CalculateHierarchyDepthAsync(parent.Id, ct);
        if (depth >= 5)
            throw new RoleHierarchyDepthException(
                $"Role hierarchy depth limited to 5 levels. Current depth: {depth}");

        // 4. Set parent
        child.ParentRoleId = parent.Id;
        await _repository.UpdateAsync(child, ct);

        // 5. Invalidate hierarchy cache
        await _cache.RemoveAsync("role-hierarchy", ct);

        _logger.LogInformation(
            "Role hierarchy set: {ParentRole} -> {ChildRole}", parentRole, childRole);
    }

    public async Task<IEnumerable<Role>> GetAccountRolesAsync(
        Guid accountId,
        CancellationToken ct = default)
    {
        // 1. Check cache first
        var cacheKey = $"account-roles:{accountId}";
        var cached = await _cache.GetAsync<IEnumerable<Role>>(cacheKey, ct);
        if (cached != null)
            return cached;

        // 2. Get direct roles
        var directRoles = await _repository.GetAccountRolesAsync(accountId, ct);

        // 3. Resolve inherited roles via hierarchy
        var allRoles = new HashSet<Role>(directRoles);
        foreach (var role in directRoles)
        {
            var inheritedRoles = await GetInheritedRolesAsync(role, ct);
            foreach (var inherited in inheritedRoles)
            {
                allRoles.Add(inherited);
            }
        }

        var result = allRoles.ToList();

        // 4. Cache result
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15), ct);

        return result;
    }

    private async Task<IEnumerable<Role>> GetInheritedRolesAsync(
        Role role,
        CancellationToken ct)
    {
        var inherited = new List<Role>();

        if (role.ParentRoleId.HasValue)
        {
            var parent = await _repository.GetByIdAsync(role.ParentRoleId.Value, ct);
            if (parent != null)
            {
                inherited.Add(parent);
                var parentInherited = await GetInheritedRolesAsync(parent, ct);
                inherited.AddRange(parentInherited);
            }
        }

        return inherited;
    }

    private async Task<bool> WouldCreateCircularDependencyAsync(
        Guid parentId,
        Guid childId,
        CancellationToken ct)
    {
        // Check if parent has child as ancestor
        var ancestors = new HashSet<Guid>();
        var current = parentId;

        while (true)
        {
            if (current == childId)
                return true; // Circular dependency

            ancestors.Add(current);

            var role = await _repository.GetByIdAsync(current, ct);
            if (role?.ParentRoleId == null)
                break;

            if (ancestors.Contains(role.ParentRoleId.Value))
                return true; // Circular dependency in existing hierarchy

            current = role.ParentRoleId.Value;
        }

        return false;
    }

    private async Task<int> CalculateHierarchyDepthAsync(
        Guid roleId,
        CancellationToken ct)
    {
        var depth = 0;
        var current = roleId;
        var visited = new HashSet<Guid>();

        while (true)
        {
            if (visited.Contains(current))
                throw new CircularRoleHierarchyException("Circular dependency detected in hierarchy");

            visited.Add(current);

            var role = await _repository.GetByIdAsync(current, ct);
            if (role?.ParentRoleId == null)
                break;

            depth++;
            current = role.ParentRoleId.Value;
        }

        return depth;
    }

    private async Task InvalidateAccountCacheAsync(Guid accountId)
    {
        await _cache.RemoveAsync($"account-roles:{accountId}");
        await _cache.RemoveAsync($"account-claims:{accountId}");
        await _cache.RemoveAsync($"account-permissions:{accountId}");
    }
}
```

---

### 2. ClaimsService (Claims Management)

**Responsibilities:**
- Add/remove claims from accounts
- Add/remove claims from roles
- Get all claims for account (including role claims)
- Claim value retrieval

**Key Design Decisions:**
- **Claim aggregation** - Combines direct + role claims
- **Duplicate handling** - Multiple claims of same type allowed
- **Performance** - Cached claims resolution

**Implementation Pattern:**
```csharp
public class ClaimsService : IClaimsService
{
    private readonly IClaimsRepository _repository;
    private readonly IRoleRepository _roleRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ClaimsService> _logger;

    public async Task AddClaimAsync(
        Guid accountId,
        Claim claim,
        CancellationToken ct = default)
    {
        // 1. Validate claim
        ValidateClaim(claim);

        // 2. Check limit (100 direct claims per account)
        var existingCount = await _repository.GetAccountClaimCountAsync(accountId, ct);
        if (existingCount >= 100)
            throw new ClaimLimitException($"Account {accountId} has reached maximum of 100 direct claims");

        // 3. Add claim
        await _repository.AddClaimAsync(accountId, claim, ct);

        // 4. Invalidate cache
        await _cache.RemoveAsync($"account-claims:{accountId}", ct);

        _logger.LogInformation(
            "Claim added to account {AccountId}: {ClaimType}={ClaimValue}",
            accountId, claim.Type, claim.Value);
    }

    public async Task<IEnumerable<Claim>> GetAccountClaimsAsync(
        Guid accountId,
        CancellationToken ct = default)
    {
        // 1. Check cache
        var cacheKey = $"account-claims:{accountId}";
        var cached = await _cache.GetAsync<IEnumerable<Claim>>(cacheKey, ct);
        if (cached != null)
            return cached;

        // 2. Get direct claims
        var directClaims = await _repository.GetAccountClaimsAsync(accountId, ct);

        // 3. Get role claims
        var roles = await _roleRepository.GetAccountRolesAsync(accountId, ct);
        var roleClaims = new List<Claim>();
        foreach (var role in roles)
        {
            var claims = await _repository.GetRoleClaimsAsync(role.Id, ct);
            roleClaims.AddRange(claims);
        }

        // 4. Combine (direct claims take precedence)
        var allClaims = directClaims.Concat(roleClaims).ToList();

        // 5. Cache result
        await _cache.SetAsync(cacheKey, allClaims, TimeSpan.FromMinutes(10), ct);

        return allClaims;
    }

    public async Task<bool> HasClaimAsync(
        Guid accountId,
        string claimType,
        string claimValue,
        CancellationToken ct = default)
    {
        var claims = await GetAccountClaimsAsync(accountId, ct);
        return claims.Any(c => c.Type == claimType && c.Value == claimValue);
    }

    public async Task<string?> GetClaimValueAsync(
        Guid accountId,
        string claimType,
        CancellationToken ct = default)
    {
        var claims = await GetAccountClaimsAsync(accountId, ct);
        return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    private void ValidateClaim(Claim claim)
    {
        if (string.IsNullOrWhiteSpace(claim.Type))
            throw new ArgumentException("Claim type cannot be empty", nameof(claim));

        if (claim.Type.Length > 100)
            throw new ArgumentException("Claim type cannot exceed 100 characters", nameof(claim));

        if (string.IsNullOrWhiteSpace(claim.Value))
            throw new ArgumentException("Claim value cannot be empty", nameof(claim));

        if (claim.Value.Length > 500)
            throw new ArgumentException("Claim value cannot exceed 500 characters", nameof(claim));
    }
}
```

---

### 3. UserRightsService (Permission Checking)

**Responsibilities:**
- Permission checking (single, any, all)
- Claims-based authorization
- Performance-optimized checks

**Key Design Decisions:**
- **Cache-first** - Permission results cached
- **Flexible matching** - Supports wildcard permissions
- **Fast path** - Direct permission checks avoid full claims resolution

**Implementation Pattern:**
```csharp
public class UserRightsService : IUserRights
{
    private readonly IClaimsService _claimsService;
    private readonly IDistributedCache _cache;

    public async Task<bool> HasPermissionAsync(
        Guid accountId,
        string permission,
        CancellationToken ct = default)
    {
        // 1. Check cache first
        var cacheKey = $"account-permission:{accountId}:{permission}";
        var cached = await _cache.GetAsync<bool?>(cacheKey, ct);
        if (cached.HasValue)
            return cached.Value;

        // 2. Get all claims
        var claims = await _claimsService.GetAccountClaimsAsync(accountId, ct);

        // 3. Check for permission claim
        var hasPermission = claims.Any(c =>
            c.Type == "permission" &&
            (c.Value == permission || MatchesWildcard(c.Value, permission)));

        // 4. Cache result
        await _cache.SetAsync(cacheKey, hasPermission, TimeSpan.FromMinutes(5), ct);

        return hasPermission;
    }

    public async Task<bool> HasAnyPermissionAsync(
        Guid accountId,
        string[] permissions,
        CancellationToken ct = default)
    {
        // Parallel check for performance
        var tasks = permissions.Select(p => HasPermissionAsync(accountId, p, ct));
        var results = await Task.WhenAll(tasks);
        return results.Any(r => r);
    }

    public async Task<bool> HasAllPermissionsAsync(
        Guid accountId,
        string[] permissions,
        CancellationToken ct = default)
    {
        // Parallel check for performance
        var tasks = permissions.Select(p => HasPermissionAsync(accountId, p, ct));
        var results = await Task.WhenAll(tasks);
        return results.All(r => r);
    }

    private bool MatchesWildcard(string pattern, string value)
    {
        // Support wildcard permissions like "documents.*" or "admin:*"
        if (!pattern.Contains('*'))
            return pattern == value;

        var regex = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        return regex.IsMatch(value);
    }
}
```

---

### 4. ClaimsEnhancer (Dynamic Claims)

**Responsibilities:**
- Add claims dynamically at runtime
- Support multiple enhancers in order
- Enrich claims from database/external sources

**Key Design Decisions:**
- **Plugin pattern** - Multiple enhancers registered
- **Ordered execution** - Enhancers run in priority order
- **Performance bound** - Total enhancement time limited

**Implementation Pattern:**
```csharp
public interface IClaimsEnhancer
{
    string Name { get; }
    int Order { get; } // Lower runs first
    Task<IEnumerable<Claim>> EnhanceClaimsAsync(ClaimsPrincipal principal, CancellationToken ct = default);
}

// Example: Account Status Enhancer
public class AccountStatusClaimsEnhancer : IClaimsEnhancer
{
    private readonly IAccountRepository _accountRepository;

    public string Name => "AccountStatus";
    public int Order => 10;

    public async Task<IEnumerable<Claim>> EnhanceClaimsAsync(
        ClaimsPrincipal principal,
        CancellationToken ct = default)
    {
        var accountIdClaim = principal.FindFirst("sub");
        if (accountIdClaim == null || !Guid.TryParse(accountIdClaim.Value, out var accountId))
            return Enumerable.Empty<Claim>();

        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account == null)
            return Enumerable.Empty<Claim>();

        var claims = new List<Claim>
        {
            new Claim("account_status", account.Status.ToString()),
            new Claim("email_verified", account.EmailVerified.ToString()),
            new Claim("mfa_enabled", account.MfaEnabled.ToString())
        };

        return claims;
    }
}

// Example: Tenant Membership Enhancer
public class TenantMembershipClaimsEnhancer : IClaimsEnhancer
{
    private readonly ITenantRepository _tenantRepository;

    public string Name => "TenantMembership";
    public int Order => 20;

    public async Task<IEnumerable<Claim>> EnhanceClaimsAsync(
        ClaimsPrincipal principal,
        CancellationToken ct = default)
    {
        var accountIdClaim = principal.FindFirst("sub");
        if (accountIdClaim == null || !Guid.TryParse(accountIdClaim.Value, out var accountId))
            return Enumerable.Empty<Claim>();

        var tenants = await _tenantRepository.GetAccountTenantsAsync(accountId, ct);

        var claims = new List<Claim>();
        foreach (var tenant in tenants)
        {
            claims.Add(new Claim("tenant", tenant.Id.ToString()));
            claims.Add(new Claim($"tenant:{tenant.Id}:role", tenant.Role));
        }

        return claims;
    }
}

// Claims Enhancement Pipeline
public class ClaimsEnhancementService
{
    private readonly IEnumerable<IClaimsEnhancer> _enhancers;
    private readonly ILogger<ClaimsEnhancementService> _logger;

    public async Task<ClaimsPrincipal> EnhanceClaimsAsync(
        ClaimsPrincipal principal,
        CancellationToken ct = default)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null)
            return principal;

        var startTime = DateTime.UtcNow;

        // Execute enhancers in order
        var orderedEnhancers = _enhancers.OrderBy(e => e.Order);

        foreach (var enhancer in orderedEnhancers)
        {
            try
            {
                var claims = await enhancer.EnhanceClaimsAsync(principal, ct);
                foreach (var claim in claims)
                {
                    identity.AddClaim(claim);
                }

                _logger.LogDebug("Claims enhancer {Name} added {Count} claims",
                    enhancer.Name, claims.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Claims enhancer {Name} failed", enhancer.Name);
                // Continue with other enhancers
            }
        }

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        if (elapsedMs > 50)
        {
            _logger.LogWarning(
                "Claims enhancement took {ElapsedMs}ms (threshold: 50ms)", elapsedMs);
        }

        return new ClaimsPrincipal(identity);
    }
}
```

---

## Data Flow

### Sequence: Assign Role with Hierarchy Resolution

```
┌────────┐      ┌──────────────┐      ┌──────────────┐      ┌──────────┐
│ Client │      │ RoleService  │      │IRoleRepo     │      │ICache    │
└───┬────┘      └──────┬───────┘      └──────┬───────┘      └────┬─────┘
    │                  │                     │                   │
    │ AssignRole       │                     │                   │
    ├─────────────────>│                     │                   │
    │                  │                     │                   │
    │                  │ GetRoleByName       │                   │
    │                  ├────────────────────>│                   │
    │                  │                     │                   │
    │                  │ Role                │                   │
    │                  │<────────────────────┤                   │
    │                  │                     │                   │
    │                  │ HasRole (check dup) │                   │
    │                  ├────────────────────>│                   │
    │                  │                     │                   │
    │                  │ false               │                   │
    │                  │<────────────────────┤                   │
    │                  │                     │                   │
    │                  │ AssignRole          │                   │
    │                  ├────────────────────>│                   │
    │                  │                     │                   │
    │                  │ Success             │                   │
    │                  │<────────────────────┤                   │
    │                  │                     │                   │
    │                  │ Invalidate Cache    │                   │
    │                  ├────────────────────────────────────────>│
    │                  │                     │                   │
    │                  │                     │                   │ Remove:
    │                  │                     │                   │ - account-roles
    │                  │                     │                   │ - account-claims
    │                  │                     │                   │ - permissions
    │                  │                     │                   │
    │ Success          │                     │                   │
    │<─────────────────┤                     │                   │
    │                  │                     │                   │
```

### Sequence: Permission Check with Caching

```
┌────────┐      ┌──────────────┐      ┌──────────────┐      ┌──────────┐
│ Client │      │UserRights    │      │IClaimsService│      │ICache    │
└───┬────┘      └──────┬───────┘      └──────┬───────┘      └────┬─────┘
    │                  │                     │                   │
    │ HasPermission    │                     │                   │
    ├─────────────────>│                     │                   │
    │                  │                     │                   │
    │                  │ GetFromCache        │                   │
    │                  ├────────────────────────────────────────>│
    │                  │                     │                   │
    │                  │ Cache MISS          │                   │
    │                  │<────────────────────────────────────────┤
    │                  │                     │                   │
    │                  │ GetAccountClaims    │                   │
    │                  ├────────────────────>│                   │
    │                  │                     │                   │
    │                  │                     │ GetFromCache      │
    │                  │                     ├──────────────────>│
    │                  │                     │                   │
    │                  │                     │ Cache HIT         │
    │                  │                     │<──────────────────┤
    │                  │                     │                   │
    │                  │ Claims              │                   │
    │                  │<────────────────────┤                   │
    │                  │                     │                   │
    │                  │ CheckPermission     │                   │
    │                  │ (in memory)         │                   │
    │                  │                     │                   │
    │                  │ SetCache(result)    │                   │
    │                  ├────────────────────────────────────────>│
    │                  │                     │                   │
    │ true/false       │                     │                   │
    │<─────────────────┤                     │                   │
    │                  │                     │                   │
```

---

## Design Patterns

### 1. Repository Pattern
- Abstracts data access for roles and claims
- Supports multiple storage backends (SQL, NoSQL)

### 2. Service Layer Pattern
- Business logic encapsulation
- Transaction management
- Cache invalidation coordination

### 3. Provider Pattern
- Multiple IClaimsEnhancer implementations
- Ordered execution pipeline
- Plugin-based extensibility

### 4. Cache-Aside Pattern
- Check cache first
- Query database on miss
- Update cache with result
- Invalidate on changes

---

## Performance Optimizations

### 1. Multi-Level Caching
```csharp
// Level 1: Permission results (5 minutes)
$"account-permission:{accountId}:{permission}"

// Level 2: Account claims (10 minutes)
$"account-claims:{accountId}"

// Level 3: Account roles (15 minutes)
$"account-roles:{accountId}"

// Level 4: Role hierarchy (indefinite, invalidate on change)
"role-hierarchy"
```

### 2. Database Indexes
```sql
CREATE INDEX IX_AccountRoles_AccountId ON AccountRoles(AccountId);
CREATE INDEX IX_AccountRoles_RoleId ON AccountRoles(RoleId);
CREATE INDEX IX_AccountClaims_AccountId ON AccountClaims(AccountId);
CREATE INDEX IX_RoleClaims_RoleId ON RoleClaims(RoleId);
CREATE INDEX IX_Roles_Name ON Roles(Name);
CREATE INDEX IX_Roles_ParentRoleId ON Roles(ParentRoleId);
```

### 3. Parallel Permission Checks
```csharp
// HasAnyPermissionAsync and HasAllPermissionsAsync use Task.WhenAll
var tasks = permissions.Select(p => HasPermissionAsync(accountId, p, ct));
var results = await Task.WhenAll(tasks);
```

### 4. Lazy Loading
- Role hierarchy resolved only when needed
- Claims loaded on-demand
- Enhancement executed only for authenticated requests

---

## Security Considerations

### 1. Circular Dependency Prevention
- Hierarchy validation before setting parent role
- Runtime detection of circular references
- Maximum depth limit (5 levels)

### 2. Privilege Escalation Prevention
- Role assignment audited
- Claim addition audited
- Admin-only role creation

### 3. Cache Poisoning Protection
- Cache invalidation on role/claim changes
- Time-based expiration
- Cryptographically signed cache keys (optional)

---

## Error Handling

### Exception Hierarchy
```csharp
public class RoleException : Exception { }

public class DuplicateRoleException : RoleException { }
public class RoleNotFoundException : RoleException { }
public class CircularRoleHierarchyException : RoleException { }
public class RoleHierarchyDepthException : RoleException { }
public class ClaimLimitException : RoleException { }
```

---

## Testing Strategy

### Unit Tests
- Role hierarchy resolution (5 levels)
- Circular dependency detection
- Permission wildcard matching
- Claims aggregation (direct + role)

### Integration Tests
- End-to-end role assignment
- Claims enhancement pipeline
- Cache invalidation verification
- Database transactions

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
