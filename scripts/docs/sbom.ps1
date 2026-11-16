<#
.SYNOPSIS
  Writes a CycloneDX 1.6 SBOM for the solution and a markdown report from it.
.DESCRIPTION
  Components come from `dotnet list package --include-transitive` (resolved versions after restore), de-duplicated and
  sorted by name and version. The timestamp is the last commit's date, so two runs on one commit give identical files.
  The markdown comes from docs/templates/SoftwareBillOfMaterials.md.xslt through OoBDev.TemplateEngine.Cli.
  Output: artifacts/sbom/bom.xml and artifacts/sbom/bom.md (artifacts/ is not committed).
#>
[CmdletBinding()]
param(
    [string]$Solution = 'src/OoBDev.slnx',
    [string]$OutputDirectory = 'artifacts/sbom',
    [switch]$SkipReport
)

$ErrorActionPreference = 'Stop'
$root = (git rev-parse --show-toplevel).Trim()
Set-Location $root
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null

$json = dotnet list $Solution package --include-transitive --format json --no-restore | Out-String | ConvertFrom-Json
if ($json.problems) { throw ($json.problems | ForEach-Object { $_.text }) -join '; ' }

$packages = foreach ($project in $json.projects) {
    foreach ($framework in $project.frameworks) {
        foreach ($package in @($framework.topLevelPackages) + @($framework.transitivePackages)) {
            if ($package) { [pscustomobject]@{ Id = $package.id; Version = $package.resolvedVersion; Direct = $framework.topLevelPackages -contains $package } }
        }
    }
}

$components = $packages | Group-Object Id, Version | ForEach-Object {
    [pscustomobject]@{ Id = $_.Group[0].Id; Version = $_.Group[0].Version; Direct = [bool]($_.Group | Where-Object Direct) }
} | Sort-Object @{ Expression = 'Id'; Ascending = $true }, Version

$version = (git describe --tags --always).Trim()
$time = [DateTimeOffset]::Parse((git log -1 --format=%cI).Trim()).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")

$xml = [System.Xml.XmlDocument]::new()
$ns = 'http://cyclonedx.org/schema/bom/1.6'
$bom = $xml.AppendChild($xml.CreateElement('bom', $ns))
$bom.SetAttribute('version', '1')
function Add-Text($parent, $name, $text) { $e = $parent.AppendChild($xml.CreateElement($name, $ns)); $e.InnerText = $text; $e }

$metadata = $bom.AppendChild($xml.CreateElement('metadata', $ns))
Add-Text $metadata 'timestamp' $time | Out-Null
$tool = $metadata.AppendChild($xml.CreateElement('tools', $ns)).AppendChild($xml.CreateElement('tool', $ns))
Add-Text $tool 'vendor' 'OoBDev scripts/docs/sbom.ps1' | Out-Null
Add-Text $tool 'name' 'dotnet list package' | Out-Null
Add-Text $tool 'version' '1' | Out-Null
$app = $metadata.AppendChild($xml.CreateElement('component', $ns))
$app.SetAttribute('type', 'application')
Add-Text $app 'name' 'OoBDev (dotex)' | Out-Null
Add-Text $app 'version' $version | Out-Null

$list = $bom.AppendChild($xml.CreateElement('components', $ns))
foreach ($c in $components) {
    $purl = "pkg:nuget/$($c.Id)@$($c.Version)"
    $e = $list.AppendChild($xml.CreateElement('component', $ns))
    $e.SetAttribute('type', 'library')
    $e.SetAttribute('bom-ref', $purl)
    Add-Text $e 'name' $c.Id | Out-Null
    Add-Text $e 'version' $c.Version | Out-Null
    Add-Text $e 'scope' ($(if ($c.Direct) { 'required' } else { 'optional' })) | Out-Null
    Add-Text $e 'purl' $purl | Out-Null
}

$settings = [System.Xml.XmlWriterSettings]@{ Indent = $true; Encoding = [System.Text.UTF8Encoding]::new($false); NewLineChars = "`n" }
$bomPath = Join-Path $OutputDirectory 'bom.xml'
$writer = [System.Xml.XmlWriter]::Create((Join-Path $root $bomPath), $settings)
$xml.Save($writer)
$writer.Close()
"CycloneDX: $bomPath ($($components.Count) components)"

if (-not $SkipReport) {
    $report = Join-Path $OutputDirectory 'bom.md'
    dotnet run --project src/Tools/OoBDev.TemplateEngine.Cli -- --input (Join-Path $root $bomPath) --output (Join-Path $root (Join-Path $OutputDirectory '[file].md')) --template (Join-Path $root 'docs/templates/SoftwareBillOfMaterials.md.xslt')
    if ($LASTEXITCODE -ne 0) { throw 'The report step failed.' }
    "Report: $report"
}