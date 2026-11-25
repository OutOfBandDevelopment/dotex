using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OoBDev.PackMan.Cli;

/// <summary>Reads and updates the property and package versions of a Directory.Packages.props file.</summary>
/// <param name="logger">Receives progress messages.</param>
public sealed partial class PackageProps(ILogger<PackageProps> logger)
{
    /// <summary>Splits <c>Name,1.2.3;Other</c> into name and optional version pairs.</summary>
    /// <param name="inputVersions">The text to split.</param>
    /// <returns>The pairs; blank names are skipped.</returns>
    public static IEnumerable<(string Name, string? Version)> SplitVersions(string? inputVersions)
    {
        if (string.IsNullOrWhiteSpace(inputVersions))
            yield break;

        foreach (var targetVersion in inputVersions.Split(';'))
        {
            var parts = targetVersion.Split(',', 2);
            if (string.IsNullOrWhiteSpace(parts[0]))
                continue;

            yield return (parts[0].Trim(), parts.ElementAtOrDefault(1)?.Trim());
        }
    }

    /// <summary>Opens a props file; a missing file gives an empty project.</summary>
    /// <param name="versionPropsFile">The props file.</param>
    /// <param name="followImports">Replace each <c>Import</c> element with the imported content.</param>
    /// <returns>The document.</returns>
    public static XDocument Open(string versionPropsFile, bool followImports = false)
    {
        if (!File.Exists(versionPropsFile))
            return new XDocument(new XElement("Project"));

        var xml = XDocument.Load(versionPropsFile, LoadOptions.PreserveWhitespace);
        if (!followImports)
            return xml;

        var directory = Path.GetDirectoryName(Path.GetFullPath(versionPropsFile)) ?? ".";
        foreach (var import in xml.Descendants("Import").ToArray())
        {
            var importFile = (string?)import.Attribute("Project");
            if (string.IsNullOrWhiteSpace(importFile))
                continue;

            var imported = Open(Path.GetFullPath(Path.Combine(directory, importFile)), followImports);
            import.ReplaceWith(imported.Root?.Nodes().Cast<object>().ToArray() ?? []);
        }

        return xml;
    }

    /// <summary>Sets the value of each named property wherever it appears in a property group.</summary>
    /// <param name="document">The document to change.</param>
    /// <param name="properties">The names and values.</param>
    public void UpdateTargetVersions(XDocument document, IEnumerable<(string Name, string? Version)> properties)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(properties);

        var propertyGroups = document.Descendants("PropertyGroup").ToArray();
        foreach (var (name, version) in properties)
        {
            foreach (var element in propertyGroups.Descendants(name))
            {
                LogSetProperty(name, version);
                element.Value = version ?? string.Empty;
            }
        }
    }

    /// <summary>Gets the first non-empty value of each property in the property groups.</summary>
    /// <param name="document">The document to read.</param>
    /// <returns>The property names and values.</returns>
    public static IEnumerable<(string PropertyName, string Value)> GetProperties(XDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return document.Descendants("PropertyGroup").SelectMany(x => x.Elements())
            .Where(element => !string.IsNullOrWhiteSpace(element.Value))
            .GroupBy(element => element.Name.LocalName, element => element.Value, StringComparer.Ordinal)
            .Select(item => (item.Key, item.First()));
    }

    /// <summary>Raises each <c>PackageVersion</c> to the newest published version it may take.</summary>
    /// <remarks>
    /// An unconditioned item moves to the newest release. A conditioned item (for example one per target framework)
    /// stays on its major version, and takes a prerelease only when it is already one. An item with a
    /// <c>_LockVersion</c> child set to true is skipped.
    /// </remarks>
    /// <param name="document">The document to change.</param>
    /// <param name="source">Where versions come from.</param>
    /// <param name="cancellationToken">Stops the run between packages.</param>
    /// <returns>A task that completes when every package was checked.</returns>
    public async Task UpdateNugetVersionsAsync(XDocument document, IPackageVersionSource source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(source);

        foreach (var packageVersion in document.Descendants("PackageVersion").ToArray())
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var packageName = (string?)packageVersion.Attribute("Include");
            var versionAttribute = packageVersion.Attribute("Version");
            var version = (string?)versionAttribute;
            if (string.IsNullOrWhiteSpace(packageName) || versionAttribute is null || !NuGetVersion.TryParse(version, out var current))
                continue;

            if ((bool?)packageVersion.Element("_LockVersion") ?? false)
            {
                LogSkipped(packageName, version);
                continue;
            }

            var versions = await source.GetVersionsAsync(packageName, cancellationToken).ConfigureAwait(false);
            var conditioned = !string.IsNullOrEmpty((string?)packageVersion.Parent?.Attribute("Condition"));
            var target = versions
                .Reverse()
                .FirstOrDefault(v => conditioned
                    ? (current.IsPrerelease || !v.IsPrerelease) && v.Major == current.Major
                    : !v.IsPrerelease);

            if (target is not null && target != current)
            {
                versionAttribute.Value = target.ToString();
                LogUpgrade(packageName, version, target);
                if (target.IsLegacyVersion)
                    LogLegacy(packageName, target);
            }
            else
            {
                LogUpToDate(packageName, version);
                if (current.IsLegacyVersion)
                    LogLegacy(packageName, version);
            }
        }
    }

    /// <summary>Saves the document, creating the folder when needed.</summary>
    /// <param name="document">The document to save.</param>
    /// <param name="filePath">The target file.</param>
    public void Save(XDocument document, string filePath)
    {
        ArgumentNullException.ThrowIfNull(document);

        var fullPath = Path.GetFullPath(filePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        document.Save(fullPath);
        LogSaved(fullPath);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Set {Name} to {Version}")]
    private partial void LogSetProperty(string name, string? version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Skip version check {PackageName}: {Version} => SKIPPED")]
    private partial void LogSkipped(string packageName, string? version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Found upgrade version {PackageName}: {Version} => {Target}")]
    private partial void LogUpgrade(string packageName, string? version, NuGetVersion target);

    [LoggerMessage(Level = LogLevel.Information, Message = "Already up to date {PackageName}: {Version}")]
    private partial void LogUpToDate(string packageName, string? version);

    [LoggerMessage(Level = LogLevel.Information, Message = "{PackageName}: {Version} is legacy, update soon")]
    private partial void LogLegacy(string packageName, object? version);

    [LoggerMessage(Level = LogLevel.Information, Message = "Saved content to {FilePath}")]
    private partial void LogSaved(string filePath);
}
