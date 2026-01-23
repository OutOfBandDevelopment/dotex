# Role & Claims Management - Testing Strategy

**Epic:** 07 - Identity & Session Management
**Feature:** Role & Claims Management
**Last Updated:** 2026-01-22

---

## Testing Overview

Comprehensive testing strategy covering role hierarchies, claims aggregation, permission checking, dynamic enhancement, and performance validation with 85%+ coverage requirement.

---

## Test Categories

### Unit Tests (TestCategory.Unit)
- Service logic with mocked dependencies
- Role hierarchy resolution
- Circular dependency detection
- Permission wildcard matching
- Claims aggregation logic
- Validation rules

**Target Coverage:** 90%+

### Integration Tests (TestCategory.Integration)
- Database operations with Docker SQL Server
- Cache invalidation flows
- End-to-end role assignment
- Claims enhancement pipeline
- Multi-layer permission checks

**Target Coverage:** 80%+

### Performance Tests (TestCategory.Unit)
- Permission check < 10ms
- Claims enhancement < 50ms
- Role resolution < 20ms
- Cache hit ratios

**Target Coverage:** Key operations

---

## Unit Test Specifications

### 1. RoleService Tests

#### Test: CreateRole_ValidRequest_CreatesRole
```csharp
[TestClass]
public class RoleServiceTests
{
    private Mock<IRoleRepository> _mockRepository;
    private Mock<IDistributedCache> _mockCache;
    private Mock<IEventPublisher> _mockEventPublisher;
    private RoleService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IRoleRepository>();
        _mockCache = new Mock<IDistributedCache>();
        _mockEventPublisher = new Mock<IEventPublisher>();

        _service = new RoleService(
            _mockRepository.Object,
            _mockCache.Object,
            _mockEventPublisher.Object,
            Mock.Of<ILogger<RoleService>>());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CreateRole_ValidRequest_CreatesRole()
    {
        // Stage
        var roleName = "Manager";
        var description = "Management role";

        _mockRepository
            .Setup(r => r.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Test
        var result = await _service.CreateRoleAsync(roleName, description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(roleName, result.Name);
        Assert.AreEqual(description, result.Description);
        Assert.AreNotEqual(Guid.Empty, result.Id);
        Assert.IsTrue(result.CreatedAt <= DateTime.UtcNow);

        // Verify
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEventPublisher.Verify(
            e => e.PublishAsync(It.IsAny<RoleCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(DuplicateRoleException))]
    public async Task CreateRole_DuplicateName_ThrowsException()
    {
        // Stage
        var roleName = "Manager";
        var existingRole = new Role { Id = Guid.NewGuid(), Name = roleName };

        _mockRepository
            .Setup(r => r.GetByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);

        // Test
        await _service.CreateRoleAsync(roleName);

        // Assert via ExpectedException
    }
}
```

---

#### Test: SetRoleHierarchy_ValidRoles_SetsParent
```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
public async Task SetRoleHierarchy_ValidRoles_SetsParent()
{
    // Stage
    var parent = new Role { Id = Guid.NewGuid(), Name = "Manager" };
    var child = new Role { Id = Guid.NewGuid(), Name = "Employee" };

    _mockRepository
        .Setup(r => r.GetByNameAsync("Manager", It.IsAny<CancellationToken>()))
        .ReturnsAsync(parent);

    _mockRepository
        .Setup(r => r.GetByNameAsync("Employee", It.IsAny<CancellationToken>()))
        .ReturnsAsync(child);

    _mockRepository
        .Setup(r => r.GetByIdAsync(parent.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(parent);

    _mockRepository
        .Setup(r => r.UpdateAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
        .Returns(Task.CompletedTask);

    // Test
    await _service.SetRoleHierarchyAsync("Manager", "Employee");

    // Verify
    _mockRepository.Verify(r => r.UpdateAsync(
        It.Is<Role>(role => role.Id == child.Id && role.ParentRoleId == parent.Id),
        It.IsAny<CancellationToken>()),
        Times.Once);

    _mockCache.Verify(c => c.RemoveAsync("role-hierarchy", It.IsAny<CancellationToken>()), Times.Once);
}
```

---

