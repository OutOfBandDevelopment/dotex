using Microsoft.Extensions.Configuration;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;

namespace OoBDev.PackMan.Cli;

/// <summary>The command line of the package-version tool.</summary>
/// <param name="SourceFile">The props file to read (Directory.Packages.props).</param>
/// <param name="OutputFile">Where to save; the source file when empty.</param>
/// <param name="NoSave">Do not save changes.</param>
/// <param name="NoNuget">Do not ask the NuGet feed for newer versions.</param>
/// <param name="TargetVersions">Property versions to set, as <c>Name,1.2.3;Other,2.0.0</c>.</param>
/// <param name="NugetUrl">The NuGet service index URL.</param>
/// <param name="FeedType">The feed protocol.</param>
/// <param name="GetProperties">Print every property in the props file.</param>
/// <param name="FollowImports">Replace Import elements with the imported content.</param>
/// <param name="GetProperty">Print one property and stop.</param>
/// <param name="GetVersion">Print the tool version and stop.</param>
/// <param name="UpdateProjects">Align PackageReference versions of matching projects with the props file.</param>
/// <param name="Projects">Project globs separated by <c>;</c>; a leading <c>!</c> excludes.</param>
/// <param name="ProjectsPath">The folder the project globs are relative to.</param>
public sealed record PackManOptions(
    string SourceFile,
    string? OutputFile,
    bool NoSave,
    bool NoNuget,
    string? TargetVersions,
    string NugetUrl,
    FeedType FeedType,
    bool GetProperties,
    bool FollowImports,
    string? GetProperty,
    bool GetVersion,
    bool UpdateProjects,
    string? Projects,
    string? ProjectsPath)
{
    /// <summary>The default NuGet service index.</summary>
    public const string DefaultNugetUrl = "https://api.nuget.org/v3/index.json";

    /// <summary>The usage text.</summary>
    public const string Usage =
        "oobdev-packman [-s <Directory.Packages.props>] [-o <output>] [-n] [--no-nuget] [-t <Name,1.2.3;...>] [--nuget-url <url>]\n" +
        "               [--nuget-feed-type <type>] [--get-properties] [--follow-imports] [-g <property>] [-v]\n" +
        "               [--update-projects --projects <glob;!glob> --projects-path <folder>]\n" +
        "  -s, --source          props file (default: Directory.Packages.props in the current folder)\n" +
        "  -o, --output          save here instead of over the source\n" +
        "  -n, --no-save         do not save changes\n" +
        "      --no-nuget        do not look up newer versions\n" +
        "  -t, --target-versions set property versions (Name,1.2.3;Other,2.0.0)\n" +
        "      --nuget-url       NuGet service index (default " + DefaultNugetUrl + ")\n" +
        "      --nuget-feed-type HttpV3 (default) or HttpV2\n" +
        "      --get-properties  print the properties of the props file\n" +
        "      --follow-imports  replace Import elements with the imported content\n" +
        "  -g, --get-property    print one property and stop\n" +
        "  -v, --get-version     print the tool version and stop\n" +
        "      --update-projects set PackageReference versions of the matching projects from the props file\n" +
        "      --projects        project globs separated by ';', a leading '!' excludes\n" +
        "      --projects-path   folder the globs are relative to (default: current folder)";

    private static readonly Dictionary<string, string> Switches = new(StringComparer.OrdinalIgnoreCase)
    {
        ["-s"] = "source",
        ["-o"] = "output",
        ["-n"] = "no-save",
        ["-t"] = "target-versions",
        ["-g"] = "get-property",
        ["-v"] = "get-version",
    };

    private static readonly HashSet<string> Flags = new(StringComparer.OrdinalIgnoreCase)
    {
        "-n", "--no-save", "--no-nuget", "--get-properties", "--follow-imports", "-v", "--get-version", "--update-projects",
    };

    /// <summary>Parses the command line.</summary>
    /// <param name="args">The arguments.</param>
    /// <returns>The options.</returns>
    /// <exception cref="ArgumentException">A value is invalid.</exception>
    public static PackManOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var config = new ConfigurationBuilder().AddCommandLine(AddFlagValues(args), Switches).Build();

        var feedText = config["nuget-feed-type"];
        var feedType = FeedType.HttpV3;
        if (!string.IsNullOrWhiteSpace(feedText) && !Enum.TryParse(feedText, ignoreCase: true, out feedType))
            throw new ArgumentException($"--nuget-feed-type '{feedText}' is not one of {string.Join(", ", Enum.GetNames<FeedType>())}", nameof(args));

        bool Flag(string key) => bool.TryParse(config[key], out var value) && value;
        string? Text(string key) => string.IsNullOrWhiteSpace(config[key]) ? null : config[key];

        return new PackManOptions(
            SourceFile: Text("source") ?? Path.Combine(Environment.CurrentDirectory, "Directory.Packages.props"),
            OutputFile: Text("output"),
            NoSave: Flag("no-save"),
            NoNuget: Flag("no-nuget"),
            TargetVersions: Text("target-versions"),
            NugetUrl: Text("nuget-url") ?? DefaultNugetUrl,
            FeedType: feedType,
            GetProperties: Flag("get-properties"),
            FollowImports: Flag("follow-imports"),
            GetProperty: Text("get-property"),
            GetVersion: Flag("get-version"),
            UpdateProjects: Flag("update-projects"),
            Projects: Text("projects"),
            ProjectsPath: Text("projects-path"));
    }

    // The configuration provider drops a switch that has no value, so a bare flag gets "true".
    private static string[] AddFlagValues(string[] args)
    {
        var result = new List<string>(args.Length + 2);
        for (var i = 0; i < args.Length; i++)
        {
            result.Add(args[i]);
            if (Flags.Contains(args[i]) && (i + 1 == args.Length || args[i + 1].StartsWith('-')))
                result.Add("true");
        }

        return [.. result];
    }
}
