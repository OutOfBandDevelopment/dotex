# GitHub Actions Integration Testing Workflow

**Status:** Planning - Workflow Design and Integration Strategy
**Execution Model:** Once daily if changes detected
**Trigger Time:** 5 PM UTC (adjustable)

## Executive Summary

This document provides a complete GitHub Actions workflow design for running integration tests in OoBDev daily at 5 PM UTC (if changes detected). The setup includes three workflows in a **three-stage pipeline**: (1) `dotnet.yml` builds + unit/simulate tests on every push/PR, (2) `integration-tests.yml` gates releases by running integration tests against real Docker services (SQL Server, RabbitMQ, MongoDB, Qdrant, OpenSearch), (3) `scheduled-release.yml` only runs if integration tests pass, creating releases and publishing to NuGet. **Integration tests are injected between build and release as a mandatory validation gate** — if tests fail, releases are blocked and no tags are created. Includes smart change detection (skip if no changes), comprehensive health checks, detailed logging and tracing reports (Docker logs, test execution logs), code coverage collection and reporting (integrated with Codecov), and semantic version tagging: `validated-vX.Y.Z` (integration tests pass) and `release-vX.Y.Z` (published to NuGet). Total execution time ~12-18 minutes, cost $0 for public repositories (unlimited GitHub Actions minutes).

## Table of Contents

