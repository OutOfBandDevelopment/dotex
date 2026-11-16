using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Wraps an <see cref="ITemplateEngine"/> so a template name resolves to the best template for the current UI culture:
/// <c>name.fr-CA</c>, then <c>name.fr</c>, then the plain <c>name</c>. Templates are named by file or source, so a
/// translation is just another template next to the original.
/// </summary>
/// <param name="inner">The engine that finds and renders templates.</param>
public class LocalizedTemplateEngine(ITemplateEngine inner) : ITemplateEngine
{
    /// <inheritdoc />
    public ITemplateContext? Get(string templateName) => inner.Get(Resolve(templateName));

    /// <inheritdoc />
    public IEnumerable<ITemplateContext> GetAll(string templateName) => inner.GetAll(Resolve(templateName));

    /// <inheritdoc />
    public Task<ITemplateContext?> ApplyAsync(string templateName, object data, Stream target) =>
        inner.ApplyAsync(Resolve(templateName), data, target);

    /// <inheritdoc />
    public Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target) =>
        inner.ApplyAsync(context, data, target);

    /// <inheritdoc />
    public Task<string?> ApplyAsync(string templateName, object data) =>
        inner.ApplyAsync(Resolve(templateName), data);

    /// <inheritdoc />
    public Task<string?> ApplyAsync(ITemplateContext context, object data) =>
        inner.ApplyAsync(context, data);

    private string Resolve(string templateName)
    {
        for (var culture = CultureInfo.CurrentUICulture; !string.IsNullOrEmpty(culture.Name); culture = culture.Parent)
        {
            var candidate = $"{templateName}.{culture.Name}";
            if (inner.Get(candidate) != null)
            {
                return candidate;
            }
        }

        return templateName;
    }
}
