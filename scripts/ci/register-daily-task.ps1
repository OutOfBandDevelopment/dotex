<#
.SYNOPSIS
    Registers a Windows Task Scheduler entry that runs the Integration suite daily (replaces the cron of integration-tests.yml).

.EXAMPLE
    pwsh scripts/ci/register-daily-task.ps1 -At 16:00
    pwsh scripts/ci/register-daily-task.ps1 -Unregister
#>
[CmdletBinding()]
param(
    [string] $At = '16:00',
    [string] $TaskName = 'OoBDev Integration Tests',
    [switch] $Unregister
)

$ErrorActionPreference = 'Stop'

if ($Unregister) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
    return
}

$root = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$script = Join-Path $PSScriptRoot 'run-tests.ps1'
$action = New-ScheduledTaskAction -Execute 'pwsh.exe' -WorkingDirectory $root `
    -Argument "-NoProfile -File `"$script`" -Category Integration"
$trigger = New-ScheduledTaskTrigger -Daily -At $At
Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Description 'Runs scripts/ci/run-tests.ps1 -Category Integration' | Out-Null
Write-Host "Registered '$TaskName' daily at $At"
