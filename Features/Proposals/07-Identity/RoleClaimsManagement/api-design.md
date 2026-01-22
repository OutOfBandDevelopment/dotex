# Role & Claims Management - API Design

**Epic:** 07 - Identity & Session Management
**Feature:** Role & Claims Management
**Last Updated:** 2026-01-22

---

## API Overview

The Role & Claims Management API provides interfaces for role-based access control (RBAC), claims-based authorization, permission checking, and dynamic claims enhancement.

---

## Core Interfaces

### IRoleService

**Purpose:** Role management and assignment with hierarchy support.

```csharp
namespace OoBDev.System.Identity;

/// <summary>
/// Role management service for RBAC operations.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Creates new role.
    /// </summary>
    /// <param name="roleName">Role name (unique per tenant)</param>
    /// <param name="description">Optional description</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Created role</returns>
    /// <exception cref="DuplicateRoleException">Role already exists</exception>
    Task<Role> CreateRoleAsync(string roleName, string? description = null, CancellationToken ct = default);

    /// <summary>
    /// Updates role details.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="description">New description</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="RoleNotFoundException">Role not found</exception>
    Task UpdateRoleAsync(Guid roleId, string? description, CancellationToken ct = default);

    /// <summary>
    /// Deletes role.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="RoleNotFoundException">Role not found</exception>
    /// <exception cref="RoleInUseException">Role assigned to accounts</exception>
    Task DeleteRoleAsync(Guid roleId, CancellationToken ct = default);

    /// <summary>
    /// Assigns role to account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="roleName">Role name</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="RoleNotFoundException">Role not found</exception>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    Task AssignRoleAsync(Guid accountId, string roleName, CancellationToken ct = default);

    /// <summary>
    /// Removes role from account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="roleName">Role name</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="RoleNotFoundException">Role not found</exception>
    Task RemoveRoleAsync(Guid accountId, string roleName, CancellationToken ct = default);

    /// <summary>
    /// Gets all roles for account (includes inherited roles via hierarchy).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>All roles (direct + inherited)</returns>
    Task<IEnumerable<Role>> GetAccountRolesAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Checks if account has specific role (includes hierarchy).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="roleName">Role name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if account has role</returns>
    Task<bool> HasRoleAsync(Guid accountId, string roleName, CancellationToken ct = default);

    /// <summary>
    /// Sets parent role for hierarchy.
    /// </summary>
    /// <param name="parentRole">Parent role name</param>
    /// <param name="childRole">Child role name</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="RoleNotFoundException">Role not found</exception>
    /// <exception cref="CircularRoleHierarchyException">Would create circular dependency</exception>
    /// <exception cref="RoleHierarchyDepthException">Exceeds depth limit (5 levels)</exception>
    Task SetRoleHierarchyAsync(string parentRole, string childRole, CancellationToken ct = default);

    /// <summary>
    /// Removes role from hierarchy (sets parent to null).
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="RoleNotFoundException">Role not found</exception>
    Task RemoveFromHierarchyAsync(string roleName, CancellationToken ct = default);

    /// <summary>
    /// Gets role by name.
    /// </summary>
    /// <param name="roleName">Role name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Role or null if not found</returns>
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);

    /// <summary>
    /// Lists all roles.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>All roles</returns>
    Task<IEnumerable<Role>> ListRolesAsync(CancellationToken ct = default);
}
```

---

### IClaimsService

**Purpose:** Claims management for accounts and roles.

