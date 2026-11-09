using OoBDev.System.Text.Templating.Xslt;
using System;
using System.IO;

namespace OoBDev.TemplateEngine.Cli;

/// <summary>Runs an XSLT template through the sandboxed transformer.</summary>
internal static class XsltRun
{
    /// <summary>Gets a value indicating whether <paramref name="template"/> names an XSLT file.</summary>
    internal static bool IsXslt(string template) =>
        template.EndsWith(".xslt", StringComparison.OrdinalIgnoreCase) || template.EndsWith(".xsl", StringComparison.OrdinalIgnoreCase);

    /// <summary>Transforms the input of <paramref name="options"/> with its XSLT template.</summary>
    internal static void Run(TemplateEngineOptions options)
    {
        var output = options.OutputFile!.Replace("[file]", "*", StringComparison.OrdinalIgnoreCase);
        var sandbox = options.Sandbox
            ?? Path.GetDirectoryName(Path.GetFullPath(output))
            ?? throw new InvalidOperationException("Sandbox is required when the output has no folder.");

        new SandboxedTransformer(sandbox).TransformAll(
            options.Template,
            options.InputFile!,
            options.Exclude,
            options.InputFileType ?? InputTypes.ByExtension,
            output,
            options.Merge);
    }
}