- [Integration Testing Workflow Architecture](#integration-testing-workflow-architecture)
- [Workflow File Structure](#workflow-file-structure)
- [Integration Tests Workflow Template](#integration-tests-workflow-template)
- [Execution Flow Timing](#execution-flow-timing)
- [Integration with Scheduled Release](#integration-with-scheduled-release)
- [Monitoring & Alerts](#monitoring--alerts)
- [Logging & Tracing Reports](#logging--tracing-reports)
- [Code Coverage Reports](#code-coverage-reports)
- [Release Tagging Strategy](#release-tagging-strategy)
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

## Workflow File Structure & Pipeline Order

```
.github/workflows/
├── dotnet.yml                    # [STAGE 1] Build + Unit + Simulate tests
├── integration-tests.yml         # [STAGE 2] Integration tests (gate before release)
├── scheduled-release.yml         # [STAGE 3] Release creation + NuGet publish
├── release.yml                   # [STAGE 3-ALT] Manual release (on-demand)
└── README.md                     # Workflow documentation
```

### Pipeline Flow

**Every Push/PR:**
```
dotnet.yml (all branches)
  ├─ Restore
  ├─ Build
  ├─ Unit + Simulate Tests
  └─ Package (upload as artifacts)
  [STOP - no integration tests or release on non-main branches]
```

**Daily on Main Branch (5 PM UTC):**
```
dotnet.yml (main only)
  ├─ Restore
  ├─ Build
  ├─ Unit + Simulate Tests
  └─ Package (upload as artifacts)
    │
    ▼
integration-tests.yml (GATE - blocks release if fails)
  ├─ Detect changes (skip if no commits since last validated tag)
  ├─ Start Docker services (SQL Server, RabbitMQ, MongoDB, etc.)
  ├─ Run Integration Tests (--filter TestCategory=Integration)
  ├─ Publish test results (TRX, JSON)
  ├─ Collect coverage reports
  ├─ Create validated-vX.Y.Z tag (only if ALL tests pass)
  └─ Upload artifacts (logs, coverage reports)
    │
    ├─ [IF TESTS FAIL] → Stop, do NOT proceed to release
    │
    └─ [IF TESTS PASS] → Continue
        │
        ▼
scheduled-release.yml (only runs if integration tests passed)
  ├─ Create GitHub Release with validated-vX.Y.Z tag
  ├─ Attach .nupkg files
  ├─ (Optional) Publish to NuGet (requires approval)
  └─ Create release-vX.Y.Z tag (only after NuGet publish)
```

**Manual Release (On-Demand):**
```
release.yml (manual workflow_dispatch)
  ├─ Select packages from latest build artifacts
  └─ Create release (requires approval for NuGet publish)
  [Integration tests already passed on main, safe to release]
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

## Integration Tests as a Release Gate

Integration tests are **injected between build and release** as a mandatory validation gate.

### Gating Mechanism

**scheduled-release.yml depends on integration-tests.yml:**

```yaml
name: Scheduled Daily Release

on:
  schedule:
    - cron: '0 17 * * *'  # 5 PM UTC

  workflow_dispatch:

jobs:
  # STAGE 2: Integration tests MUST pass before proceeding
  integration-tests:
    uses: ./.github/workflows/integration-tests.yml
    # Outputs:
    #   - has-changes: true|false (skip release if false)
    #   - validated-tag: validated-vX.Y.Z (only if tests pass)

  # STAGE 3: Release only happens if integration tests pass
  release:
    needs: integration-tests
    # GATE: Only proceed if tests passed
    if: |
      success() &&
      needs.integration-tests.result == 'success' &&
      needs.integration-tests.outputs.has-changes == 'true'
    runs-on: ubuntu-latest

    steps:
      # Download already-built packages from dotnet.yml
      - uses: actions/download-artifact@v3
        with:
          name: packages

      # Create GitHub Release with validated tag
      - uses: softprops/action-gh-release@v1
        with:
          tag_name: ${{ needs.integration-tests.outputs.validated-tag }}
          files: '*.nupkg'

      # Optional: Publish to NuGet (approval gate)
      - name: Publish to NuGet
        if: github.event.inputs.publish-nuget == 'true'
        run: |
          dotnet nuget push **/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json \
            --skip-duplicate

          # Create release tag only after successful NuGet publish
          git tag -a "release-${{ env.VERSION }}" \
            -m "Released to NuGet"
          git push origin "release-${{ env.VERSION }}"
```

### What Happens If Integration Tests Fail

**Block Release:**
```
dotnet.yml ✅ passes
  ↓
integration-tests.yml ❌ FAILS
  ↓
scheduled-release.yml 🚫 BLOCKED (does not run)
  ↓
Result: No release, no tags, no NuGet publish
```

**Notification:**
- GitHub Actions shows failure in scheduled-release workflow
- Integration test report visible in GitHub Actions UI
- Logs, service logs, and coverage reports available as artifacts
- (Optional) Slack/email notification of failure

### What Happens If Integration Tests Pass

**Allow Release:**
```
dotnet.yml ✅ passes
  ↓
integration-tests.yml ✅ PASSES
  ↓
scheduled-release.yml ✅ PROCEEDS
  ├─ Create GitHub Release (validated-vX.Y.Z)
  ├─ Attach .nupkg files
  └─ (Optional) Publish to NuGet → release-vX.Y.Z
```

**Automatic Tagging:**
- `validated-vX.Y.Z` - Created by integration-tests.yml when tests pass
- `release-vX.Y.Z` - Created by scheduled-release.yml after NuGet publish (if enabled)

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

## Logging & Tracing Reports

Integration tests generate detailed logs and traces for debugging and audit trails.

### Docker Container Logs

**Capture service logs on test failure:**
```yaml
- name: Collect service logs on failure
  if: failure()
  run: |
    mkdir -p test-logs

    echo "=== SQL Server Logs ===" > test-logs/sqlserver.log
    docker logs $(docker ps -a --filter ancestor=mcr.microsoft.com/mssql/server -q) >> test-logs/sqlserver.log 2>&1 || true

    echo "=== RabbitMQ Logs ===" > test-logs/rabbitmq.log
    docker logs $(docker ps -a --filter ancestor=rabbitmq:3.12 -q) >> test-logs/rabbitmq.log 2>&1 || true

    echo "=== MongoDB Logs ===" > test-logs/mongodb.log
    docker logs $(docker ps -a --filter ancestor=mongo:7.0 -q) >> test-logs/mongodb.log 2>&1 || true

    echo "=== OpenSearch Logs ===" > test-logs/opensearch.log
    docker logs $(docker ps -a --filter ancestor=opensearchproject/opensearch -q) >> test-logs/opensearch.log 2>&1 || true

    echo "=== Qdrant Logs ===" > test-logs/qdrant.log
    docker logs $(docker ps -a --filter ancestor=qdrant/qdrant -q) >> test-logs/qdrant.log 2>&1 || true

- name: Upload service logs
  if: failure()
  uses: actions/upload-artifact@v3
  with:
    name: service-logs-${{ github.run_id }}
    path: test-logs/
    retention-days: 30
```

### Test Execution Logs

**Structured test logging via MSTest:**
```yaml
- name: Run integration tests with detailed logging
  run: >
    dotnet test src/
    --configuration Release
    --filter "TestCategory=Integration"
    --logger "console;verbosity=detailed"
    --logger "trx;LogFileName=${{ runner.temp }}/test-results-integration.trx"
    --logger "json;LogFileName=${{ runner.temp }}/test-results-integration.json"
    --collect:"XPlat Code Coverage"
  env:
    DOTNET_CLI_TELEMETRY_OPTOUT: true
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
```

**Test result artifacts:**
- TRX format (MSTest native) - Parseable by test result reporters
- JSON format - Machine readable, detailed test info
- Code coverage data - Detailed line-by-line coverage

---

## Code Coverage Reports

Integration tests contribute to overall code coverage metrics.

### Coverage Collection

**Enable coverage during test execution:**
```yaml
- name: Run integration tests with coverage
  run: >
    dotnet test src/
    --configuration Release
    --filter "TestCategory=Integration"
    --logger "trx;LogFileName=${{ runner.temp }}/test-results-integration.trx"
    --collect:"XPlat Code Coverage"
    --settings .runsettings
  env:
    COVERAGE_OPTS: /p:CollectCoverage=true /p:CoverageFormat=opencover /p:CoverageFilePath=${{ runner.temp }}/coverage-integration.xml
```

### Coverage Reports

**Generate and upload coverage reports:**
```yaml
- name: Merge coverage reports
  run: |
    # Merge Unit + Simulate + Integration coverage
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator \
      -reports:"${{ runner.temp }}/**/coverage*.xml" \
      -targetdir:"${{ runner.temp }}/coverage-report" \
      -reporttypes:"HtmlInline;Cobertura;Badges"

- name: Publish to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ${{ runner.temp }}/coverage-integration.xml
    flags: integration
    name: Integration Tests Coverage
    fail_ci_if_error: false

- name: Upload HTML coverage report
  if: always()
  uses: actions/upload-artifact@v3
  with:
    name: coverage-report-${{ github.run_id }}
    path: ${{ runner.temp }}/coverage-report/
    retention-days: 90
```

### Coverage Badges

**Display coverage in README:**
```markdown
![Integration Tests Coverage](https://codecov.io/gh/user/repo/branch/main/graph/badge.svg?token=ABC123)
```

---

## Release Tagging Strategy

Implement semantic versioning with integration test validation.

### Tag Naming Convention

**Two-tier tagging approach:**

| Tag Pattern | Trigger | Meaning |
|------------|---------|---------|
| `validated-vX.Y.Z` | Integration tests ✅ pass on main | Code validated, ready for release |
| `release-vX.Y.Z` | Manual approval + Publish to NuGet ✅ | Published to NuGet, production-ready |

**Example:**
```
validated-v2.1.0    ← All integration tests passed on main branch
↓
release-v2.1.0      ← Released to NuGet (manual approval)
```

### Implementation

**Create validated tag on test success:**
```yaml
- name: Create validated tag
  if: success()  # Only if integration tests pass
  run: |
    VERSION=$(cat src/GitVersion.yml | grep 'full-semver:' | cut -d' ' -f2)
    git config user.name "github-actions[bot]"
    git config user.email "github-actions[bot]@users.noreply.github.com"
    git tag -a "validated-${VERSION}" -m "Integration tests passed for ${VERSION}"
    git push origin "validated-${VERSION}"
```

**Create release tag on NuGet publish:**
```yaml
- name: Create release tag
  if: success() && github.event_name == 'workflow_dispatch'
  run: |
    VERSION=$(cat src/GitVersion.yml | grep 'full-semver:' | cut -d' ' -f2)
    git config user.name "github-actions[bot]"
    git config user.email "github-actions[bot]@users.noreply.github.com"
    git tag -a "release-${VERSION}" -m "Released ${VERSION} to NuGet"
    git push origin "release-${VERSION}"
```

### Tag Creation Workflow

**Only validated-vX.Y.Z tags are created automatically (by integration-tests.yml):**
```yaml
# In integration-tests.yml (STAGE 2)
- name: Create validated tag (only if tests pass)
  if: success()
  run: |
    VERSION=$(cat src/GitVersion.yml | grep 'full-semver:' | cut -d' ' -f2)
    git config user.name "github-actions[bot]"
    git config user.email "github-actions[bot]@users.noreply.github.com"
    git tag -a "validated-${VERSION}" \
      -m "Integration tests passed - code validated for release"
    git push origin "validated-${VERSION}"
```

**release-vX.Y.Z tags are created only after NuGet publish (by scheduled-release.yml):**
```yaml
# In scheduled-release.yml (STAGE 3)
# Only runs if integration tests passed
- name: Publish to NuGet
  run: dotnet nuget push **/*.nupkg ...

- name: Create release tag (only after successful NuGet publish)
  run: |
    VERSION=$(cat src/GitVersion.yml | grep 'full-semver:' | cut -d' ' -f2)
    git tag -a "release-${VERSION}" \
      -m "Released ${VERSION} to NuGet"
    git push origin "release-${VERSION}"
```

### Tag Visibility in GitHub

**GitHub Release for validated-vX.Y.Z:**
```yaml
- name: Create GitHub Release (after integration tests pass)
  if: success()
  uses: softprops/action-gh-release@v1
  with:
    tag_name: validated-v${{ env.VERSION }}
    draft: false
    prerelease: false
    name: "Validated v${{ env.VERSION }}"
    body: |
      # Integration Tests Validated
      - All integration tests passed ✅
      - Ready for release
      - See artifacts for test results and coverage
    files: |
      ${{ runner.temp }}/packages/*.nupkg
      test-results-integration.trx
      coverage-report.html
```

### Tag Conditions Summary

| Tag | Condition | Created By | Can Release |
|-----|-----------|------------|------------|
| `validated-vX.Y.Z` | ✅ Integration tests pass | integration-tests.yml | Yes (validated) |
| ❌ Tag missing | ❌ Integration tests fail | (NOT created) | No (blocked) |
| `release-vX.Y.Z` | ✅ NuGet publish succeeds | scheduled-release.yml | Yes (released) |

**Key Point:** Without the `validated-vX.Y.Z` tag, no release is created. The tag itself serves as proof that integration tests have validated the code.

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