```csharp
namespace OoBDev.System.Identity;

/// <summary>
/// Claims management service for fine-grained authorization.
/// </summary>
public interface IClaimsService
{
    /// <summary>
    /// Adds claim to account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="claim">Claim to add</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="AccountNotFoundException">Account not found</exception>
    /// <exception cref="ClaimLimitException">Account has 100+ claims</exception>
    Task AddClaimAsync(Guid accountId, Claim claim, CancellationToken ct = default);

    /// <summary>
    /// Removes claim from account.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="claimType">Claim type</param>
    /// <param name="claimValue">Claim value</param>
    /// <param name="ct">Cancellation token</param>
    Task RemoveClaimAsync(Guid accountId, string claimType, string claimValue, CancellationToken ct = default);

    /// <summary>
    /// Adds claim to role (inherited by all accounts with role).
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="claim">Claim to add</param>
    /// <param name="ct">Cancellation token</param>
    /// <exception cref="RoleNotFoundException">Role not found</exception>
    Task AddRoleClaimAsync(Guid roleId, Claim claim, CancellationToken ct = default);

    /// <summary>
    /// Removes claim from role.
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="claimType">Claim type</param>
    /// <param name="claimValue">Claim value</param>
    /// <param name="ct">Cancellation token</param>
    Task RemoveRoleClaimAsync(Guid roleId, string claimType, string claimValue, CancellationToken ct = default);

    /// <summary>
    /// Gets all claims for account (direct + role claims + inherited).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>All claims</returns>
    Task<IEnumerable<Claim>> GetAccountClaimsAsync(Guid accountId, CancellationToken ct = default);

    /// <summary>
    /// Checks if account has specific claim.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="claimType">Claim type</param>
    /// <param name="claimValue">Claim value</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if account has claim</returns>
    Task<bool> HasClaimAsync(Guid accountId, string claimType, string claimValue, CancellationToken ct = default);

    /// <summary>
    /// Gets first claim value of specified type.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="claimType">Claim type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Claim value or null if not found</returns>
    Task<string?> GetClaimValueAsync(Guid accountId, string claimType, CancellationToken ct = default);

    /// <summary>
    /// Gets all claim values of specified type.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="claimType">Claim type</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>All claim values</returns>
    Task<IEnumerable<string>> GetClaimValuesAsync(Guid accountId, string claimType, CancellationToken ct = default);
}
```

---

### IUserRights

**Purpose:** Permission checking abstraction.

```csharp
namespace OoBDev.System.Security;

/// <summary>
/// Permission checking service for authorization.
/// </summary>
public interface IUserRights
{
    /// <summary>
    /// Checks if user has specific permission.
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="permission">Permission name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if user has permission</returns>
    /// <remarks>
    /// Supports wildcard permissions: "documents.*" matches "documents.read", "documents.write"
    /// </remarks>
    Task<bool> HasPermissionAsync(Guid accountId, string permission, CancellationToken ct = default);

    /// <summary>
    /// Checks if user has any of the specified permissions (OR logic).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="permissions">Permissions to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if user has at least one permission</returns>
    Task<bool> HasAnyPermissionAsync(Guid accountId, string[] permissions, CancellationToken ct = default);

    /// <summary>
    /// Checks if user has all specified permissions (AND logic).
    /// </summary>
    /// <param name="accountId">Account ID</param>
    /// <param name="permissions">Permissions to check</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if user has all permissions</returns>
    Task<bool> HasAllPermissionsAsync(Guid accountId, string[] permissions, CancellationToken ct = default);
}
```

---

### IClaimsEnhancer

**Purpose:** Dynamic claims enhancement at runtime.

```csharp
namespace OoBDev.System.Identity;

/// <summary>
/// Claims enhancer for adding dynamic claims at runtime.
/// </summary>
public interface IClaimsEnhancer
{
    /// <summary>
    /// Enhancer name for logging and debugging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Execution order (lower runs first).
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Enhances claims for principal.
    /// </summary>
    /// <param name="principal">Current claims principal</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Additional claims to add</returns>
    /// <remarks>
    /// Implementation must complete in < 50ms (budget for all enhancers).
    /// </remarks>
    Task<IEnumerable<Claim>> EnhanceClaimsAsync(ClaimsPrincipal principal, CancellationToken ct = default);
}
```

---

## Models

### Role
```csharp
namespace OoBDev.System.Identity;

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
```

### Claim
```csharp
namespace System.Security.Claims;

// Standard .NET claim
public class Claim
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}
```

---

## Dependency Injection

