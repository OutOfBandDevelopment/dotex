<#
.SYNOPSIS
    Runs test projects on Linux inside the .NET SDK container (for tests that need a real Linux shell).

.DESCRIPTION
    Copies src/ (without bin and obj) into the container, so the Windows build output on the host is never
    touched, then runs dotnet test there (GitVersion is off because the copy has no .git). Results (trx) go to artifacts/test-results/linux.

.EXAMPLE
    pwsh scripts/ci/run-linux-tests.ps1                                   # CGI middleware tests
    pwsh scripts/ci/run-linux-tests.ps1 -Project Extensions/OoBDev.AspNetCore.Cgi.Tests -Filter TestCategory=Simulate
#>
[CmdletBinding()]
param(
    [string] $Project = 'Extensions/OoBDev.AspNetCore.Cgi.Tests',
    [string] $Filter = '',
    [string] $Image = 'mcr.microsoft.com/dotnet/sdk:10.0'
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$results = Join-Path $root 'artifacts\test-results\linux'
New-Item -ItemType Directory -Force $results | Out-Null

$filterArg = if ($Filter) { "--filter '$Filter'" } else { '' }
$script = @"
set -e
mkdir -p /work
tar -C /source --exclude=bin --exclude=obj --exclude=.vs --exclude=publish --exclude=TestResults -cf - src GitVersion.yml | tar -C /work -xf -
cd /work/src
dotnet test $Project $filterArg -p:DisableGitVersionTask=true --logger 'trx;LogFileName=linux.trx' --results-directory /results
"@

docker run --rm -v "${root}:/source:ro" -v "${results}:/results" $Image bash -c $script.Replace("`r", '')
if ($LASTEXITCODE -ne 0) { throw "Linux tests failed (exit $LASTEXITCODE)." }
