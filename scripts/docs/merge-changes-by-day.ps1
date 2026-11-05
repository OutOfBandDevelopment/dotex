<#
.SYNOPSIS
  Merges the per-task change documents in docs/changes into one file per day and rewrites every link to them.

.DESCRIPTION
  A file docs/changes/{name}-{YYYY-MM-DD}.md becomes the section "## {its title}" of docs/changes/{YYYY-MM-DD}.md.
  The task's headings move one level down. Links to the old file anywhere in the tracked text files point at the
  new section. The old files are removed with git rm. Run it from the repository root; running it again finds
  nothing to merge. -WhatIf only reports the plan.
#>
[CmdletBinding()]
param([switch]$WhatIf)

$ErrorActionPreference = 'Stop'
$utf8 = [System.Text.UTF8Encoding]::new($false)
$changes = Join-Path (Get-Location) 'docs/changes'
$taskPattern = '^(?<name>.+)-(?<date>\d{4}-\d{2}-\d{2})\.md$'

function Get-Slug([string]$title, [hashtable]$seen) {
    $slug = $title.ToLowerInvariant()
    $slug = [regex]::Replace($slug, '[^\p{L}\p{N}\p{M} _-]', '')
    $slug = $slug.Replace(' ', '-')
    if ($seen.ContainsKey($slug)) {
        $n = $seen[$slug]
        $seen[$slug] = $n + 1
        return "$slug-$n"
    }
    $seen[$slug] = 1
    return $slug
}

$tasks = Get-ChildItem $changes -Filter '*.md' -File |
    Where-Object { $_.Name -ne 'README.md' -and $_.Name -match $taskPattern } |
    ForEach-Object {
        [void]($_.Name -match $taskPattern)
        [pscustomobject]@{ File = $_; Name = $Matches['name']; Date = $Matches['date'] }
    }

$map = @{}
foreach ($day in ($tasks | Group-Object Date | Sort-Object Name)) {
    $target = Join-Path $changes "$($day.Name).md"
    $existing = Test-Path $target
    $text = [System.Text.StringBuilder]::new()
    $anchors = @{}
    $seen = @{}
    $sections = [System.Collections.Generic.List[object]]::new()

    foreach ($task in ($day.Group | Sort-Object { $_.File.Name })) {
        $lines = [System.IO.File]::ReadAllText($task.File.FullName, $utf8) -split "`r?`n"
        $title = $null
        $body = [System.Collections.Generic.List[string]]::new()
        $fence = $false
        foreach ($line in $lines) {
            if ($line -match '^\s*(```|~~~)') { $fence = -not $fence }
            if (-not $fence -and $null -eq $title -and $line -match '^#\s+(.+?)\s*$') { $title = $Matches[1]; continue }
            if (-not $fence -and $line -match '^(#{1,5})(\s.*)$') { $body.Add('#' + $line) } else { $body.Add($line) }
        }

        if ($null -eq $title) { $title = $task.Name }
        while ($body.Count -gt 0 -and [string]::IsNullOrWhiteSpace($body[0])) { $body.RemoveAt(0) }
        while ($body.Count -gt 0 -and [string]::IsNullOrWhiteSpace($body[$body.Count - 1])) { $body.RemoveAt($body.Count - 1) }
        $sections.Add([pscustomobject]@{ Task = $task; Title = $title; Body = $body })
    }

    $slugSeen = @{}
    if ($existing) {
        $old = [System.IO.File]::ReadAllText($target, $utf8)
        foreach ($m in [regex]::Matches($old, '(?m)^#{1,6}\s+(.+?)\s*$')) { [void](Get-Slug $m.Groups[1].Value $slugSeen) }
    }

    [void]$text.AppendLine("# Changes $($day.Name)")
    [void]$text.AppendLine()
    [void]$text.AppendLine("Work finished on $($day.Name), one section per task. Index: [README](README.md).")
    [void]$text.AppendLine()
    if ($sections.Count -ge 3) {
        [void]$text.AppendLine('## Contents')
        [void]$text.AppendLine()
    }

    foreach ($s in $sections) {
        $s | Add-Member -NotePropertyName Slug -NotePropertyValue (Get-Slug $s.Title $slugSeen)
        $map[$s.Task.File.Name] = "$($day.Name).md#$($s.Slug)"
        $map["title:$($s.Task.File.Name)"] = $s.Title
        if ($sections.Count -ge 3) { [void]$text.AppendLine("- [$($s.Title)](#$($s.Slug))") }
    }
    if ($sections.Count -ge 3) { [void]$text.AppendLine() }

    foreach ($s in $sections) {
        [void]$text.AppendLine("## $($s.Title)")
        [void]$text.AppendLine()
        foreach ($line in $s.Body) { [void]$text.AppendLine($line) }
        [void]$text.AppendLine()
    }

    if ($existing) {
        $old = [System.IO.File]::ReadAllText($target, $utf8).TrimEnd()
        [void]$text.AppendLine('## Earlier notes of the day')
        [void]$text.AppendLine()
        $fence = $false
        foreach ($line in ($old -split "`r?`n")) {
            if ($line -match '^\s*(```|~~~)') { $fence = -not $fence }
            if (-not $fence -and $line -match '^(#{1,5})(\s.*)$') { [void]$text.AppendLine('##' + $line) } else { [void]$text.AppendLine($line) }
        }
    }

    if ($WhatIf) { Write-Host "$($day.Name): $($sections.Count) task(s)" } else { [System.IO.File]::WriteAllText($target, $text.ToString().TrimEnd() + "`n", $utf8) }
}

if ($WhatIf) { return }

$names = $tasks | ForEach-Object { $_.File.Name } | Sort-Object { $_.Length } -Descending
$files = git ls-files '*.md' '*.cs' '*.mjs' '*.json' '*.ps1' '*.yml' '*.csproj' | Where-Object { $_ -notmatch '(^|/)(bin|obj|Incoming)/' }
$files = @($files) + ($tasks | ForEach-Object { $_.Date } | Sort-Object -Unique | ForEach-Object { "docs/changes/$_.md" })
$changed = 0
foreach ($rel in $files) {
    $path = Join-Path (Get-Location) $rel
    if (-not (Test-Path $path)) { continue }
    $content = [System.IO.File]::ReadAllText($path, $utf8)
    $new = $content
    foreach ($name in $names) {
        if (-not $new.Contains($name)) { continue }
        $target = $map[$name]
        $escaped = [regex]::Escape($name)
        $title = $map["title:$name"]
        $new = [regex]::Replace($new, "\[(\*\*)?$escaped(\*\*)?\]\(", { param($m) "[$($m.Groups[1].Value)$title$($m.Groups[2].Value)](" })
        $new = [regex]::Replace($new, "(?<![A-Za-z0-9_-])$escaped(#[A-Za-z0-9_-]+)?", { param($m) $target })
    }
    if ($new -ne $content) {
        [System.IO.File]::WriteAllText($path, $new, $utf8)
        $changed++
    }
}

foreach ($task in $tasks) { git rm -q -f -- "docs/changes/$($task.File.Name)" }
Write-Host "merged $($tasks.Count) task file(s) into $(($tasks | Group-Object Date).Count) day file(s); rewrote links in $changed file(s)"
