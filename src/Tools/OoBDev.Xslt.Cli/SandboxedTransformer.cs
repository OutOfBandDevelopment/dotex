using OoBDev.Extensions.CodeAnalysis.CSharp;
using OoBDev.Extensions.CodeAnalysis.DacFx;
using OoBDev.Extensions.CodeAnalysis.StructuredLog;
using OoBDev.Extensions.CodeAnalysis.VisualBasic;
using OoBDev.System.IO;
using OoBDev.System.Xml.XPath;
using OoBDev.System.Text.Html;
using OoBDev.System.Text.Json;
using OoBDev.System.Text.Yaml;
using OoBDev.System.Xml.Xsl;
using OoBDev.System.Xml.Xsl.Extensions;
using OoBDev.TestUtilities.Xml.Xsl.Extensions;
using global::System;
using global::System.Globalization;
using global::System.IO;
using global::System.Xml.XPath;

namespace OoBDev.Xslt.Cli;

/// <summary>Runs an XSLT template over inputs of many types; file functions in the template reach only the sandbox folder.</summary>
public sealed class SandboxedTransformer
{
    private readonly XsltTransformer _transformer;

    /// <summary>Creates a transformer whose file and path functions are limited to <paramref name="sandbox"/>.</summary>
    /// <param name="sandbox">The folder the template may read and write.</param>
    public SandboxedTransformer(string sandbox)
    {
        var root = Path.GetFullPath(sandbox);
        _transformer = new XsltTransformer(
            root,
            new PathExtensions(root),
            new EnvironmentExtensions(),
            new FileExtensions(root),
            new TrxExtensions(),
            new StringExtensions(),
            new XmlExtensions(),
            new XPath20Functions());
    }

    /// <summary>Transforms <paramref name="input"/> (a file, a wildcard, or a path for <see cref="InputTypes.Path"/>) with <paramref name="template"/>.</summary>
    /// <param name="template">The XSLT file.</param>
    /// <param name="input">The input file or wildcard.</param>
    /// <param name="exclude">An optional wildcard of inputs to skip.</param>
    /// <param name="inputType">The input type; <see cref="InputTypes.ByExtension"/> picks it from the extension.</param>
    /// <param name="output">The output file or folder.</param>
    /// <param name="merge">Merge all inputs into one document before transforming.</param>
    public void TransformAll(string template, string input, string? exclude, InputTypes inputType, string output, bool merge)
    {
        if (inputType == InputTypes.Path)
        {
            _transformer.Transform(template, input, GetNavigator(inputType, input)(input), output);
            return;
        }

        var factory = GetNavigator(inputType, input);
        if (merge)
            _transformer.TransformMerge(template, input, factory, output, exclude);
        else
            _transformer.TransformAll(template, input, factory, output, exclude);
    }

    /// <summary>Gets the function that opens a file as an XPath-navigable document.</summary>
    /// <param name="inputType">The input type.</param>
    /// <param name="filePath">The file, used when the type is <see cref="InputTypes.ByExtension"/>.</param>
    /// <returns>The navigator factory.</returns>
    /// <exception cref="NotSupportedException">No navigator exists for the type or extension.</exception>
    public Func<string, IXPathNavigable> GetNavigator(InputTypes inputType, string filePath) =>
        (inputType == InputTypes.ByExtension ? ByExtension(filePath) : ByType(inputType))
        ?? throw new NotSupportedException($"No mapping for {inputType} ({Path.GetExtension(filePath)}) found");

    private static Func<string, IXPathNavigable> Open(IToXPathNavigable navigator) =>
        file => navigator.ToNavigable(file) ?? throw new FormatException($"{file} could not be opened");

    private Func<string, IXPathNavigable>? ByType(InputTypes inputType) =>
        inputType switch
        {
            InputTypes.Xml => file => _transformer.ReadAsXml(file) ?? throw new FormatException($"{file} is not valid XML"),
            InputTypes.Html => Open(new HtmlNavigator()),
            InputTypes.Json => Open(new JsonNavigator()),
            InputTypes.Yaml => Open(new YamlNavigator()),
            InputTypes.CSharp => Open(new CSharpNavigator()),
            InputTypes.VB => Open(new VisualBasicNavigator()),
            InputTypes.MSBuildStructuredLog => Open(new StructuredLogNavigator()),
            InputTypes.DacPac => Open(new DacPacNavigator()),
            InputTypes.Path => Open(new PathNavigator()),
            _ => null,
        };

    private Func<string, IXPathNavigable>? ByExtension(string filePath) =>
        Path.GetExtension(filePath).ToUpper(CultureInfo.InvariantCulture) switch
        {
            ".XML" => ByType(InputTypes.Xml),
            ".HTML" or ".HTM" => ByType(InputTypes.Html),
            ".JSON" => ByType(InputTypes.Json),
            ".YAML" or ".YML" => ByType(InputTypes.Yaml),
            ".CS" => ByType(InputTypes.CSharp),
            ".VB" => ByType(InputTypes.VB),
            ".BINLOG" => ByType(InputTypes.MSBuildStructuredLog),
            ".DACPAC" => ByType(InputTypes.DacPac),
            _ => null,
        };
}
