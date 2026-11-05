[CmdletBinding()]
param(
    [string]$Runtime = 'win-x64',
    [string]$Output = (Join-Path $PSScriptRoot "..\..\artifacts\ser2netwin\$Runtime")
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '..\..\src\Applications\Ser2NetWin\OoBDev.Ser2NetWin.Cli'

dotnet publish $project -c Release -r $Runtime --self-contained `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o $Output
if ($LASTEXITCODE -ne 0) { throw "publish failed" }

Write-Host "Published to $((Resolve-Path $Output).Path)"
Write-Host "Next (elevated): ser2net-win.exe config ; ser2net-win.exe install --start"
