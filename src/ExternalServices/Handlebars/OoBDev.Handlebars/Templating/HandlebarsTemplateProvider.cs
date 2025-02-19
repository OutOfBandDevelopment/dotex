using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;
using HandlebarsDotNet.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
public class HandlebarsTemplateProvider(
    IEnumerable<IHelpersRegistry> helpersRegistry,
    IEnumerable<IHelperDescriptor<BlockHelperOptions>> blockHelperDescriptors,
    IEnumerable<IHelperDescriptor<HelperOptions>> helperDescriptors,
    ILogger<HandlebarsTemplateProvider> log
    ) : ITemplateProvider
{
    private readonly ILogger _log = log;

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
        SupportedContentTypes.Any(type => string.Equals(context.TemplateContentType, type, StringComparison.InvariantCultureIgnoreCase));

    /// <summary>
    /// Asynchronously applies Handlebars template processing to the specified context, data, and target stream.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <param name="data">The data to be used during template processing.</param>
    /// <param name="target">The target stream where the processed template is written.</param>
    /// <returns>A task representing the asynchronous operation. The task result is <c>true</c> if successful, <c>false</c> otherwise.</returns>
    public async Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target)
    {
        var config = new HandlebarsConfiguration()
        {
            NoEscape = true, //TODO: make this configurable
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

        using var template = context.OpenTemplate(context);
        var reader = new StreamReader(template);
        var compiled = handlebar.Compile(reader);

        using var writer = new StreamWriter(target, leaveOpen: true)
        {
            AutoFlush = true,
        };
        compiled(writer, context: data, data: context);
        await writer.FlushAsync();
        return true;
    }
}