#### Test: SetRoleHierarchy_CircularDependency_ThrowsException
```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
[ExpectedException(typeof(CircularRoleHierarchyException))]
public async Task SetRoleHierarchy_CircularDependency_ThrowsException()
{
    // Stage: A -> B -> C, trying to set C -> A (circular)
    var roleA = new Role { Id = Guid.NewGuid(), Name = "A", ParentRoleId = null };
    var roleB = new Role { Id = Guid.NewGuid(), Name = "B", ParentRoleId = roleA.Id };
    var roleC = new Role { Id = Guid.NewGuid(), Name = "C", ParentRoleId = roleB.Id };

    _mockRepository
        .Setup(r => r.GetByNameAsync("C", It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleC);

    _mockRepository
        .Setup(r => r.GetByNameAsync("A", It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleA);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleC.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleC);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleB.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleB);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleA.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleA);

    // Test - trying to set C as parent of A
    await _service.SetRoleHierarchyAsync("C", "A");

    // Assert via ExpectedException
}
```

---

#### Test: SetRoleHierarchy_ExceedsDepthLimit_ThrowsException
```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
[ExpectedException(typeof(RoleHierarchyDepthException))]
public async Task SetRoleHierarchy_ExceedsDepthLimit_ThrowsException()
{
    // Stage: Create 5-level hierarchy (A -> B -> C -> D -> E)
    var roleA = new Role { Id = Guid.NewGuid(), Name = "A" };
    var roleB = new Role { Id = Guid.NewGuid(), Name = "B", ParentRoleId = roleA.Id };
    var roleC = new Role { Id = Guid.NewGuid(), Name = "C", ParentRoleId = roleB.Id };
    var roleD = new Role { Id = Guid.NewGuid(), Name = "D", ParentRoleId = roleC.Id };
    var roleE = new Role { Id = Guid.NewGuid(), Name = "E", ParentRoleId = roleD.Id };
    var roleF = new Role { Id = Guid.NewGuid(), Name = "F" };

    // Mock hierarchy chain
    _mockRepository
        .Setup(r => r.GetByNameAsync("A", It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleA);

    _mockRepository
        .Setup(r => r.GetByNameAsync("F", It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleF);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleA.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleA);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleB.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleB);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleC.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleC);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleD.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleD);

    _mockRepository
        .Setup(r => r.GetByIdAsync(roleE.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(roleE);

    // Test - trying to set A as parent of F (would create 6th level)
    await _service.SetRoleHierarchyAsync("A", "F");

    // Assert via ExpectedException
}
```

---

#### Test: GetAccountRoles_WithHierarchy_ReturnsInheritedRoles
```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
public async Task GetAccountRoles_WithHierarchy_ReturnsInheritedRoles()
{
    // Stage: Hierarchy: Admin -> Manager -> Employee
    var employeeRole = new Role { Id = Guid.NewGuid(), Name = "Employee" };
    var managerRole = new Role
    {
        Id = Guid.NewGuid(),
        Name = "Manager",
        ParentRoleId = employeeRole.Id
    };
    var adminRole = new Role
    {
        Id = Guid.NewGuid(),
        Name = "Admin",
        ParentRoleId = managerRole.Id
    };

    var accountId = Guid.NewGuid();

    // Account has only Manager role directly
    _mockRepository
        .Setup(r => r.GetAccountRolesAsync(accountId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new[] { managerRole });

    _mockRepository
        .Setup(r => r.GetByIdAsync(employeeRole.Id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(employeeRole);

    _mockCache
        .Setup(c => c.GetAsync<IEnumerable<Role>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((IEnumerable<Role>?)null);

    // Test
    var result = (await _service.GetAccountRolesAsync(accountId)).ToList();

    // Assert
    Assert.AreEqual(2, result.Count); // Manager + Employee (inherited)
    Assert.IsTrue(result.Any(r => r.Name == "Manager"));
    Assert.IsTrue(result.Any(r => r.Name == "Employee"));

    // Verify cache set
    _mockCache.Verify(
        c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<Role>>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>()),
        Times.Once);
}
```

---

### 2. ClaimsService Tests

