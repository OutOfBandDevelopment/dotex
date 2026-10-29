using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OoBDev.PackMan.Cli;

/// <summary>Sets the <c>PackageReference</c> versions of projects from the versions in a props file.</summary>
public static class ProjectUpdater
{
    /// <summary>Updates every project the globs match.</summary>
    /// <param name="versionXml">The props document that holds the <c>PackageVersion</c> items.</param>
    /// <param name="projectsPath">The folder the globs are relative to; the current folder when blank.</param>
    /// <param name="projects">Globs separated by <c>;</c>; a leading <c>!</c> excludes.</param>
    /// <returns>The full paths of the projects that were saved.</returns>
    /// <exception cref="NotSupportedException">A matched project has no <c>TargetFramework</c>.</exception>
    public static IReadOnlyList<string> Update(XDocument versionXml, string? projectsPath, string? projects)
    {
        ArgumentNullException.ThrowIfNull(versionXml);

        var matcher = new Matcher(StringComparison.OrdinalIgnoreCase);
        foreach (var pattern in (projects ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (pattern.StartsWith('!'))
                matcher.AddExclude(pattern[1..]);
            else
                matcher.AddInclude(pattern);
        }

        var root = string.IsNullOrWhiteSpace(projectsPath) ? Environment.CurrentDirectory : projectsPath;
        var saved = new List<string>();
        foreach (var project in matcher.GetResultsInFullPath(root))
        {
            if (UpdateProject(versionXml, project))
                saved.Add(project);
        }

        return saved;
    }

    private static bool UpdateProject(XDocument versionXml, string project)
    {
        var projectXml = XElement.Load(project, LoadOptions.PreserveWhitespace);

        var targetFramework = projectXml.Descendants("PropertyGroup").Select(x => (string?)x.Element("TargetFramework")).FirstOrDefault(t => !string.IsNullOrEmpty(t))
            ?? throw new NotSupportedException($"{project} has no TargetFramework");

        var packages = (from itemGroup in versionXml.Root?.Descendants("ItemGroup") ?? []
                        let condition = (string?)itemGroup.Attribute("Condition")
                        where condition is null || condition.Contains(targetFramework, StringComparison.OrdinalIgnoreCase)
                        from packageVersion in itemGroup.Elements("PackageVersion")
                        select (Package: (string?)packageVersion.Attribute("Include"), Version: (string?)packageVersion.Attribute("Version"))).ToArray();

        var changed = false;
        foreach (var reference in projectXml.Descendants("PackageReference").ToArray())
        {
            var package = (string?)reference.Attribute("Include");
            if (string.IsNullOrWhiteSpace(package))
                continue;

            var versionAttribute = reference.Attribute("Version") ?? new XAttribute("Version", string.Empty);
            var target = packages.FirstOrDefault(p => string.Equals(p.Package, package, StringComparison.OrdinalIgnoreCase)).Version;
            if (string.IsNullOrWhiteSpace(target) || string.Equals((string)versionAttribute, target, StringComparison.OrdinalIgnoreCase))
                continue;

            versionAttribute.Value = target;
            if (versionAttribute.Parent is null)
                reference.Add(versionAttribute);

            changed = true;
        }

        if (changed)
            projectXml.Save(project);

        return changed;
    }
}
