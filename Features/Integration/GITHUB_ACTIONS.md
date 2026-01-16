# GitHub Actions Integration Testing Workflow

**Status:** Planning - Workflow Design and Integration Strategy
**Execution Model:** Once daily if changes detected
**Trigger Time:** 5 PM UTC (adjustable)

## Executive Summary

This document provides a complete GitHub Actions workflow design for running integration tests in OoBDev daily at 5 PM UTC (if changes detected). The setup includes three workflows: `dotnet.yml` (build + unit/simulate tests on every push/PR), `release.yml` (manual on-demand releases), and `integration-tests.yml` (daily integration testing with all Docker services). Integration tests run against real SQL Server, RabbitMQ, MongoDB, Qdrant, and OpenSearch databases/services in Docker containers. The workflow includes smart change detection (skip if no changes), comprehensive health checks, environment variable setup, test result publishing, codecov integration, and monitoring strategies. The total execution time is ~12-18 minutes with cost of $0 for public repositories (unlimited GitHub Actions minutes).

## Table of Contents

- [Integration Testing Workflow Architecture](#integration-testing-workflow-architecture)
- [Workflow File Structure](#workflow-file-structure)
- [Integration Tests Workflow Template](#integration-tests-workflow-template)
- [Execution Flow Timing](#execution-flow-timing)
- [Integration with Scheduled Release](#integration-with-scheduled-release)
- [Monitoring & Alerts](#monitoring--alerts)
- [Test Connection Strings](#test-connection-strings)
- [Cost Analysis (Public Repository)](#cost-analysis-public-repository)
- [Debugging Failed Tests](#debugging-failed-tests)
- [Next Steps When Ready to Implement](#next-steps-when-ready-to-implement)
- [Key Takeaways](#key-takeaways)

## Integration Testing Workflow Architecture

```plantuml
@startuml GitHubActionsIntegrationFlow
title OoBDev Integration Testing - GitHub Actions Workflow

participant "Scheduled Trigger\n(5 PM UTC)" as SCHED
participant "Change Detection" as DETECT
participant "Build Job" as BUILD
participant "Docker Setup" as DOCKER
participant "Test Job" as TEST
participant "Reporting" as REPORT
participant "Release" as RELEASE

SCHED ->> DETECT: Daily trigger
activate DETECT

DETECT ->> DETECT: git describe --tags
DETECT ->> DETECT: git rev-list since tag

alt Changes on main detected
  DETECT -->> BUILD: Proceed with build
  deactivate DETECT

  activate BUILD
  BUILD ->> BUILD: Restore dependencies
  BUILD ->> BUILD: Build solution
  BUILD ->> BUILD: Unit + Simulate tests
  BUILD -->> BUILD: ✅ Tests pass
  BUILD ->> BUILD: Package projects
  BUILD -->> BUILD: Upload artifacts
  deactivate BUILD

  BUILD ->> DOCKER: Start services
  activate DOCKER
  DOCKER ->> DOCKER: docker-compose up -d
  DOCKER ->> DOCKER: Wait for health checks
  DOCKER -->> TEST: Ready
  deactivate DOCKER

  activate TEST
  TEST ->> TEST: dotnet test --filter Integration
  TEST -->> TEST: Run integration tests
  TEST -->> REPORT: Results
  deactivate TEST

  activate REPORT
  REPORT ->> REPORT: Publish test results
  REPORT ->> REPORT: Create summary
  REPORT ->> REPORT: Check for failures
  deactivate REPORT

  alt All tests passed
    REPORT ->> RELEASE: Create GitHub Release
    RELEASE ->> RELEASE: Git tag
    RELEASE ->> RELEASE: Attach packages
    RELEASE -->> RELEASE: Notify on success
  else Tests failed
    REPORT -->> REPORT: Send failure notification
  end

else No changes detected
  DETECT ->> REPORT: Skip execution
  deactivate DETECT
  REPORT -->> REPORT: Log: "No changes"
end

@enduml
```

**View diagram:** Copy code above to https://www.plantuml.com/plantuml/uml/

---

## Workflow File Structure

```
.github/workflows/
├── dotnet.yml                    # Build + Unit + Simulate (every push/PR)
├── release.yml                   # Manual release (on-demand)
├── scheduled-release.yml         # Scheduled daily (main branch)
├── integration-tests.yml (NEW)  # Daily integration tests
└── README.md                     # Workflow documentation
```

---

## Integration Tests Workflow Template

Create: `.github/workflows/integration-tests.yml`

### Full Workflow Configuration

```yaml
name: Integration Tests

# Run daily at 5 PM UTC on main branch
on:
  schedule:
    - cron: '0 17 * * *'

  # Allow manual trigger for debugging
  workflow_dispatch:
    inputs:
      skip-change-check:
        description: 'Skip change detection (always run)'
        required: false
        default: 'false'

env:
  MSBUILDTERMINALLOGGER: off
  DOTNET_NOLOGO: true

jobs:
  # Check if there are changes since last release
  check-changes:
    runs-on: ubuntu-latest
    outputs:
      has-changes: ${{ steps.detect.outputs.has-changes }}
      last-version: ${{ steps.detect.outputs.last-version }}

    steps:
      - name: Checkout code
        uses: actions/checkout@v4.1.1
        with:
          fetch-depth: 0  # Full history for tag detection

      - name: Detect changes since last release
        id: detect
        run: |
          # Get last tag on main
          LAST_TAG=$(git describe --tags --abbrev=0 2>/dev/null || echo "none")
          echo "last-version=$LAST_TAG" >> $GITHUB_OUTPUT

          if [ "$LAST_TAG" = "none" ]; then
            # No releases yet
            echo "has-changes=true" >> $GITHUB_OUTPUT
            echo "✅ No previous releases - running integration tests"
          else
            # Count commits since last tag
            COMMITS=$(git rev-list --count $LAST_TAG..main 2>/dev/null || echo "0")

            if [ $COMMITS -gt 0 ]; then
              echo "has-changes=true" >> $GITHUB_OUTPUT
              echo "✅ Found $COMMITS commits since $LAST_TAG"
            else
              echo "has-changes=false" >> $GITHUB_OUTPUT
              echo "⏭️ No changes since $LAST_TAG - skipping tests"
            fi
          fi

      # Override for manual testing
      - name: Check manual override
        if: ${{ github.event.inputs.skip-change-check == 'true' }}
        run: |
          echo "🔨 Manual override: Running tests regardless of changes"

  # Build job (must pass before integration tests)
  build:
    if: needs.check-changes.outputs.has-changes == 'true' || github.event.inputs.skip-change-check == 'true'
    needs: check-changes
    runs-on: ubuntu-latest
    outputs:
      version: ${{ steps.version.outputs.FullSemVerLower }}
      packages-path: ${{ steps.version.outputs.PackagesPath }}

    steps:
      - name: Checkout code
        uses: actions/checkout@v4.1.1
        with:
          fetch-depth: 0

      - name: Setup .NET 10.0
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Install GitVersion
        uses: gittools/actions/gitversion/setup@v3.1.11
        with:
          versionSpec: '6.0.x'

      - name: Execute GitVersion
        uses: gittools/actions/gitversion/execute@v3.1.11
        id: gitversion
        with:
          useConfigFile: true
          configFilePath: GitVersion.yml

      - name: Calculate version
        id: version
        shell: bash
        run: |
          VERSION="${{ steps.gitversion.outputs.fullSemVer }}".ToLower()
          PACKAGES_PATH="${{ runner.temp }}/Packages"

          echo "FullSemVerLower=$VERSION" >> $GITHUB_OUTPUT
          echo "PackagesPath=$PACKAGES_PATH" >> $GITHUB_OUTPUT

      - name: Restore dependencies
        run: dotnet restore src/ --property:Version=${{ steps.version.outputs.FullSemVerLower }}

      - name: Build
        run: >
          dotnet build src/
          --configuration Release
          --property:Version=${{ steps.version.outputs.FullSemVerLower }}
          --no-restore
          --nologo

      - name: Unit tests
        run: >
          dotnet test src/
          --configuration Release
          --filter "TestCategory=Unit|TestCategory=Simulate"
          --no-build
          --nologo

      - name: Package
        run: >
          dotnet pack src/
          --configuration Release
          --property:Version=${{ steps.version.outputs.FullSemVerLower }}
          --no-build
          --output "${{ steps.version.outputs.PackagesPath }}"

  # Integration tests job
  integration-tests:
    if: needs.build.result == 'success'
    needs: [check-changes, build]
    runs-on: ubuntu-latest

    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2019-latest
        env:
          ACCEPT_EULA: Y
          MSSQL_SA_PASSWORD: L0c@lD3v
          MSSQL_PID: Developer
        options: >-
          --health-cmd "ACCEPT_EULA=Y /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P L0c@lD3v -Q 'SELECT 1'"
          --health-interval 10s
          --health-timeout 3s
          --health-retries 5
          --health-start-period 40s
        ports:
          - 1433:1433

      rabbitmq:
        image: rabbitmq:3.12-management
        env:
          RABBITMQ_DEFAULT_USER: guest
          RABBITMQ_DEFAULT_PASS: guest
        options: >-
          --health-cmd rabbitmq-diagnostics -q ping
          --health-interval 10s
          --health-timeout 3s
          --health-retries 5
          --health-start-period 30s
        ports:
          - 5672:5672
          - 15672:15672

      mongodb:
        image: mongo:7.0
        env:
          MONGO_INITDB_DATABASE: test_oodev
        options: >-
          --health-cmd "mongosh --eval 'db.adminCommand(\"ping\")'"
          --health-interval 10s
          --health-timeout 3s
          --health-retries 5
          --health-start-period 30s
        ports:
          - 27017:27017

      qdrant:
        image: qdrant/qdrant:latest
        options: >-
          --health-cmd "curl -f http://localhost:6333/health"
          --health-interval 10s
          --health-timeout 3s
          --health-retries 5
          --health-start-period 20s
        ports:
          - 6333:6333

      opensearch:
        image: opensearchproject/opensearch:2.0.0
        env:
          discovery.type: single-node
          OPENSEARCH_JAVA_OPTS: "-Xms512m -Xmx512m"
          DISABLE_SECURITY_PLUGIN: "true"
        options: >-
          --health-cmd "curl -f http://localhost:9200/_cluster/health"
          --health-interval 10s
          --health-timeout 3s
          --health-retries 5
          --health-start-period 30s
        ports:
          - 9200:9200

    steps:
      - name: Checkout code
        uses: actions/checkout@v4.1.1

      - name: Setup .NET 10.0
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Wait for SQL Server
        run: |
          for i in {1..30}; do
            /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P L0c@lD3v -Q "SELECT 1" && break
            echo "Waiting for SQL Server... ($i/30)"
            sleep 1
          done

      - name: Initialize test databases
        run: |
          sqlcmd -S localhost -U sa -P L0c@lD3v -Q "
            CREATE DATABASE [VectorDb];
            CREATE DATABASE [ExampleDb];
            CREATE DATABASE [test_integration];
            ALTER DATABASE [VectorDb] SET ENABLE_BROKER;
            ALTER DATABASE [ExampleDb] SET ENABLE_BROKER;
          "

      - name: Restore dependencies
        run: dotnet restore src/

      - name: Run integration tests
        run: >
          dotnet test src/
          --configuration Release
          --filter "TestCategory=Integration"
          --no-restore
          --no-build
          --logger "trx;LogFileName=test-results-integration.trx"
          --collect:"XPlat Code Coverage"
        env:
          # Connection strings for tests
          ConnectionStrings__SqlServer: "Server=localhost,1433;User Id=sa;Password=L0c@lD3v;TrustServerCertificate=True"
          ConnectionStrings__MongoDB: "mongodb://localhost:27017/test_oodev"
          ConnectionStrings__RabbitMQ: "amqp://guest:guest@localhost:5672/"
          Qdrant__Uri: "http://localhost:6333"
          OpenSearch__Uri: "http://localhost:9200"

      - name: Publish test results
        if: always()
        uses: EnricoMi/publish-unit-test-result-action/composite@v2
        with:
          trx_files: ${{ runner.temp }}/**/*.trx
          report_individual_runs: true

      - name: Upload coverage
        if: always()
        uses: codecov/codecov-action@v3
        with:
          files: ${{ runner.temp }}/coverage.cobertura.xml

  # Summary job
  integration-summary:
    if: always()
    needs: [check-changes, integration-tests]
    runs-on: ubuntu-latest

    steps:
      - name: Integration tests summary
        run: |
          echo "## Integration Tests Summary"
          echo ""
          echo "Last version: ${{ needs.check-changes.outputs.last-version }}"
          echo "Changes detected: ${{ needs.check-changes.outputs.has-changes }}"
          echo ""
          if [ "${{ needs.integration-tests.result }}" = "success" ]; then
            echo "✅ All integration tests passed"
          elif [ "${{ needs.integration-tests.result }}" = "failure" ]; then
            echo "❌ Integration tests failed"
            exit 1
          else
            echo "⏭️ Integration tests skipped"
          fi
```

**View diagram:** Copy code above to https://www.plantuml.com/plantuml/uml/

---

## Execution Flow Timing

```plantuml
@startuml ExecutionTiming
skinparam backgroundColor #FEFEFE

rectangle "5:00 PM UTC\ncheck-changes" as FIVE #E8F4F8 {
  card "Action" as F1
  card "Check git tags" as F2
  card "Count commits" as F3
  card "~40 seconds" as F4
}

rectangle "5:01 PM UTC\nbuild (parallel)" as ONE #FFE8CC {
  card "Restore deps" as O1
  card "Build solution" as O2
  card "Unit + Simulate tests" as O3
  card "~5-7 minutes" as O4
}

rectangle "5:07 PM UTC\nintegration-tests (parallel)" as SEVEN #E8F8E8 {
  card "Wait for services" as S1
  card "Init databases" as S2
  card "Run integration tests" as S3
  card "~5-10 minutes" as S4
}

rectangle "5:17 PM UTC\nintegration-summary" as SEVENTEEN #F0E8FF {
  card "Report results" as SE1
  card "Trigger release" as SE2
  card "~1 minute" as SE3
}

FIVE --> ONE
ONE --> SEVEN
SEVEN --> SEVENTEEN

note right of ONE
  Services starting in parallel
  with test execution
end note

@enduml
```

**View diagram:** Copy code above to https://www.plantuml.com/plantuml/uml/

---

## Integration with Scheduled Release

Modify `scheduled-release.yml` to depend on integration tests:

```yaml
name: Scheduled Daily Release

on:
  schedule:
    - cron: '0 17 * * *'  # 5 PM UTC (same as integration tests)

  workflow_dispatch:

jobs:
  # Integration tests must pass first
  integration-tests:
    uses: ./.github/workflows/integration-tests.yml

  release:
    needs: integration-tests
    if: needs.integration-tests.outputs.has-changes == 'true'
    runs-on: ubuntu-latest
    # ... rest of release job
```

---

## Monitoring & Alerts

### Build Status Check

```yaml
- name: Check build status
  if: failure()
  run: |
    echo "❌ Build or tests failed"
    exit 1
```

### Slack/Email Notifications (Optional)

```yaml
- name: Notify on failure
  if: failure()
  uses: 8398a7/action-slack@v3
  with:
    status: ${{ job.status }}
    text: 'Integration tests failed'
    webhook_url: ${{ secrets.SLACK_WEBHOOK }}
```

### GitHub Issues

```yaml
- name: Create issue on failure
  if: failure()
  uses: actions/github-script@v7
  with:
    script: |
      github.rest.issues.create({
        owner: context.repo.owner,
        repo: context.repo.repo,
        title: `Integration tests failed - ${new Date().toISOString()}`,
        body: 'Check workflow logs for details'
      })
```

---

## Test Connection Strings

Tests should use environment variables for connections:

```csharp
// appsettings.Integration.json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,1433;User Id=sa;Password=L0c@lD3v;TrustServerCertificate=True",
    "MongoDB": "mongodb://localhost:27017/test_oodev",
    "RabbitMQ": "amqp://guest:guest@localhost:5672/",
    "Qdrant": "http://localhost:6333",
    "OpenSearch": "http://localhost:9200"
  }
}

// C# test fixture
public class IntegrationTestFixture
{
    public string SqlConnectionString { get; } =
        Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer") ??
        "Server=localhost,1433;User Id=sa;Password=L0c@lD3v;TrustServerCertificate=True";

    public string MongoDbConnectionString { get; } =
        Environment.GetEnvironmentVariable("ConnectionStrings__MongoDB") ??
        "mongodb://localhost:27017/test_oodev";
}
```

---

## Cost Analysis (Public Repository)

```plantuml
@startuml CostAnalysis
!theme plain
title Integration Testing Cost for Public Repository

rectangle "GitHub Actions" as GA #E8F4F8 {
  card "Unlimited Linux/macOS" as GH1
  card "Free for public repos" as GH2
  card "2000 min/month Windows\n(not used)" as GH3
}

rectangle "Execution Frequency" as EF #E8F0F8 {
  card "Daily at 5 PM UTC" as EF1
  card "Smart skip if no changes" as EF2
  card "~20-25 days/month with changes" as EF3
}

rectangle "Time per Execution" as TE #E8F4E8 {
  card "Change detection\n~40 seconds" as TE1
  card "Build job\n~5-7 minutes" as TE2
  card "Integration tests\n~5-10 minutes" as TE3
  card "Total\n~12-18 minutes" as TE4
}

rectangle "Monthly Cost" as MC #F8E8E8 {
  card "20 days × 15 min\n= 300 minutes" as MC1
  card "Divided by 60\n= 5 hours" as MC2
  card "Public repo\n= $0" as MC3
}

note bottom of MC
  Free for public open-source repositories!
  Can run comprehensive tests without cost
end note

@enduml
```

**View diagram:** Copy code above to https://www.plantuml.com/plantuml/uml/

---

## Debugging Failed Tests

### Access to Service Logs

```yaml
- name: Collect service logs on failure
  if: failure()
  run: |
    echo "=== SQL Server Logs ==="
    docker logs $(docker ps --filter ancestor=mcr.microsoft.com/mssql/server -q) || true

    echo "=== RabbitMQ Logs ==="
    docker logs $(docker ps --filter ancestor=rabbitmq:3.12-management -q) || true

    echo "=== MongoDB Logs ==="
    docker logs $(docker ps --filter ancestor=mongo:7.0 -q) || true
```

### Service Health Status

```yaml
- name: Check service health
  if: failure()
  run: |
    echo "=== Network Status ==="
    docker network ls

    echo "=== Running Containers ==="
    docker ps -a

    echo "=== Service Connectivity ==="
    docker exec -i $(docker ps --filter ancestor=mcr.microsoft.com/mssql/server -q) \
      sqlcmd -S localhost -U sa -P L0c@lD3v -Q "SELECT @@VERSION"
```

---

## Next Steps When Ready to Implement

1. **Create integration-tests.yml workflow**
   - Copy template above
   - Adjust service versions if needed
   - Set up environment variables

2. **Update scheduled-release.yml**
   - Add integration tests as prerequisite
   - Only release if tests pass

3. **Create test projects**
   - Add Integration test category
   - Write first integration tests
   - Validate against real services

4. **Monitor first few runs**
   - Check timing
   - Verify service health
   - Adjust resource constraints

5. **Optimize based on results**
   - Parallel test execution
   - Service startup optimization
   - Coverage improvements

---

## Key Takeaways

✅ **Free for public repositories** - Unlimited minutes
✅ **Smart execution** - Skip if no changes
✅ **Daily rhythm** - Consistent release window
✅ **Comprehensive testing** - Real database operations
✅ **Easy debugging** - Full access to logs and services

The infrastructure is ready. When you're ready to write integration tests, the workflow will support them!
