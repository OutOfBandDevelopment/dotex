# Builds the CGI example programs into ../cgi-bin (the script root of the example web API).
# Windows gets greet.exe, other systems greet.cgi; both are listed in appsettings.json.
$ErrorActionPreference = 'Stop'
$out = Join-Path $PSScriptRoot '..\cgi-bin'
$name = if ($IsWindows -or $env:OS -eq 'Windows_NT') { 'greet.exe' } else { 'greet.cgi' }
Push-Location (Join-Path $PSScriptRoot 'greet')
try {
    go test ./...
    if ($LASTEXITCODE -ne 0) { throw 'go test failed' }
    go build -o (Join-Path $out $name) .
    if ($LASTEXITCODE -ne 0) { throw 'go build failed' }
} finally { Pop-Location }
$json = & (Join-Path $out $name) -openapi | Out-String
if ($LASTEXITCODE -ne 0) { throw 'greet -openapi failed' }
[IO.File]::WriteAllText((Join-Path $out 'greet.openapi.json'), $json, (New-Object Text.UTF8Encoding $false))
Write-Host "Built $name into $out"
