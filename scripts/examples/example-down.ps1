<#
.SYNOPSIS
  Stops the example stack. -Clean also removes its volumes (Keycloak, databases, Ollama models) and built images.
#>
param([switch]$Clean)
$ErrorActionPreference = 'Stop'
$compose = Join-Path $PSScriptRoot '../../containers/example/docker-compose.example.yml'
$composeArgs = @('compose', '-f', $compose, 'down')
if ($Clean) { $composeArgs += @('--volumes', '--rmi', 'local') }
& docker @args
if ($LASTEXITCODE -ne 0) { throw 'docker compose down failed' }
