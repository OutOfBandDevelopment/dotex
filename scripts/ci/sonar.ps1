<#
.SYNOPSIS
    Scans src/ with SonarQube Community (the stack service in containers/testing) using dotnet-sonarscanner.

.DESCRIPTION
    Begin, build, test with coverage (OpenCover), end. The token is read from the SONAR_TOKEN environment
    variable (a user token created in the SonarQube UI); it is never written to a file. The server address
    comes from SONARQUBE_URL, then TEST_PORT_SONARQUBE, then http://localhost:9002.
    Without -Gate the scan only uploads results. With -Gate the script waits for the quality gate and exits
    non-zero when it fails (the gate is switchable, owner 2026-11-16).

.EXAMPLE
    $env:SONAR_TOKEN = '<token>'; pwsh scripts/ci/sonar.ps1
    pwsh scripts/ci/sonar.ps1 -Gate
    pwsh scripts/ci/sonar.ps1 -SkipTests     # analysis without coverage
#>
[CmdletBinding()]
param(
    [string] $ProjectKey = 'oobdev-dotex',
    [string] $Category = 'Unit',
    [switch] $Gate,
    [switch] $SkipTests
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$src = Join-Path $root 'src'

$token = $env:SONAR_TOKEN
if (-not $token) { throw 'Set SONAR_TOKEN to a SonarQube user token (create it under My Account > Security). It is never stored in the repository.' }

$url = $env:SONARQUBE_URL
if (-not $url) {
    $port = if ($env:TEST_PORT_SONARQUBE) { $env:TEST_PORT_SONARQUBE } else { '9002' }
    $url = "http://localhost:$port"
}

try {
    $status = Invoke-RestMethod "$url/api/system/status" -TimeoutSec 10
    if ($status.status -ne 'UP') { throw "SonarQube reports '$($status.status)'" }
}
catch {
    throw "SonarQube is not reachable at $url ($($_.Exception.Message)). Start it: containers/testing/scripts/integration-up.bat --wait"
}

function Invoke-Step([string] $Name, [scriptblock] $Action) {
    Write-Host "==== $Name" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) { throw "$Name failed with exit code $LASTEXITCODE" }
}

Push-Location $src
try {
    dotnet tool restore | Out-Null

    $begin = @('dotnet-sonarscanner', 'begin', "/k:$ProjectKey", "/d:sonar.host.url=$url", "/d:sonar.token=$token",
        '/d:sonar.cs.opencover.reportsPaths=**/TestResults/**/coverage.opencover.xml',
        '/d:sonar.exclusions=**/obj/**,**/bin/**,**/*.Designer.cs')
    if ($Gate) { $begin += '/d:sonar.qualitygate.wait=true' }
    Invoke-Step 'Sonar begin' { dotnet @begin }

    $failed = $null
    try {
        Invoke-Step 'Build' { dotnet build OoBDev.slnx --configuration Release --nologo -m:1 }
        if (-not $SkipTests) {
            Invoke-Step 'Test with coverage' {
                dotnet test OoBDev.slnx --configuration Release --no-build --nologo --filter "TestCategory=$Category" `
                    --collect:'XPlat Code Coverage' -- 'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover'
            }
        }
    }
    catch { $failed = $_ }

    # End always runs so the scanner leaves no half-open analysis behind; it also fails the script when the gate fails.
    Invoke-Step 'Sonar end' { dotnet dotnet-sonarscanner end "/d:sonar.token=$token" }
    if ($failed) { throw $failed }
    Write-Host "Results: $url/dashboard?id=$ProjectKey" -ForegroundColor Green
}
finally {
    Pop-Location
}
