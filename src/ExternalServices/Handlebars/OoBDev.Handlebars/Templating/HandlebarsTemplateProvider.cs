using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;
using HandlebarsDotNet.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Handlebars.Templating;

/// <summary>
/// Provides Handlebars template processing functionality.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="HandlebarsTemplateProvider"/> class.
/// </remarks>
/// <param name="helpersRegistry">The collection of helpers registries.</param>
/// <param name="blockHelperDescriptors">The collection of helpers registries.</param>
/// <param name="helperDescriptors">The collection of helpers registries.</param>
/// <param name="log">The logger instance.</param>
/// <param name="options">The provider options; <c>NoEscape</c> is on when omitted.</param>
public class HandlebarsTemplateProvider(
    IEnumerable<IHelpersRegistry> helpersRegistry,
    IEnumerable<IHelperDescriptor<BlockHelperOptions>> blockHelperDescriptors,
    IEnumerable<IHelperDescriptor<HelperOptions>> helperDescriptors,
    ILogger<HandlebarsTemplateProvider> log,
    IOptions<HandlebarsOptions>? options = null
    ) : ITemplateProvider
{
    private readonly ILogger _log = log;
    private readonly ConcurrentDictionary<string, HandlebarsTemplate<TextWriter, object, object>> _cache = new(StringComparer.Ordinal);
    private readonly global::System.Threading.Lock _gate = new();
    private IHandlebars? _handlebars;

    private IHandlebars Environment { get { lock (_gate) { return _handlebars ??= CreateEnvironment(); } } }

    /// <summary>
    /// Gets the collection of supported content types by the template provider.
    /// 
    /// text/x-handlebars-template
    /// </summary>
    public IReadOnlyCollection<string> SupportedContentTypes { get; } = new[]
    {
         ContentTypesExtensions.Text.HandlebarsTemplate
    };

    /// <summary>
    /// Determines whether this template provider can apply template processing to the given context.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <returns><c>true</c> if the template processing can be applied; otherwise, <c>false</c>.</returns>
    public bool CanApply(ITemplateContext context) =>
        SupportedContentTypes.Any(type => string.Equals(context.TemplateContentType, type, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Asynchronously applies Handlebars template processing to the specified context, data, and target stream.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <param name="data">The data to be used during template processing.</param>
    /// <param name="target">The target stream where the processed template is written.</param>
    /// <returns>A task representing the asynchronous operation. The task result is <c>true</c> if successful, <c>false</c> otherwise.</returns>
    public async Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target)
    {
        using var template = context.OpenTemplate(context);
        using var reader = new StreamReader(template);
        var text = await reader.ReadToEndAsync().ConfigureAwait(false);
        var compiled = Compile(text);

        using var writer = new StreamWriter(target, leaveOpen: true)
        {
            AutoFlush = true,
        };
        compiled(writer, context: data, data: context);
        await writer.FlushAsync().ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Gets the number of compiled templates currently cached.
    /// </summary>
    public int CachedTemplateCount => _cache.Count;

    private HandlebarsTemplate<TextWriter, object, object> Compile(string text)
    {
        var settings = options?.Value ?? new HandlebarsOptions();
        if (!settings.EnableCompilationCache)
            return Environment.Compile(new StringReader(text));

        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        if (_cache.TryGetValue(key, out var found))
            return found;

        var compiled = Environment.Compile(new StringReader(text));
        if (_cache.Count >= Math.Max(1, settings.MaxCachedTemplates))
            _cache.Clear();
        _cache[key] = compiled;
        return compiled;
    }

    private IHandlebars CreateEnvironment()
    {
        var config = new HandlebarsConfiguration()
        {
            NoEscape = options?.Value.NoEscape ?? true,
        };

        // https://github.com/Handlebars-Net/Handlebars.Net
        var handlebar = HandlebarsDotNet.Handlebars.Create(config);
        handlebar.Configuration.UseJson();

        foreach (var item in helpersRegistry ?? [])
        {
            foreach (var helper in item.GetBlockHelpers())
            {
                handlebar.RegisterHelper(helper.Value);
            }
            foreach (var helper in item.GetHelpers())
            {
                handlebar.RegisterHelper(helper.Value);
            }
        }

        foreach (var helper in blockHelperDescriptors)
        {
            handlebar.RegisterHelper(helper);
        }
        foreach (var helper in helperDescriptors)
        {
            handlebar.RegisterHelper(helper);
        }

        return handlebar;
    }
}
