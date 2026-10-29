<#
.SYNOPSIS
    Local replacement for the packaging step of the dotnet.yml GitHub workflow.

.DESCRIPTION
    Computes the version with dotnet-gitversion (GitVersion.yml policy), builds src/ and packs every
    packable project into artifacts/packages at the repository root. It never pushes a package anywhere:
    publishing is a separate, owner-approved step.

.EXAMPLE
    pwsh scripts/ci/pack.ps1
    pwsh scripts/ci/pack.ps1 -Version 1.2.3-test      # skip GitVersion
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [string] $Version
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$src = Join-Path $root 'src'
$output = 'G:\nuget'

if (-not $Version) {
    $json = & dotnet-gitversion $root /output json | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw 'dotnet-gitversion failed (dotnet tool install -g GitVersion.Tool)' }
    $Version = $json.FullSemVer.ToLowerInvariant()
}

Write-Host "==== Pack $Version ($Configuration) into $output" -ForegroundColor Cyan
New-Item -ItemType Directory -Force $output | Out-Null

Push-Location $src
try {
    dotnet pack OoBDev.slnx --configuration $Configuration "--property:Version=$Version" "--property:PackageVersion=$Version" "--property:SolutionDir=$src\" --output $output --nologo -v:q
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed with exit code $LASTEXITCODE" }
}
finally {
    Pop-Location
}

$packages = Get-ChildItem $output -Filter '*.nupkg'
Write-Host "$($packages.Count) packages in $output" -ForegroundColor Green
