<#
.SYNOPSIS
    Packs every packable .NET project and every npm project with one GitVersion version.

.DESCRIPTION
    Computes the version with dotnet-gitversion (GitVersion.yml policy), packs the NuGet packages into G:\nuget and the npm
    packages into G:\npm with that same version. It never pushes a package to a remote registry: publishing is a separate,
    owner-approved step.

.EXAMPLE
    pwsh scripts/ci/pack.ps1
    pwsh scripts/ci/pack.ps1 -Version 1.2.3-test      # skip GitVersion
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string] $Configuration = 'Release',
    [string] $Version,
    [switch] $SkipNpm
)

$ErrorActionPreference = 'Stop'
$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$src = Join-Path $root 'src'
$output = 'G:\nuget'
$npmOutput = 'G:\npm'

if (-not $Version) {
    $json = & dotnet-gitversion $root /output json | ConvertFrom-Json
    if ($LASTEXITCODE -ne 0) { throw 'dotnet-gitversion failed (dotnet tool install -g GitVersion.Tool)' }
    $Version = $json.FullSemVer.ToLowerInvariant()
}
$npmVersion = $Version -replace '\+.*

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

if (-not $SkipNpm) {
    New-Item -ItemType Directory -Force $npmOutput | Out-Null
    # same rule as the NuGet side: no examples, tests or private packages
    $manifests = git -C $root ls-files '*package.json' | Where-Object {
        $_ -notmatch 'node_modules|(^|/)Examples?(/|$)|(^|/)[^/]*\.(Tests?|Examples?|AppHost)(/|$)' -and
        -not (Get-Content (Join-Path $root $_) -Raw | ConvertFrom-Json).private
    }
    foreach ($manifest in $manifests) {
        $file = Join-Path $root $manifest
        $original = Get-Content $file -Raw
        Push-Location (Split-Path $file)
        try {
            # npm pack takes the version from package.json: set it for the pack, then restore the file
            npm pkg set "version=$npmVersion" | Out-Null
            npm pack --pack-destination $npmOutput --silent
            if ($LASTEXITCODE -ne 0) { throw "npm pack failed for $manifest with exit code $LASTEXITCODE" }
        }
        finally {
            Pop-Location
            Set-Content -Path $file -Value $original -NoNewline
        }
    }
    Write-Host "$(@($manifests).Count) npm package(s) in $npmOutput" -ForegroundColor Green
}
, ''

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