#### Test: GetAccountClaims_CombinesDirectAndRoleClaims
```csharp
[TestClass]
public class ClaimsServiceTests
{
    private Mock<IClaimsRepository> _mockClaimsRepo;
    private Mock<IRoleRepository> _mockRoleRepo;
    private Mock<IDistributedCache> _mockCache;
    private ClaimsService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockClaimsRepo = new Mock<IClaimsRepository>();
        _mockRoleRepo = new Mock<IRoleRepository>();
        _mockCache = new Mock<IDistributedCache>();

        _service = new ClaimsService(
            _mockClaimsRepo.Object,
            _mockRoleRepo.Object,
            _mockCache.Object,
            Mock.Of<ILogger<ClaimsService>>());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetAccountClaims_CombinesDirectAndRoleClaims()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var directClaims = new[]
        {
            new Claim("permission", "documents.write"),
            new Claim("department", "Engineering")
        };

        var roleClaims = new[]
        {
            new Claim("permission", "documents.read"),
            new Claim("role", "Employee")
        };

        var role = new Role { Id = roleId, Name = "Employee" };

        _mockCache
            .Setup(c => c.GetAsync<IEnumerable<Claim>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Claim>?)null);

        _mockClaimsRepo
            .Setup(r => r.GetAccountClaimsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(directClaims);

        _mockRoleRepo
            .Setup(r => r.GetAccountRolesAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { role });

        _mockClaimsRepo
            .Setup(r => r.GetRoleClaimsAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleClaims);

        // Test
        var result = (await _service.GetAccountClaimsAsync(accountId)).ToList();

        // Assert
        Assert.AreEqual(4, result.Count);
        Assert.IsTrue(result.Any(c => c.Type == "permission" && c.Value == "documents.write"));
        Assert.IsTrue(result.Any(c => c.Type == "permission" && c.Value == "documents.read"));
        Assert.IsTrue(result.Any(c => c.Type == "department" && c.Value == "Engineering"));
        Assert.IsTrue(result.Any(c => c.Type == "role" && c.Value == "Employee"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [ExpectedException(typeof(ClaimLimitException))]
    public async Task AddClaim_ExceedsLimit_ThrowsException()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var claim = new Claim("test", "value");

        _mockClaimsRepo
            .Setup(r => r.GetAccountClaimCountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(100); // At limit

        // Test
        await _service.AddClaimAsync(accountId, claim);

        // Assert via ExpectedException
    }
}
```

---

### 3. UserRightsService Tests

#### Test: HasPermission_DirectMatch_ReturnsTrue
```csharp
[TestClass]
public class UserRightsServiceTests
{
    private Mock<IClaimsService> _mockClaimsService;
    private Mock<IDistributedCache> _mockCache;
    private UserRightsService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockClaimsService = new Mock<IClaimsService>();
        _mockCache = new Mock<IDistributedCache>();

        _service = new UserRightsService(_mockClaimsService.Object, _mockCache.Object);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HasPermission_DirectMatch_ReturnsTrue()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var permission = "documents.read";

        var claims = new[]
        {
            new Claim("permission", "documents.read"),
            new Claim("permission", "documents.write")
        };

        _mockCache
            .Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _mockClaimsService
            .Setup(s => s.GetAccountClaimsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        // Test
        var result = await _service.HasPermissionAsync(accountId, permission);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HasPermission_WildcardMatch_ReturnsTrue()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var permission = "documents.delete";

        var claims = new[]
        {
            new Claim("permission", "documents.*") // Wildcard
        };

        _mockCache
            .Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _mockClaimsService
            .Setup(s => s.GetAccountClaimsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        // Test
        var result = await _service.HasPermissionAsync(accountId, permission);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HasAnyPermission_OneMatches_ReturnsTrue()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var permissions = new[] { "documents.read", "documents.write", "documents.delete" };

        var claims = new[]
        {
            new Claim("permission", "documents.read") // Only one matches
        };

        _mockCache
            .Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _mockClaimsService
            .Setup(s => s.GetAccountClaimsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        // Test
        var result = await _service.HasAnyPermissionAsync(accountId, permissions);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HasAllPermissions_AllMatch_ReturnsTrue()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var permissions = new[] { "documents.read", "documents.write" };

        var claims = new[]
        {
            new Claim("permission", "documents.read"),
            new Claim("permission", "documents.write")
        };

        _mockCache
            .Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _mockClaimsService
            .Setup(s => s.GetAccountClaimsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        // Test
        var result = await _service.HasAllPermissionsAsync(accountId, permissions);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HasAllPermissions_OneMissing_ReturnsFalse()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var permissions = new[] { "documents.read", "documents.write", "documents.delete" };

        var claims = new[]
        {
            new Claim("permission", "documents.read"),
            new Claim("permission", "documents.write")
            // Missing documents.delete
        };

        _mockCache
            .Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((bool?)null);

        _mockClaimsService
            .Setup(s => s.GetAccountClaimsAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        // Test
        var result = await _service.HasAllPermissionsAsync(accountId, permissions);

        // Assert
        Assert.IsFalse(result);
    }
}
```

