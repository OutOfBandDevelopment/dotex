using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Access templates shipped as embedded resources of an assembly. Register it beside <see cref="FileTemplateSource"/>
/// with the default priority (higher than the file source's) so a template on disk overrides the embedded default.
/// </summary>
/// <param name="assembly">The assembly holding the resources.</param>
/// <param name="resourcePrefix">The resource name prefix (for example <c>MyLib.Templates.</c>); folders in a name become dots.</param>
/// <param name="fileTypes">The known file types.</param>
/// <param name="priority">The template priority; the lowest value wins.</param>
public class EmbeddedResourceTemplateSource(
    Assembly assembly,
    string resourcePrefix,
    IEnumerable<IFileType> fileTypes,
    int priority = EmbeddedResourceTemplateSource.DefaultPriority
    ) : ITemplateSource
{
    /// <summary>
    /// The default priority, behind <see cref="FileTemplatingOptions.Priority"/>.
    /// </summary>
    public const int DefaultPriority = 1000;

    /// <inheritdoc />
    public IEnumerable<ITemplateContext> Get(string templateName)
    {
        var names = assembly.GetManifestResourceNames().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var baseName = resourcePrefix + templateName.Replace('/', '.').Replace((char)92, '.');

        var templateTypes = fileTypes.Where(t => t.IsTemplateType);
        var suffixTypes = fileTypes.Where(t => !t.IsTemplateType)
            .Concat([new FileType { Extension = "", ContentType = "application/octet-stream", IsTemplateType = false }]);

        return
            from template in templateTypes
            from target in suffixTypes
            let resource = baseName + target.Extension + template.Extension
            where names.Contains(resource)
            select (ITemplateContext)new TemplateContext
            {
                TemplateName = templateName,
                TemplateContentType = template.ContentType,
                TemplateFileExtension = template.Extension,
                TemplateSource = this,

                TemplateReference = resource,
                OpenTemplate = ctx => assembly.GetManifestResourceStream(ctx.TemplateReference)
                    ?? throw new InvalidOperationException($"Embedded template \"{ctx.TemplateReference}\" not found."),

                TargetContentType = target.ContentType,
                TargetFileExtension = target.Extension,

                Priority = priority,
            };
    }
}
