using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Generate templating engine that will try to use best match for source and provider
/// </summary>
/// <param name="sources">An enumerable collection of template sources.</param>
/// <param name="providers">An enumerable collection of template providers.</param>
/// <param name="logger">The logger for capturing log messages.</param>
public class TemplateEngine(
    IEnumerable<ITemplateSource> sources,
    IEnumerable<ITemplateProvider> providers,
    ILogger<TemplateEngine> logger
        ) : ITemplateEngine
{
    /// <summary>
    /// Applies the template asynchronously to the provided data and writes the result to the target stream.
    /// </summary>
    /// <param name="templateName">The name of the template to apply.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <param name="target">The stream to write the result to.</param>
    /// <returns>The applied template context or <c>null</c> if no matching template is found.</returns>
    public virtual async Task<ITemplateContext?> ApplyAsync(string templateName, object data, Stream target)
    {
        logger.LogInformation(
            $"Apply {{{nameof(templateName)}}} => {{{nameof(data)}}}",
            templateName,
            data
            );

        var templates =
            from context in GetAll(templateName)
            from provider in providers
            where provider.CanApply(context)
            select (context, provider);

        var template = templates.FirstOrDefault();
        if (template == default || template.context == null)
        {
            logger.LogInformation(
                $"No template {{{nameof(templateName)}}}",
                templateName
                );
            return null;
        }

        if (await template.provider.ApplyAsync(template.context, data, target))
        {
            logger.LogInformation(
                $"Applied {{{nameof(templateName)}}} => {{{nameof(template)}}}",
                templateName,
                template
                );
            return template.context;
        }

        return null;
    }

    /// <summary>
    /// Applies the template asynchronously to the provided data and writes the result to the target stream.
    /// </summary>
    /// <param name="context">The template context to apply.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <param name="target">The stream to write the result to.</param>
    /// <returns><c>true</c> if the template is applied successfully; otherwise, <c>false</c>.</returns>
    public virtual async Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target)
    {
        var provider = providers.FirstOrDefault(p => p.CanApply(context));
        return provider != null && await provider.ApplyAsync(context, data, target);
    }

    /// <summary>
    /// Applies the template asynchronously to the provided data and returns the result as a string.
    /// </summary>
    /// <param name="templateName">The name of the template to apply.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <returns>The applied template as a string or <c>null</c> if no matching template is found.</returns>
    public virtual async Task<string?> ApplyAsync(string templateName, object data)
    {
        using var ms = new MemoryStream();
        var context = await ApplyAsync(templateName, data, ms);
        return context == null ? null : Encoding.UTF8.GetString(ms.ToArray());
    }

    /// <summary>
    /// Applies the template asynchronously to the provided data and returns the result as a string.
    /// </summary>
    /// <param name="context">The template context to apply.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <returns>The applied template as a string or <c>null</c> if the template cannot be applied.</returns>
    public virtual async Task<string?> ApplyAsync(ITemplateContext context, object data)
    {
        using var ms = new MemoryStream();
        return !await ApplyAsync(context, data, ms) ? null : Encoding.UTF8.GetString(ms.ToArray());
    }

    /// <summary>
    /// Gets the template context with the specified name.
    /// </summary>
    /// <param name="templateName">The name of the template to retrieve.</param>
    /// <returns>The template context or <c>null</c> if no matching template is found.</returns>
    public virtual ITemplateContext? Get(string templateName) =>
        GetAll(templateName).FirstOrDefault();

    /// <summary>
    /// Gets all template contexts with the specified name.
    /// </summary>
    /// <param name="templateName">The name of the template to retrieve.</param>
    /// <returns>An enumerable collection of template contexts.</returns>
    public virtual IEnumerable<ITemplateContext> GetAll(string templateName) =>
        from source in sources
        from context in source.Get(templateName)
        orderby context.Priority
        select context;
}
