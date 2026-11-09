<#
.SYNOPSIS
  Replaces markdown links whose relative target does not exist with their plain text.
.DESCRIPTION
  Mirrors the Links_Relative_Resolve check of OoBDev.Docs.Tests (relative targets, fragments ignored,
  fenced blocks skipped). Used for planned documents that were never written: the text stays, the dead link goes.
.PARAMETER Path
  Folder (or file) to process, relative to the repository root.
.PARAMETER WhatIf
  Only report what would change.
#>
param(
    [Parameter(Mandatory)][string]$Path,
    [switch]$WhatIf
)

$root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$full = Join-Path $root $Path
$files = if (Test-Path $full -PathType Leaf) { Get-Item $full } else { Get-ChildItem $full -Recurse -Filter *.md }
$pattern = [regex]'\[([^\]]*)\]\((?!https?:|mailto:|#)([^)#\s]+)(#[^)\s]*)?\)'
$total = 0

foreach ($file in $files) {
    $directory = $file.DirectoryName
    $fence = $null
    $changed = $false
    $lines = [System.IO.File]::ReadAllLines($file.FullName)
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        if ($null -eq $fence) {
            if ($line -match '^(`{3,})\w*\s*$') { $fence = $Matches[1]; continue }
        }
        elseif ($line.Trim() -eq $fence) { $fence = $null; continue }
        else { continue }

        $lines[$i] = $pattern.Replace($line, {
            param($m)
            $target = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($directory, [Uri]::UnescapeDataString($m.Groups[2].Value)))
            if ((Test-Path -LiteralPath $target)) { return $m.Value }
            $script:total++
            return $m.Groups[1].Value
        })
        if ($lines[$i] -ne $line) { $changed = $true }
    }
    if ($changed -and -not $WhatIf) {
        $newline = if ([System.IO.File]::ReadAllText($file.FullName).Contains("`r`n")) { "`r`n" } else { "`n" }
        [System.IO.File]::WriteAllText($file.FullName, ($lines -join $newline) + $newline)
    }
}
Write-Output "$total dead link(s) $(if ($WhatIf) { 'found' } else { 'unlinked' })"
