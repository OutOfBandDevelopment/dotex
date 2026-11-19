<#
.SYNOPSIS
    Starts the slow integration services in the background so they initialise while the solution builds.

.DESCRIPTION
    A bare `dotnet test OoBDev.slnx --filter TestCategory=Integration` with the stack down starts each container on demand
    (IntegrationTestMethod), so the first test of pgvector, SQL Server and the Ollama projects waits 1 to 2.5 minutes for its
    container while the other projects compete for the CPU. Run this script first: it pulls the images and runs
    `docker compose up -d` (no --wait) for the services that start slowly, then returns; the build runs while they come up, and
    the harness finds them healthy. Containers started here are not stopped by the tests (they were running already), so end
    the run with containers/testing/scripts/integration-down.sh.

    Not needed with scripts/ci/run-tests.ps1 -Category Integration, which starts the whole stack with --wait before the build.

.PARAMETER Service
    Compose services to warm up; the default is the set that dominated the 2026-11-19 timing (docs/architecture/testing/integration/suite-timing.md).

.PARAMETER SkipPull
    Do not pull the images first (use when they are known to be current).

.PARAMETER DryRun
    Check the service names against the compose file and print the commands without running them.

.EXAMPLE
    pwsh scripts/ci/warm-up-integration.ps1; dotnet build src/OoBDev.slnx; dotnet test src/OoBDev.slnx --filter TestCategory=Integration
#>
[CmdletBinding()]
param(
    [string[]] $Service = @('pgvector', 'sql-server', 'ollama', 'sbert', 'keycloak', 'opensearch', 'neo4j', 'rabbitmq', 'docling'),
    [switch] $SkipPull,
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$compose = Join-Path $root 'containers\testing\docker-compose.integration-tests.yml'
$envFile = Join-Path $root 'containers\testing\.env.integration'
$base = @('compose', '-f', $compose)
if (Test-Path $envFile) { $base += @('--env-file', $envFile) }

$known = & docker @base config --services
if ($LASTEXITCODE -ne 0) { throw 'docker compose config failed' }
$unknown = $Service | Where-Object { $_ -notin $known }
if ($unknown) { throw "Not a service of the integration compose file: $($unknown -join ', ')" }

$steps = @()
if (-not $SkipPull) { $steps += , ($base + @('pull', '--ignore-pull-failures', '--quiet') + $Service) }
$steps += , ($base + @('up', '-d') + $Service)

foreach ($step in $steps) {
    Write-Host ('docker ' + ($step -join ' ')) -ForegroundColor Cyan
    if ($DryRun) { continue }
    & docker @step
    if ($LASTEXITCODE -ne 0) { throw "docker $($step[3]) failed with exit code $LASTEXITCODE" }
}