### Service Registration
```csharp
namespace OoBDev.System.Identity.Extensions;

public static class RoleClaimsServiceExtensions
{
    /// <summary>
    /// Adds role and claims management services.
    /// </summary>
    public static IServiceCollection AddRoleClaimsManagement(
        this IServiceCollection services,
        Action<RoleClaimsOptions>? configure = null)
    {
        // Configuration
        if (configure != null)
        {
            services.Configure(configure);
        }

        // Core services
        services.TryAddScoped<IRoleService, RoleService>();
        services.TryAddScoped<IClaimsService, ClaimsService>();
        services.TryAddScoped<IUserRights, UserRightsService>();

        // Repositories
        services.TryAddScoped<IRoleRepository, RoleRepository>();
        services.TryAddScoped<IClaimsRepository, ClaimsRepository>();

        // Claims enhancement
        services.TryAddScoped<IClaimsEnhancementService, ClaimsEnhancementService>();

        return services;
    }

    /// <summary>
    /// Registers claims enhancer.
    /// </summary>
    public static IServiceCollection AddClaimsEnhancer<TEnhancer>(
        this IServiceCollection services)
        where TEnhancer : class, IClaimsEnhancer
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IClaimsEnhancer, TEnhancer>());
        return services;
    }
}
```

---

## Usage Examples

### Example 1: Create Role and Assign

```csharp
using OoBDev.System.Identity;

public class RoleManagementController : ControllerBase
{
    private readonly IRoleService _roleService;

    [HttpPost("roles")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRoleAsync([FromBody] CreateRoleRequest request)
    {
        try
        {
            var role = await _roleService.CreateRoleAsync(
                request.RoleName,
                request.Description);

            return Ok(new
            {
                roleId = role.Id,
                name = role.Name,
                description = role.Description,
                createdAt = role.CreatedAt
            });
        }
        catch (DuplicateRoleException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("accounts/{accountId}/roles/{roleName}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRoleAsync(Guid accountId, string roleName)
    {
        try
        {
            await _roleService.AssignRoleAsync(accountId, roleName);

            return Ok(new { message = $"Role '{roleName}' assigned successfully" });
        }
        catch (RoleNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (AccountNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpGet("accounts/{accountId}/roles")]
    [Authorize]
    public async Task<IActionResult> GetAccountRolesAsync(Guid accountId)
    {
        var roles = await _roleService.GetAccountRolesAsync(accountId);

        return Ok(roles.Select(r => new
        {
            id = r.Id,
            name = r.Name,
            description = r.Description,
            parentRoleId = r.ParentRoleId
        }));
    }
}
```

---

### Example 2: Role Hierarchy

```csharp
[HttpPost("roles/hierarchy")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> SetHierarchyAsync([FromBody] SetHierarchyRequest request)
{
    try
    {
        await _roleService.SetRoleHierarchyAsync(request.ParentRole, request.ChildRole);

        return Ok(new
        {
            message = $"Hierarchy set: {request.ParentRole} -> {request.ChildRole}"
        });
    }
    catch (CircularRoleHierarchyException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
    catch (RoleHierarchyDepthException ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}

// Example hierarchy setup
public async Task SetupDefaultHierarchyAsync()
{
    // Create roles
    await _roleService.CreateRoleAsync("Administrator", "Full system access");
    await _roleService.CreateRoleAsync("Manager", "Management access");
    await _roleService.CreateRoleAsync("Supervisor", "Supervision access");
    await _roleService.CreateRoleAsync("Employee", "Basic access");

    // Set hierarchy
    await _roleService.SetRoleHierarchyAsync("Administrator", "Manager");
    await _roleService.SetRoleHierarchyAsync("Manager", "Supervisor");
    await _roleService.SetRoleHierarchyAsync("Supervisor", "Employee");

    // Now:
    // - Administrator has all permissions
    // - Manager has Supervisor + Employee permissions
    // - Supervisor has Employee permissions
    // - Employee has only Employee permissions
}
```

---

### Example 3: Claims Management

```csharp
public class ClaimsController : ControllerBase
{
    private readonly IClaimsService _claimsService;

    [HttpPost("accounts/{accountId}/claims")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddClaimAsync(
        Guid accountId,
        [FromBody] AddClaimRequest request)
    {
        try
        {
            await _claimsService.AddClaimAsync(
                accountId,
                new Claim(request.ClaimType, request.ClaimValue));

            return Ok(new { message = "Claim added successfully" });
        }
        catch (ClaimLimitException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("accounts/{accountId}/claims")]
    [Authorize]
    public async Task<IActionResult> GetClaimsAsync(Guid accountId)
    {
        // Verify user can view claims
        var currentUserId = GetCurrentUserId();
        if (currentUserId != accountId && !User.IsInRole("Admin"))
            return Forbid();

        var claims = await _claimsService.GetAccountClaimsAsync(accountId);

        return Ok(claims.Select(c => new
        {
            type = c.Type,
            value = c.Value
        }));
    }

    [HttpPost("roles/{roleId}/claims")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddRoleClaimAsync(
        Guid roleId,
        [FromBody] AddClaimRequest request)
    {
        try
        {
            await _claimsService.AddRoleClaimAsync(
                roleId,
                new Claim(request.ClaimType, request.ClaimValue));

            return Ok(new { message = "Claim added to role successfully" });
        }
        catch (RoleNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
```

