<#
.SYNOPSIS
  Local check for the example SPA: unit tests, build, Playwright journeys, and generated-docs drift.

.DESCRIPTION
  Runs `npm ci` (skip with -SkipInstall), Vitest, the production build, and the Playwright journeys, which
  rewrite docs/example (user guides, specs, screenshots). Then fails if the regenerated guides and specs (markdown) differ from what
  is committed (screenshots are rewritten but not compared: they vary by sub-pixel rendering), so a UI change cannot be committed without its guide being refreshed.
  Use -Accept to keep the regenerated docs and not fail on a difference.
  Set E2E_BASE_URL first to run the journeys against the compose stack instead of the mocked API.
#>
param(
    [switch]$SkipInstall,
    [switch]$Accept
)

$ErrorActionPreference = 'Stop'
$repo = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$web = Join-Path $repo 'src\Examples\OoBDev.Example.Web'

function Invoke-Step([string]$Name, [scriptblock]$Block) {
    Write-Host "== $Name" -ForegroundColor Cyan
    & $Block
    if ($LASTEXITCODE -ne 0) { throw "$Name failed (exit $LASTEXITCODE)" }
}

Push-Location $web
try {
    if (-not $SkipInstall) {
        Invoke-Step 'npm ci' { npm ci }
        Invoke-Step 'playwright install chromium' { npx playwright install chromium }
    }
    Invoke-Step 'unit tests' { npm test }
    Invoke-Step 'build' { npm run build }
    Invoke-Step 'journeys' { npm run e2e }
}
finally {
    Pop-Location
}

Push-Location $repo
try {
    $drift = git status --porcelain -- 'docs/example/*.md'
    if ($drift -and -not $Accept) {
        Write-Host "Generated guides or specs differ from the committed ones:" -ForegroundColor Yellow
        $drift | ForEach-Object { Write-Host "  $_" }
        throw 'Review docs/example, commit it, or rerun with -Accept.'
    }
    Write-Host 'Example web verified.' -ForegroundColor Green
}
finally {
    Pop-Location
}
