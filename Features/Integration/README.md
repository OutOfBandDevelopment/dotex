# Integration Testing Environment

**Status:** Planning Phase - Not Yet Implemented
**Last Updated:** 2026-01-16

## Overview

This folder contains planning materials and infrastructure code for setting up a comprehensive integration testing environment for OoBDev. Integration tests will validate the framework against real external dependencies and services.

## Structure

```
Features/Integration/
├── README.md (this file)
├── PLANNING.md - Implementation roadmap and strategy
├── DOCKER_SETUP.md - Docker container configuration guide
├── TEST_CATEGORIES.md - Integration test categories and scope
├── SERVICE_CONTAINERS.md - Database and service container definitions
├── GITHUB_ACTIONS.md - CI/CD integration testing workflow
├── EXAMPLES/
│   ├── docker-compose.yml - Complete example setup
│   ├── .env.example - Environment variables template
│   └── sample-integration-test.cs - Example integration test
└── WORKFLOWS/ (to be created)
    ├── docker-compose.yml - Production setup
    └── services/ - Individual service configs
```

## Quick Links & Reading Order

**Start Here:**
1. **[PLANNING.md](./PLANNING.md)** ⭐ - Overview, architecture, phases, timeline
2. **[TEST_CATEGORIES.md](./TEST_CATEGORIES.md)** - Decision tree for test categorization
3. **[DOCKER_SETUP.md](./DOCKER_SETUP.md)** - Container architecture with PlantUML diagrams

**Implementation Guides:**
4. **[SERVICE_CONTAINERS.md](./SERVICE_CONTAINERS.md)** - Detailed service specifications
5. **[GITHUB_ACTIONS.md](./GITHUB_ACTIONS.md)** - Daily scheduled execution workflow

**Examples:**
- **[EXAMPLES/](./EXAMPLES/)** - Sample docker-compose and test code (when available)

## Current State

### ✅ Done
- Unit tests (fast, isolated)
- Simulate tests (mocked integration)
- Test result publishing to PR
- GitVersion property passing

### ⏳ Planned (This Phase)
- Planning documents (IN PROGRESS)
- Docker Compose setup
- Service container definitions
- Integration test examples

### 🔮 Future
- Integration test implementation
- Database schema setup
- Service initialization
- CI/CD integration
- DevLocal test evaluation

## What Are Integration Tests?

Integration tests validate the framework with **real dependencies**:
- Real databases (SQL Server, PostgreSQL)
- Real file I/O and system operations
- Network communication
- Actual external services
- Cross-component interactions

Unlike `Simulate` tests which mock everything, Integration tests use the actual implementations.

## Why Do We Need Them?

✅ **Catch real-world issues** - Mocks can hide bugs
✅ **Validate schema/migrations** - Real database testing
✅ **Test third-party integrations** - Azure, AWS, etc.
✅ **Performance testing** - Real I/O characteristics
✅ **Regression prevention** - Comprehensive coverage

## Test Scope by Category

| Category | Environment | Speed | Coverage | When to Use |
|----------|-------------|-------|----------|------------|
| **Unit** | Isolated (no I/O) | ⚡ Fast | Single unit | Every change |
| **Simulate** | Mocked deps | ⚡ Fast | Component | Every change |
| **Integration** | Real services | 🐢 Slow | Cross-component | Main branch |
| **DevLocal** | Local dev only | ⏸️ Variable | Specific | Developer only |

## Docker in GitHub Actions

GitHub Actions runners have Docker pre-installed. For public projects, you get **unlimited free minutes**.

**Service Container Example:**
```yaml
services:
  postgres:
    image: postgres:15
    env:
      POSTGRES_PASSWORD: test
    options: >-
      --health-cmd pg_isready
      --health-interval 10s
      --health-timeout 5s
      --health-retries 5
```

No additional cost, no external services needed.

## Next Steps

When you're ready to implement:

1. **Review PLANNING.md** - Understand the strategy
2. **Review TEST_CATEGORIES.md** - Identify what needs testing
3. **Review DOCKER_SETUP.md** - Understand container setup
4. **Review EXAMPLES/** - See working examples
5. **Write first integration test** - Start with one component
6. **Create docker-compose.yml** - Service definitions
7. **Integrate with CI/CD** - Add to GitHub Actions

## Important Notes

### DevLocal Tests
Current code has `[TestCategory(TestCategories.DevLocal)]` tests. These are **developer-only** and should NOT run in CI. They typically require:
- Local file system access
- Machine-specific configuration
- Long-running operations
- Development tools installed

**Decision needed:** Review these tests before CI integration.

### Service Dependencies

**Likely needed based on OoBDev scope:**
- ✅ SQL Server / SQL Database (SQLCLR projects)
- ✅ PostgreSQL (extension projects)
- ✅ File system operations
- ✅ Network/HTTP testing
- ❓ Azure services (if ExternalServices layer tests included)
- ❓ AWS services (if cloud projects included)

### Performance Considerations

Integration tests are **slower** than Unit/Simulate:
- Database setup: 5-30 seconds
- Migration running: 5-60+ seconds
- Test execution: Variable
- Total time: 2-10 minutes typical

**Recommendation:** Run Integration tests only on `main` branch or nightly, not on every PR.

## Resources

### Docker for .NET Testing
- [Docker .NET SDK](https://hub.docker.com/_/microsoft-dotnet-sdk)
- [SQL Server in Docker](https://hub.docker.com/_/microsoft-mssql-server)
- [PostgreSQL in Docker](https://hub.docker.com/_/postgres)

### GitHub Actions
- [Service Containers](https://docs.github.com/en/actions/guides/about-service-containers)
- [Docker in Actions](https://docs.github.com/en/actions/publishing-packages/publishing-docker-images)

### Testing in .NET
- [Integration Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Testcontainers for .NET](https://testcontainers.com/modules/testcontainers-dotnet/)

## Questions for When You're Ready

When you start implementation, you'll need to answer:

1. **Which projects need integration tests?**
   - All Framework layer?
   - Specific domains (Database, IO, etc.)?
   - ExternalServices layer?

2. **What external services are required?**
   - Databases (which ones)?
   - Cloud services (Azure, AWS)?
   - Network/HTTP services?

3. **Test data strategy:**
   - Seed data in containers?
   - Temporary test databases?
   - Schema migration testing?

4. **When should they run?**
   - Every PR (slow, expensive)?
   - Only on main (recommended)?
   - Scheduled nightly?
   - Manual trigger only?

5. **DevLocal test evaluation:**
   - Which DevLocal tests are valuable?
   - Can any be converted to Integration tests?
   - Should they run in CI at all?

## Contact & Resources

- **Planning Documents:** See links above
- **GitHub Actions Docs:** https://docs.github.com/en/actions
- **Docker Hub:** https://hub.docker.com
- **.NET Testing:** https://docs.microsoft.com/en-us/dotnet/core/testing/
