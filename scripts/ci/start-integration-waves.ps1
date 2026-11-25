<#
.SYNOPSIS
    Starts the integration stack in small waves, waiting for each wave to be healthy before the next one starts.

.DESCRIPTION
    Starting about 15 containers at once starves the host and makes a cold whole-solution Integration run fail tests that pass alone
    (docs/issues/cold-stack-integration-run.md). This script starts the services in waves (heaviest first, then the light ones)
    with `docker compose up -d --wait`, so no wave competes with the next. Services of a wave that the compose file does not define
    are skipped with a note. Stop the stack with containers/testing/scripts/integration-down.sh.

.PARAMETER Wave
    Waves of compose service names; override to change the order.

.PARAMETER OneShot
    Services that run once and exit (started after the waves with `up -d`, no `--wait`, which would count their exit as a failure).

.PARAMETER DryRun
    Print the commands without running them.

.EXAMPLE
    pwsh scripts/ci/start-integration-waves.ps1; dotnet test src/OoBDev.slnx --filter TestCategory=Integration
#>
[CmdletBinding()]
param(
    [string[][]] $Wave = @(
        , @('sql-server', 'pgvector', 'opensearch')
        , @('keycloak', 'ollama', 'sbert', 'docling')
        , @('rabbitmq', 'mongodb', 'neo4j', 'redis', 'postgres', 'qdrant')
        , @('apache-tika', 'smtp4dev', 'clamav', 'mosquitto', 'azurite', 'moto', 'servicebus-sql', 'servicebus-emulator', 'eventhubs-emulator', 'otel-dashboard', 'ser2net', 'loopback-device', 'nginx')
    ),
    [string[]] $OneShot = @('moto-init'),
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

$index = 0
foreach ($names in $Wave) {
    $index++
    $present = @($names | Where-Object { $_ -in $known })
    $missing = @($names | Where-Object { $_ -notin $known })
    if ($missing) { Write-Host "wave ${index}: not in the compose file, skipped: $($missing -join ', ')" -ForegroundColor Yellow }
    if (-not $present) { continue }

    $step = $base + @('up', '-d', '--wait') + $present
    Write-Host ("wave ${index}: docker " + ($step -join ' ')) -ForegroundColor Cyan
    if ($DryRun) { continue }
    & docker @step
    if ($LASTEXITCODE -ne 0) { throw "wave $index failed with exit code $LASTEXITCODE" }
}

$oneShot = @($OneShot | Where-Object { $_ -in $known })
if ($oneShot) {
    $step = $base + @('up', '-d') + $oneShot
    Write-Host ('one-shot: docker ' + ($step -join ' ')) -ForegroundColor Cyan
    if (-not $DryRun) {
        & docker @step
        if ($LASTEXITCODE -ne 0) { throw "one-shot start failed with exit code $LASTEXITCODE" }
    }
}