---

## Integration Test Specifications

### 1. Role Assignment End-to-End

```csharp
[TestClass]
public class RoleAssignmentIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    private IRoleService _roleService;
    private IAccountService _accountService;

    [TestInitialize]
    public async Task Setup()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING");

        var services = new ServiceCollection();
        services.AddRoleClaimsManagement();
        services.AddAccountManagement();
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(connectionString));

        var provider = services.BuildServiceProvider();
        _roleService = provider.GetRequiredService<IRoleService>();
        _accountService = provider.GetRequiredService<IAccountService>();

        // Migrate database
        var dbContext = provider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task AssignRole_WithHierarchy_InheritsPermissions()
    {
        // Stage
        var employeeRole = await _roleService.CreateRoleAsync("Employee", "Basic access");
        var managerRole = await _roleService.CreateRoleAsync("Manager", "Management access");

        await _roleService.SetRoleHierarchyAsync("Manager", "Employee");

        var account = await _accountService.CreateAccountAsync(new CreateAccountRequest
        {
            Email = $"test-{Guid.NewGuid():N}@example.com",
            Password = "Test1234!@#$"
        });

        // Test
        await _roleService.AssignRoleAsync(account.Id, "Manager");

        // Assert
        var roles = (await _roleService.GetAccountRolesAsync(account.Id)).ToList();
        Assert.AreEqual(2, roles.Count);
        Assert.IsTrue(roles.Any(r => r.Name == "Manager"));
        Assert.IsTrue(roles.Any(r => r.Name == "Employee"));

        // Verify has both roles
        Assert.IsTrue(await _roleService.HasRoleAsync(account.Id, "Manager"));
        Assert.IsTrue(await _roleService.HasRoleAsync(account.Id, "Employee"));
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Clean up test data
    }
}
```

---

### 2. Claims Enhancement Pipeline

```csharp
[TestClass]
public class ClaimsEnhancementIntegrationTests
{
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task EnhanceClaims_MultipleEnhancers_AddsAllClaims()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddRoleClaimsManagement();
        services.AddClaimsEnhancer<AccountStatusClaimsEnhancer>();
        services.AddClaimsEnhancer<TenantMembershipClaimsEnhancer>();

        var provider = services.BuildServiceProvider();
        var enhancementService = provider.GetRequiredService<IClaimsEnhancementService>();

        var accountId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", accountId.ToString()),
            new Claim("email", "test@example.com")
        });
        var principal = new ClaimsPrincipal(identity);

        // Test
        var enhanced = await enhancementService.EnhanceClaimsAsync(principal);

        // Assert
        var claims = enhanced.Claims.ToList();
        Assert.IsTrue(claims.Any(c => c.Type == "account_status"));
        Assert.IsTrue(claims.Any(c => c.Type == "tenant"));
    }
}
```

---

## Performance Test Specifications

### 1. Permission Check Performance

```csharp
[TestClass]
public class PermissionCheckPerformanceTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HasPermission_CacheHit_CompletesUnder10ms()
    {
        // Stage
        var accountId = Guid.NewGuid();
        var permission = "documents.read";

        var mockClaimsService = new Mock<IClaimsService>();
        var mockCache = new Mock<IDistributedCache>();

        // Cache hit
        mockCache
            .Setup(c => c.GetAsync<bool?>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new UserRightsService(mockClaimsService.Object, mockCache.Object);

        // Test
        var stopwatch = Stopwatch.StartNew();
        var result = await service.HasPermissionAsync(accountId, permission);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10,
            $"Permission check took {stopwatch.ElapsedMilliseconds}ms (expected < 10ms)");
    }
}
```

