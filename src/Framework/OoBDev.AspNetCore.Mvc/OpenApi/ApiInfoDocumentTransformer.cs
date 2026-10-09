using System;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using OoBDev.System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Document transformer that presents the application title, version and description.
/// The <c>all</c> document uses the first available <see cref="IVersionProvider"/> or assembly information; the other documents use their assembly.
/// </summary>
public class ApiInfoDocumentTransformer(
    IActionDescriptorCollectionProvider provider,
    IEnumerable<IVersionProvider> versions
        ) : IOpenApiDocumentTransformer
{
    /// <summary>
    /// Sets <see cref="OpenApiDocument.Info"/>.
    /// </summary>
    /// <param name="document">The document to modify.</param>
    /// <param name="context">The transformer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var controllerAssemblies = provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Select(c => c.ControllerTypeInfo.Assembly)
            .Distinct()
            .ToList();

        if (!string.Equals(context.DocumentName, OpenApiDocumentCatalog.AllDocumentName, StringComparison.OrdinalIgnoreCase))
        {
            var assembly = controllerAssemblies.FirstOrDefault(a => string.Equals(a.GetName().Name, context.DocumentName, StringComparison.OrdinalIgnoreCase));
            document.Info = new OpenApiInfo
            {
                Title = assembly?.GetName().Name ?? context.DocumentName,
                Version = assembly?.GetName().Version?.ToString() ?? "v0.0.0.0",
            };
            return Task.CompletedTask;
        }

        var composed = versions.Reverse()
            .Select(v => (Title: v.Title, Description: v.Description, Version: v.Version, Assembly: v.Assembly))
            .Concat(controllerAssemblies.Concat([Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly()])
                .Select(a => (Title: (string?)null, Description: (string?)null, Version: (string?)null, Assembly: a)));

        var selected = (from i in composed
                        let fvi = i.Assembly == null || string.IsNullOrEmpty(i.Assembly.Location) ? null : FileVersionInfo.GetVersionInfo(i.Assembly.Location)
                        select new
                        {
                            Name = First(i.Title, fvi?.ProductName, i.Assembly?.GetName().Name),
                            Version = First(i.Version, fvi?.FileVersion, i.Assembly?.GetName().Version?.ToString()),
                            Description = First(i.Description, fvi?.Comments),
                        }).FirstOrDefault(i => i.Name != null);

        document.Info = new OpenApiInfo
        {
            Title = selected?.Name ?? "Unknown",
            Version = selected?.Version ?? "v0.0.0.0",
            Description = selected?.Description,
        };
        return Task.CompletedTask;
    }

    private static string? First(params string?[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
