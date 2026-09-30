<#
.SYNOPSIS
  Installs the OoBDev dotnet templates into a throw-away hive, generates each one into the
  source tree, builds and tests it, then removes everything it generated.
#>
[CmdletBinding()]
param(
    [switch]$KeepOutput
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$src = Join-Path $root 'src'
$hive = Join-Path ([IO.Path]::GetTempPath()) ('oobdev-template-hive-' + [guid]::NewGuid().ToString('N'))
$generated = @()

function Invoke-Step([string]$What, [scriptblock]$Block) {
    Write-Host "== $What"
    & $Block
    if ($LASTEXITCODE -ne 0) { throw "Step failed: $What (exit $LASTEXITCODE)" }
}

try {
    foreach ($t in 'capability', 'adapter', 'webapp') {
        Invoke-Step "install $t" { dotnet new install (Join-Path $root "templates\content\$t") --debug:custom-hive $hive | Out-Null }
    }

    $fw = Join-Path $src 'Framework'
    Push-Location $fw
    Invoke-Step 'generate capability' { dotnet new oobdev-capability -n ZzCheck --debug:custom-hive $hive }
    Pop-Location
    $generated += Join-Path $fw 'OoBDev.ZzCheck*'

    $vendor = Join-Path $src 'ExternalServices\ZzVendor'
    New-Item -ItemType Directory -Force $vendor | Out-Null
    $generated += $vendor
    Push-Location $vendor
    Invoke-Step 'generate adapter' { dotnet new oobdev-adapter -n ZzVendor --capability ZzCheck --debug:custom-hive $hive }
    Pop-Location

    $ex = Join-Path $src 'Examples'
    Push-Location $ex
    Invoke-Step 'generate webapp' { dotnet new oobdev-webapp -n ZzApp --debug:custom-hive $hive }
    Pop-Location
    $generated += Join-Path $ex 'OoBDev.ZzApp'

    # Directory.Build.props finds the .sln relative to the current directory, so build from a folder one level below src
    Push-Location $fw
    Invoke-Step 'test capability' { dotnet test (Join-Path $fw 'OoBDev.ZzCheck.Tests') --nologo -v q }
    Invoke-Step 'test adapter' { dotnet test (Join-Path $vendor 'OoBDev.ZzVendor.ZzCheck.Tests') --nologo -v q }
    Invoke-Step 'build webapp' { dotnet build (Join-Path $ex 'OoBDev.ZzApp') --nologo -v q }
    Pop-Location
    Write-Host 'All templates generated, built and tested.'
}
finally {
    if (-not $KeepOutput) {
        foreach ($g in $generated) { Remove-Item $g -Recurse -Force -ErrorAction SilentlyContinue }
    }
    Remove-Item $hive -Recurse -Force -ErrorAction SilentlyContinue
}