---

### 2. Claims Enhancement Performance

```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
public async Task EnhanceClaims_AllEnhancers_CompletesUnder50ms()
{
    // Stage
    var enhancers = new List<IClaimsEnhancer>
    {
        new AccountStatusClaimsEnhancer(Mock.Of<IAccountRepository>()),
        new TenantMembershipClaimsEnhancer(Mock.Of<ITenantRepository>()),
        new FeatureFlagClaimsEnhancer(Mock.Of<IFeatureFlagService>())
    };

    var service = new ClaimsEnhancementService(
        enhancers,
        Mock.Of<ILogger<ClaimsEnhancementService>>());

    var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
        new Claim("sub", Guid.NewGuid().ToString())
    }));

    // Test
    var stopwatch = Stopwatch.StartNew();
    var enhanced = await service.EnhanceClaimsAsync(principal);
    stopwatch.Stop();

    // Assert
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50,
        $"Claims enhancement took {stopwatch.ElapsedMilliseconds}ms (expected < 50ms)");
}
```

---

## Test Data Builders

### Role Test Data Builder
```csharp
public class RoleBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = "TestRole";
    private string? _description;
    private Guid? _parentRoleId;
    private List<Claim> _claims = new();

    public RoleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public RoleBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public RoleBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public RoleBuilder WithParent(Guid parentRoleId)
    {
        _parentRoleId = parentRoleId;
        return this;
    }

    public RoleBuilder WithClaim(string type, string value)
    {
        _claims.Add(new Claim(type, value));
        return this;
    }

    public Role Build()
    {
        return new Role
        {
            Id = _id,
            Name = _name,
            Description = _description,
            ParentRoleId = _parentRoleId,
            CreatedAt = DateTime.UtcNow,
            Claims = _claims
        };
    }
}

// Usage:
var role = new RoleBuilder()
    .WithName("Manager")
    .WithDescription("Management role")
    .WithClaim("permission", "documents.read")
    .WithClaim("permission", "documents.write")
    .Build();
```

---

## Test Coverage Requirements

### By Component

| Component | Unit Coverage | Integration Coverage |
|-----------|---------------|---------------------|
| RoleService | 95%+ | 80%+ |
| ClaimsService | 95%+ | 80%+ |
| UserRightsService | 95%+ | 85%+ |
| ClaimsEnhancer | 90%+ | 75%+ |
| Repositories | 80%+ | 90%+ |

### Critical Paths (100% Coverage Required)

1. Role hierarchy resolution
2. Circular dependency detection
3. Permission checking (wildcard matching)
4. Claims aggregation (direct + role)
5. Cache invalidation

---

## Test Environment

### Docker Services (Integration Tests)
```yaml
# SQL Server for role/claim storage
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    ACCEPT_EULA: Y
    SA_PASSWORD: YourStrong@Passw0rd

# Redis for distributed caching
redis:
  image: redis:7-alpine
  ports:
    - "6379:6379"
```

### Test Configuration (.runsettings)
```xml
<RunSettings>
  <TestRunParameters>
    <Parameter name="SQLSERVER_CONNECTION_STRING" value="Server=localhost;Database=IdentityTest;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true" />
    <Parameter name="REDIS_CONNECTION_STRING" value="localhost:6379" />
  </TestRunParameters>
</RunSettings>
```

---

## Continuous Integration

### Test Execution Order

1. **Fast Unit Tests** (< 5 minutes)
   - Role service tests
   - Claims service tests
   - Permission checking tests

2. **Integration Tests** (< 10 minutes)
   - Database operations
   - Cache invalidation
   - End-to-end flows

3. **Performance Tests** (< 2 minutes)
   - Permission check latency
   - Claims enhancement timing

### Success Criteria

- ✅ All unit tests pass (85%+ coverage)
- ✅ All integration tests pass (80%+ coverage)
- ✅ Performance tests meet thresholds
- ✅ No critical code paths uncovered

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 07 Overview](../README.md)