---

### Example 4: Permission Checking

```csharp
public class DocumentsController : ControllerBase
{
    private readonly IUserRights _userRights;

    [HttpGet("{documentId}")]
    [Authorize]
    public async Task<IActionResult> GetDocumentAsync(Guid documentId)
    {
        var accountId = GetCurrentUserId();

        // Check permission
        if (!await _userRights.HasPermissionAsync(accountId, "documents.read"))
        {
            return Forbid();
        }

        // Retrieve document...
        return Ok(document);
    }

    [HttpPost("{documentId}")]
    [Authorize]
    public async Task<IActionResult> UpdateDocumentAsync(
        Guid documentId,
        [FromBody] UpdateDocumentRequest request)
    {
        var accountId = GetCurrentUserId();

        // Check permission
        if (!await _userRights.HasPermissionAsync(accountId, "documents.write"))
        {
            return Forbid();
        }

        // Update document...
        return Ok();
    }

    [HttpDelete("{documentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteDocumentAsync(Guid documentId)
    {
        var accountId = GetCurrentUserId();

        // Check multiple permissions (must have either)
        var canDelete = await _userRights.HasAnyPermissionAsync(
            accountId,
            new[] { "documents.delete", "admin.*" });

        if (!canDelete)
        {
            return Forbid();
        }

        // Delete document...
        return NoContent();
    }
}
```

---

### Example 5: Wildcard Permissions

```csharp
// Setup permissions with wildcards
public async Task SetupPermissionsAsync()
{
    var adminRole = await _roleService.GetRoleByNameAsync("Administrator");

    // Admin has all permissions
    await _claimsService.AddRoleClaimAsync(
        adminRole!.Id,
        new Claim("permission", "admin.*"));

    var managerRole = await _roleService.GetRoleByNameAsync("Manager");

    // Manager has all document permissions
    await _claimsService.AddRoleClaimAsync(
        managerRole!.Id,
        new Claim("permission", "documents.*"));

    var employeeRole = await _roleService.GetRoleByNameAsync("Employee");

    // Employee has only read permissions
    await _claimsService.AddRoleClaimAsync(
        employeeRole!.Id,
        new Claim("permission", "documents.read"));
}

// Permission checks automatically handle wildcards
var hasAccess = await _userRights.HasPermissionAsync(accountId, "documents.write");
// - Admin: true (matches admin.*)
// - Manager: true (matches documents.*)
// - Employee: false (only has documents.read)
```

---

### Example 6: Custom Claims Enhancer

```csharp
// Register enhancer
public void ConfigureServices(IServiceCollection services)
{
    services.AddRoleClaimsManagement();
    services.AddClaimsEnhancer<AccountStatusClaimsEnhancer>();
    services.AddClaimsEnhancer<TenantMembershipClaimsEnhancer>();
}

// Implement enhancer
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

        return new[]
        {
            new Claim("account_status", account.Status.ToString()),
            new Claim("email_verified", account.EmailVerified.ToString()),
            new Claim("mfa_enabled", account.MfaEnabled.ToString())
        };
    }
}

// Use in authentication middleware
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var enhancementService = context.RequestServices
            .GetRequiredService<IClaimsEnhancementService>();

        var enhanced = await enhancementService.EnhanceClaimsAsync(context.User);
        context.User = enhanced;
    }

    await next();
});
```

---

### Example 7: Authorization Policy

