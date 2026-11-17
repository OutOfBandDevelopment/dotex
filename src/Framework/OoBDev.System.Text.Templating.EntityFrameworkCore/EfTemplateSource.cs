using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace OoBDev.System.Text.Templating.EntityFrameworkCore;

/// <summary>
/// Serves templates from the database. The requested name may carry a culture suffix (<c>welcome.es-ES</c>, the form
/// <see cref="LocalizedTemplateEngine"/> asks for) and a version (<c>welcome@2</c>); with no version the latest active one is served.
/// </summary>
/// <param name="repository">The template store.</param>
/// <param name="options">The tenant and priority.</param>
public sealed class EfTemplateSource(EfTemplateRepository repository, IOptions<EfTemplateSourceOptions> options) : ITemplateSource
{
    /// <inheritdoc />
    public IEnumerable<ITemplateContext> Get(string templateName)
    {
        ArgumentNullException.ThrowIfNull(templateName);
        var (name, version) = SplitVersion(templateName);
        var settings = options.Value;

        var row = repository.GetAsync(name, string.Empty, settings.TenantId, version).GetAwaiter().GetResult();
        if (row == null)
        {
            var dot = name.LastIndexOf('.');
            if (dot > 0 && IsCulture(name[(dot + 1)..]))
            {
                row = repository.GetAsync(name[..dot], name[(dot + 1)..], settings.TenantId, version).GetAwaiter().GetResult();
            }
        }

        if (row == null)
        {
            yield break;
        }

        var content = row.Content;
        yield return new TemplateContext
        {
            TemplateName = templateName,
            TemplateContentType = row.ContentType,
            TemplateFileExtension = string.Empty,
            TemplateSource = this,
            TemplateReference = $"db:{row.Name}/{row.Culture}/{row.TenantId}@{row.Version}",
            OpenTemplate = _ => new MemoryStream(Encoding.UTF8.GetBytes(content)),
            TargetContentType = row.TargetContentType,
            TargetFileExtension = row.TargetFileExtension,
            Priority = settings.Priority,
        };
    }

    private static (string Name, int? Version) SplitVersion(string templateName)
    {
        var at = templateName.LastIndexOf('@');
        return at > 0 && int.TryParse(templateName[(at + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var v)
            ? (templateName[..at], v)
            : (templateName, null);
    }

    private static bool IsCulture(string text)
    {
        try
        {
            return text.Length > 0 && !CultureInfo.GetCultureInfo(text, predefinedOnly: true).Equals(CultureInfo.InvariantCulture);
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }
}
