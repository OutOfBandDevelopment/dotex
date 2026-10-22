<#
.SYNOPSIS
    Local replacement for the dotnet.yml and integration-tests.yml GitHub workflows.

.DESCRIPTION
    Restores, builds and tests src/ for the chosen categories. When Integration is included the Docker
    stack in containers/testing is started first (integration-up.bat --wait) and always stopped afterwards.
    Results (trx) go to artifacts/test-results at the repository root.

.EXAMPLE
    pwsh scripts/ci/run-tests.ps1                         # Unit and Simulate, like a push build
    pwsh scripts/ci/run-tests.ps1 -Category Integration   # Docker stack, then the Integration tests
    pwsh scripts/ci/run-tests.ps1 -Category Unit -Coverage
#>
[CmdletBinding()]
param(
    [ValidateSet('Unit', 'Simulate', 'Integration', 'DevLocal', 'LiveIntegration')]
    [string[]] $Category = @('Unit', 'Simulate'),
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [switch] $Coverage,
    [switch] $KeepStack,
    [switch] $BuildImages
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$src = Join-Path $root 'src'
$testing = Join-Path $root 'containers\testing'
$results = Join-Path $root 'artifacts\test-results'
$stackStarted = $false

function Invoke-Step([string] $Name, [scriptblock] $Action) {
    Write-Host "==== $Name" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -ne 0) { throw "$Name failed with exit code $LASTEXITCODE" }
}

try {
    if ($Category -contains 'Integration') {
        $upArgs = @('--wait')
        if ($BuildImages) { $upArgs += '--build' }
        Invoke-Step 'Start Docker stack' { & (Join-Path $testing 'scripts\integration-up.bat') @upArgs }
        $stackStarted = $true
    }

    # Directory.Build.props computes SolutionDir from the working directory, so run from src.
    Push-Location $src
    try {
        Invoke-Step 'Restore' { dotnet restore }
        Invoke-Step 'Build' { dotnet build --configuration $Configuration --no-restore --nologo }

        $filter = ($Category | ForEach-Object { "TestCategory=$_" }) -join '|'
        $testArgs = @(
            '--configuration', $Configuration, '--no-build', '--no-restore', '--nologo',
            '--settings', (Join-Path $src '.runsettings'),
            '--filter', $filter,
            '--results-directory', $results,
            '--logger', 'trx'
        )
        if ($Coverage) { $testArgs += '--collect:XPlat Code Coverage' }
        Invoke-Step "Test ($filter)" { dotnet test @testArgs }
    }
    finally {
        Pop-Location
    }
}
finally {
    if ($stackStarted -and -not $KeepStack) {
        Write-Host '==== Stop Docker stack' -ForegroundColor Cyan
        & (Join-Path $testing 'scripts\integration-down.bat') --clean
    }
}

Write-Host "Done. Results: $results" -ForegroundColor Green