```csharp
// Define custom authorization handler
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserRights _userRights;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var accountIdClaim = context.User.FindFirst("sub");
        if (accountIdClaim == null || !Guid.TryParse(accountIdClaim.Value, out var accountId))
        {
            context.Fail();
            return;
        }

        var hasPermission = await _userRights.HasPermissionAsync(
            accountId,
            requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

// Register in Startup
services.AddAuthorization(options =>
{
    options.AddPolicy("CanReadDocuments", policy =>
        policy.Requirements.Add(new PermissionRequirement("documents.read")));

    options.AddPolicy("CanWriteDocuments", policy =>
        policy.Requirements.Add(new PermissionRequirement("documents.write")));
});

services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// Use in controllers
[HttpGet]
[Authorize(Policy = "CanReadDocuments")]
public async Task<IActionResult> GetDocumentsAsync()
{
    // User has documents.read permission
    return Ok(documents);
}
```

---

### Example 8: Configuration

```csharp
// Program.cs or Startup.cs
services.AddRoleClaimsManagement(options =>
{
    // Role options
    options.MaxHierarchyDepth = 5;
    options.CaseSensitiveRoleNames = false;

    // Claims options
    options.MaxClaimsPerAccount = 100;
    options.MaxClaimTypeLength = 100;
    options.MaxClaimValueLength = 500;

    // Caching
    options.CacheRolesDuration = TimeSpan.FromMinutes(15);
    options.CacheClaimsDuration = TimeSpan.FromMinutes(10);
    options.CachePermissionsDuration = TimeSpan.FromMinutes(5);

    // Performance
    options.ClaimsEnhancementTimeoutMs = 50;
});

// Register enhancers
services.AddClaimsEnhancer<AccountStatusClaimsEnhancer>();
services.AddClaimsEnhancer<TenantMembershipClaimsEnhancer>();
services.AddClaimsEnhancer<FeatureFlagClaimsEnhancer>();
```

---

## Error Handling

### Exception Types
```csharp
namespace OoBDev.System.Identity;

public class RoleException : Exception
{
    public RoleException(string message) : base(message) { }
}

public class DuplicateRoleException : RoleException
{
    public DuplicateRoleException(string message) : base(message) { }
}

public class RoleNotFoundException : RoleException
{
    public RoleNotFoundException(string message) : base(message) { }
}

public class CircularRoleHierarchyException : RoleException
{
    public CircularRoleHierarchyException(string message) : base(message) { }
}

public class RoleHierarchyDepthException : RoleException
{
    public RoleHierarchyDepthException(string message) : base(message) { }
}

public class RoleInUseException : RoleException
{
    public RoleInUseException(string message) : base(message) { }
}

public class ClaimLimitException : RoleException
{
    public ClaimLimitException(string message) : base(message) { }
}
```

---

## Best Practices

### 1. Always Use Permission Claims
```csharp
// ✅ GOOD - Check permissions via IUserRights
if (await _userRights.HasPermissionAsync(accountId, "documents.read"))
{
    // Authorized
}

// ❌ BAD - Hard-coded role checks
if (User.IsInRole("Admin"))
{
    // Not flexible, hard to maintain
}
```

### 2. Use Wildcard Permissions
```csharp
// ✅ GOOD - Wildcard for admin
await _claimsService.AddRoleClaimAsync(adminRole, new Claim("permission", "admin.*"));

// ✅ GOOD - Specific permissions for employees
await _claimsService.AddRoleClaimAsync(empRole, new Claim("permission", "documents.read"));
```

### 3. Leverage Role Hierarchies
```csharp
// ✅ GOOD - Use hierarchy
await _roleService.SetRoleHierarchyAsync("Manager", "Employee");
// Manager automatically gets Employee permissions

// ❌ BAD - Duplicate permissions
await _claimsService.AddRoleClaimAsync(managerRole, employeePermissions);
```

---

## Performance Considerations

### Caching Strategy
```csharp
// Roles cached for 15 minutes
var roles = await _roleService.GetAccountRolesAsync(accountId);

// Claims cached for 10 minutes
var claims = await _claimsService.GetAccountClaimsAsync(accountId);

// Permissions cached for 5 minutes
var hasPermission = await _userRights.HasPermissionAsync(accountId, "documents.read");
```

### Parallel Permission Checks
```csharp
// Efficient: Parallel checks
var hasAny = await _userRights.HasAnyPermissionAsync(
    accountId,
    new[] { "documents.read", "documents.write", "admin.*" });

// Inefficient: Sequential checks
var hasRead = await _userRights.HasPermissionAsync(accountId, "documents.read");
var hasWrite = await _userRights.HasPermissionAsync(accountId, "documents.write");
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 07 Overview](../README.md)
