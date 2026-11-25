<#
.SYNOPSIS
  Starts the example stack (SPA, API, Keycloak and the backend services), waits until the SPA answers, and opens it.
.PARAMETER NoBuild  Reuse existing images instead of building.
.PARAMETER NoOpen   Do not open the browser.
.PARAMETER KeycloakPort / OllamaPort  Host ports; the defaults avoid clashes with a local Keycloak or Ollama.
#>
param(
  [switch]$NoBuild,
  [switch]$NoOpen,
  [int]$KeycloakPort = 18081,
  [int]$OllamaPort = 21435,
  [int]$WebPort = 8088,
  [int]$TimeoutSeconds = 300
)
$ErrorActionPreference = 'Stop'
$compose = Join-Path $PSScriptRoot '../../containers/example/docker-compose.example.yml'
$env:TEST_PORT_KEYCLOAK = "$KeycloakPort"
$env:TEST_PORT_OLLAMA = "$OllamaPort"
$env:EXAMPLE_PORT_WEB = "$WebPort"

$composeArgs = @('compose', '-f', $compose, 'up', '-d')
if (-not $NoBuild) { $composeArgs += '--build' }
& docker @args
if ($LASTEXITCODE -ne 0) { throw 'docker compose up failed' }

$url = "http://localhost:$WebPort"
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
do {
  try { if ((Invoke-WebRequest "$url/config.json" -UseBasicParsing -TimeoutSec 3).StatusCode -eq 200) { $ready = $true } } catch { $ready = $false }
  if (-not $ready) { Start-Sleep -Seconds 3 }
} until ($ready -or (Get-Date) -gt $deadline)
if (-not $ready) { throw "The example did not answer on $url within $TimeoutSeconds seconds" }

Write-Host "Example ready: $url (sign in as ada / ada; see containers/example/keycloak/dev-users.md)"
if (-not $NoOpen) { Start-Process $url }
