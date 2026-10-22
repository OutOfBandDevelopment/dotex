<#
.SYNOPSIS
    Writes a copy of src/.runsettings whose Docker host ports follow the TEST_PORT_<NAME> overrides.

.DESCRIPTION
    The compose file publishes every host port as ${TEST_PORT_<NAME>:-default}. The tests read their ports from
    src/.runsettings, which holds the defaults. This script reads the overrides from the process environment and
    from containers/testing/.env (the environment wins), then rewrites the matching ports in a copy of the
    runsettings file, so the stack and the tests use one source of truth.

    Rewrites: "localhost:<port>" and "localhost,<port>" inside values, and <Parameter name="..._PORT" value="<port>">.
    Prints the path of the file to use. When there are no overrides it prints the original path.

.EXAMPLE
    $env:TEST_PORT_SQLSERVER = 14330
    pwsh scripts/ci/new-runsettings.ps1
#>
[CmdletBinding()]
param(
    [string] $Output = (Join-Path $PSScriptRoot '..\..\artifacts\test.runsettings')
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$compose = Join-Path $root 'containers\testing\docker-compose.integration-tests.yml'
$envFile = Join-Path $root 'containers\testing\.env'
$runsettings = Join-Path $root 'src\.runsettings'

# Defaults come from the compose file, so a new service needs no change here.
$defaults = [ordered]@{}
foreach ($m in [regex]::Matches((Get-Content $compose -Raw), '\$\{(TEST_PORT_[A-Z0-9_]+):-(\d+)\}')) {
    $defaults[$m.Groups[1].Value] = $m.Groups[2].Value
}

$fromFile = @{}
if (Test-Path $envFile) {
    foreach ($line in Get-Content $envFile) {
        if ($line -match '^\s*(TEST_PORT_[A-Z0-9_]+)\s*=\s*(\d+)\s*$') { $fromFile[$Matches[1]] = $Matches[2] }
    }
}

$changes = [ordered]@{}
foreach ($name in $defaults.Keys) {
    $value = [Environment]::GetEnvironmentVariable($name)
    if (-not $value) { $value = $fromFile[$name] }
    if ($value -and $value -ne $defaults[$name]) { $changes[$defaults[$name]] = $value }
}

if ($changes.Count -eq 0) {
    Write-Output (Resolve-Path $runsettings)
    return
}

$text = Get-Content $runsettings -Raw
# Replace through a placeholder first so two overrides that swap ports cannot chain.
$i = 0
$pending = @{}
foreach ($old in $changes.Keys) {
    $token = "__PORT_$($i++)__"
    $pending[$token] = $changes[$old]
    $text = [regex]::Replace($text, "(localhost[:,])$old(?!\d)", "`${1}$token")
    $text = [regex]::Replace($text, "(name=`"\w*PORT\w*`"\s+value=`")$old(`")", "`${1}$token`${2}")
}
foreach ($token in $pending.Keys) { $text = $text.Replace($token, $pending[$token]) }

New-Item -ItemType Directory -Force (Split-Path $Output) | Out-Null
Set-Content -Path $Output -Value $text -Encoding utf8
Write-Output (Resolve-Path $Output)
