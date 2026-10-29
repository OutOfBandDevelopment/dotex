<#
.SYNOPSIS
    Points this clone at the versioned git hooks in .githooks.

.DESCRIPTION
    Sets core.hooksPath to .githooks (local to this clone) and marks the hook scripts executable in the
    index. Run it once per clone. -Remove puts git back on the default .git/hooks folder.

.EXAMPLE
    pwsh .githooks/setup.ps1
    pwsh .githooks/setup.ps1 -Remove
#>
[CmdletBinding()]
param([switch] $Remove)

$ErrorActionPreference = 'Stop'
$root = (& git -C $PSScriptRoot rev-parse --show-toplevel).Trim()

if ($Remove) {
    & git -C $root config --local --unset core.hooksPath
    Write-Host 'core.hooksPath unset; git uses .git/hooks again.'
    return
}

& git -C $root config --local core.hooksPath .githooks
foreach ($hook in Get-ChildItem $PSScriptRoot -File | Where-Object { $_.Extension -eq '' }) {
    & git -C $root update-index --chmod=+x --add -- ".githooks/$($hook.Name)" 2>$null
}

Write-Host "core.hooksPath = $(& git -C $root config --local core.hooksPath)"
Write-Host 'Hooks: pre-merge-commit (build and test before a merge commit on main), post-merge (pack after it).'
