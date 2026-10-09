using HandlebarsDotNet;
using System;
using System.IO;

namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// Renders prompt templates with Handlebars (no HTML escaping, prompts are plain text).
/// </summary>
public static class PromptRenderer
{
    /// <summary>
    /// Renders the template source with the data.
    /// </summary>
    /// <param name="templateSource">Handlebars template text.</param>
    /// <param name="data">The model the template binds to.</param>
    /// <returns>The prompt text.</returns>
    public static string Render(string templateSource, object data)
    {
        var handlebars = Handlebars.Create(new HandlebarsConfiguration { NoEscape = true });
        return handlebars.Compile(templateSource)(data);
    }

    /// <summary>
    /// Resolves a bundled template name or a file path to the template text.
    /// </summary>
    /// <param name="template">A bundled name (for example <c>documentation</c>) or the path of a template file.</param>
    /// <returns>The short name used for output file names and the template source.</returns>
    /// <exception cref="FileNotFoundException">The template is neither a file nor a bundled name.</exception>
    public static (string Name, string Source) Load(string template)
    {
        if (File.Exists(template))
        {
            return (Path.GetFileNameWithoutExtension(template), File.ReadAllText(template));
        }

        var bundled = Path.Combine(AppContext.BaseDirectory, "Templates", template + ".hbs");
        if (File.Exists(bundled))
        {
            return (template, File.ReadAllText(bundled));
        }

        throw new FileNotFoundException($"Template \"{template}\" is not a file and not one of the bundled templates.", template);
    }
}
