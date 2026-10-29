using Microsoft.Extensions.Configuration;
using OoBDev.System.IO;
using global::System;
using global::System.Collections.Generic;
using global::System.IO;

namespace OoBDev.Xslt.Cli;

/// <summary>The command line of the XSLT tool.</summary>
/// <param name="Input">The input file or wildcard.</param>
/// <param name="Exclude">An optional wildcard of inputs to skip.</param>
/// <param name="InputType">The input type.</param>
/// <param name="Template">The XSLT file.</param>
/// <param name="Output">The output file or folder.</param>
/// <param name="Sandbox">The folder the template may read and write.</param>
/// <param name="Merge">Merge all inputs into one document before transforming.</param>
public sealed record XsltCliOptions(
    string Input,
    string? Exclude,
    InputTypes InputType,
    string Template,
    string Output,
    string Sandbox,
    bool Merge)
{
    /// <summary>The usage text.</summary>
    public const string Usage =
        "oobdev-xslt -i <input> -t <template.xslt> -o <output> [-e <exclude>] [-x <input-type>] [-s <sandbox>] [-m]\n" +
        "  -i, --input       input file or wildcard\n" +
        "  -t, --template    XSLT file\n" +
        "  -o, --output      output file or folder\n" +
        "  -e, --exclude     wildcard of inputs to skip\n" +
        "  -x, --input-type  Xml (default), Html, Json, Yaml, CSharp, VB, MSBuildStructuredLog, DacPac, Path, ByExtension\n" +
        "  -s, --sandbox     folder the template may read and write (default: the folder of the output)\n" +
        "  -m, --merge       merge all inputs into one document";

    private static readonly Dictionary<string, string> Switches = new(StringComparer.OrdinalIgnoreCase)
    {
        ["-i"] = "input",
        ["-e"] = "exclude",
        ["-x"] = "input-type",
        ["-t"] = "template",
        ["-o"] = "output",
        ["-s"] = "sandbox",
        ["-m"] = "merge",
    };

    /// <summary>Parses the command line.</summary>
    /// <param name="args">The arguments.</param>
    /// <returns>The options.</returns>
    /// <exception cref="ArgumentException">A required option is missing or a value is invalid.</exception>
    public static XsltCliOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var config = new ConfigurationBuilder().AddCommandLine(AddFlagValue(args), Switches).Build();

        string Required(string key) =>
            PathEx.FixUpPath(config[key] ?? string.Empty) ?? throw new ArgumentException($"--{key} must be provided", nameof(args));

        var output = Required("output");
        var sandbox = PathEx.FixUpPath(config["sandbox"] ?? string.Empty)
            ?? Path.GetDirectoryName(Path.GetFullPath(output))
            ?? throw new ArgumentException("--sandbox must be provided when the output has no folder", nameof(args));

        var typeText = config["input-type"];
        var inputType = InputTypes.Xml;
        if (!string.IsNullOrWhiteSpace(typeText) && !Enum.TryParse(typeText, ignoreCase: true, out inputType))
            throw new ArgumentException($"--input-type '{typeText}' is not one of {string.Join(", ", Enum.GetNames<InputTypes>())}", nameof(args));

        return new XsltCliOptions(
            Input: Required("input"),
            Exclude: PathEx.FixUpPath(config["exclude"] ?? string.Empty),
            InputType: inputType,
            Template: Required("template"),
            Output: output,
            Sandbox: sandbox,
            Merge: bool.TryParse(config["merge"], out var merge) && merge);
    }

    // The configuration provider drops a switch that has no value, so a bare -m or --merge gets "true".
    private static string[] AddFlagValue(string[] args)
    {
        var result = new List<string>(args.Length + 1);
        for (var i = 0; i < args.Length; i++)
        {
            result.Add(args[i]);
            var isMerge = args[i].Equals("-m", StringComparison.OrdinalIgnoreCase) || args[i].Equals("--merge", StringComparison.OrdinalIgnoreCase);
            if (isMerge && (i + 1 == args.Length || args[i + 1].StartsWith('-')))
                result.Add("true");
        }

        return [.. result];
    }
}
