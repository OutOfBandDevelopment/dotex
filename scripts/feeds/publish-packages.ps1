<#
.SYNOPSIS
  Packs every packable .NET project into a local NuGet feed and every npm project into a local npm folder.
.DESCRIPTION
  The version is {major}.{minor}.{commits since the last version tag} from GitVersion (git depth), with the branch
  label as the prerelease part on any branch other than main. Reads only the local clone; no remote is contacted.
#>
param(
    [string] $NugetPath = 'G:\nuget',
    [string] $NpmPath = 'G:\npm',
    [string] $Configuration = 'Release',
    [switch] $SkipNuget,
    [switch] $SkipNpm
)

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
Set-Location $root

dotnet tool restore | Out-Null
$gv = (dotnet dotnet-gitversion $root /output json) -join "`n" | ConvertFrom-Json
$version = "$($gv.Major).$($gv.Minor).$($gv.CommitsSinceVersionSource)"
if ($gv.PreReleaseLabel) { $version += "-$($gv.PreReleaseLabel)" }
Write-Host "Version $version ($($gv.BranchName), $($gv.CommitsSinceVersionSource) commits since $($gv.VersionSourceSha.Substring(0, 8)))"

if (-not $SkipNuget) {
    New-Item -ItemType Directory -Force $NugetPath | Out-Null
    dotnet pack src/OoBDev.slnx -c $Configuration -o $NugetPath `
        -p:PackageVersion=$version -p:Version=$version -p:UpdateVersionProperties=false -p:GeneratePackageOnBuild=false
    if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed ($LASTEXITCODE)" }
}

if (-not $SkipNpm) {
    New-Item -ItemType Directory -Force $NpmPath | Out-Null
    $manifests = git ls-files '*package.json' | Where-Object { $_ -notmatch 'node_modules' }
    foreach ($manifest in $manifests) {
        $file = Join-Path $root $manifest
        $original = Get-Content $file -Raw
        try {
            # npm pack reads the version from package.json; set it for the pack and restore the file afterwards.
            Push-Location (Split-Path $file)
            npm pkg set "version=$version" | Out-Null
            npm pack --pack-destination $NpmPath
            if ($LASTEXITCODE -ne 0) { throw "npm pack failed for $manifest ($LASTEXITCODE)" }
        }
        finally {
            Pop-Location
            Set-Content -Path $file -Value $original -NoNewline
        }
    }
}
