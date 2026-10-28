using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Cgi.OpenApi;

/// <summary>
/// Adds the operations that a CGI program describes in its own <c>{program}.openapi.json</c> to the OpenAPI document.
/// Paths in the description are relative to the program: <c>/</c> is the program itself and <c>/{id}</c> reaches it with PATH_INFO.
/// </summary>
/// <param name="cgi">The CGI options (prefix, script root and handlers).</param>
/// <param name="settings">The CGI OpenAPI options.</param>
/// <param name="logger">The logger.</param>
public partial class CgiOpenApiDocumentTransformer(
    IOptions<CgiOptions> cgi,
    IOptions<CgiOpenApiOptions> settings,
    ILogger<CgiOpenApiDocumentTransformer> logger) : IOpenApiDocumentTransformer
{
    /// <inheritdoc />
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var root = Path.GetFullPath(cgi.Value.ScriptRoot);
        var suffix = settings.Value.FileSuffix;
        var prefix = cgi.Value.Prefix.TrimEnd('/');

        foreach (var file in Directory.EnumerateFiles(root, "*" + suffix).Order(StringComparer.OrdinalIgnoreCase))
        {
            var baseName = Path.GetFileName(file)[..^suffix.Length];
            var script = FindScript(root, baseName);
            if (script == null)
            {
                LogNoProgram(Path.GetFileName(file));
                continue;
            }

            var text = await File.ReadAllTextAsync(file, cancellationToken).ConfigureAwait(false);
            var read = OpenApiDocument.Parse(text, OpenApiConstants.Json);
            if (read.Document == null || read.Diagnostic?.Errors.Count > 0)
            {
                LogInvalid(Path.GetFileName(file), string.Join("; ", read.Diagnostic?.Errors.Select(e => e.Message) ?? []));
                continue;
            }

            Merge(document, read.Document, $"{prefix}/{script}", baseName);
        }
    }

    private string? FindScript(string root, string baseName) =>
        cgi.Value.Handlers.Keys
            .Select(extension => baseName + extension)
            .FirstOrDefault(name => File.Exists(Path.Combine(root, name)));

    private void Merge(OpenApiDocument target, OpenApiDocument source, string basePath, string tagName)
    {
        target.Paths ??= [];
        foreach (var (path, item) in source.Paths ?? [])
        {
            var key = basePath + (path == "/" ? string.Empty : path);
            if (target.Paths.ContainsKey(key))
            {
                LogDuplicate(key);
                continue;
            }
            if (item.Operations != null)
            {
                foreach (var operation in item.Operations.Values)
                {
                    if (operation.Tags == null || operation.Tags.Count == 0)
                        operation.Tags = new HashSet<OpenApiTagReference> { new(tagName) };
                }
            }
            target.Paths[key] = item;
        }

        if (source.Components?.Schemas is { Count: > 0 } schemas)
        {
            target.Components ??= new OpenApiComponents();
            target.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);
            foreach (var (name, schema) in schemas)
            {
                if (!target.Components.Schemas.TryAdd(name, schema))
                    LogDuplicate("#/components/schemas/" + name);
            }
        }

        if (source.Tags is { Count: > 0 })
        {
            target.Tags ??= new HashSet<OpenApiTag>();
            foreach (var tag in source.Tags)
                target.Tags.Add(tag);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The description {File} has no CGI program next to it with a listed handler; skipped.")]
    private partial void LogNoProgram(string file);

    [LoggerMessage(Level = LogLevel.Warning, Message = "The description {File} is not valid OpenAPI and is skipped: {Errors}")]
    private partial void LogInvalid(string file, string errors);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Key} is already in the document; the CGI description for it is ignored.")]
    private partial void LogDuplicate(string key);
}
