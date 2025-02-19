using OoBDev.Handlebars.Helpers;
using OoBDev.Handlebars.Templating;
using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Handlebars;

/// <summary>
/// Provides extension methods for configuring services related to Handlebars templates in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services related to Handlebars templates to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> with added Handlebars template services.</returns>
    public static IServiceCollection TryAddHandlebarServices(this IServiceCollection services)
    {
        // Add a transient service for Handlebars template provider.
        services.AddTransient<ITemplateProvider, HandlebarsTemplateProvider>();
        services.AddKeyedTransient<ITemplateProvider, HandlebarsTemplateProvider>("HANDLEBARS");

        // Add a transient service for FileType with default values for Handlebars templates.
        services.AddTransient<IFileType>(_ => new FileType { Extension = ".hbs", ContentType = ContentTypesExtensions.Text.HandlebarsTemplate, IsTemplateType = true });

        services.AddTransient<IHelperDescriptor<HelperOptions>, DateNowHelperDescriptor>();
        services.AddTransient<IHelperDescriptor<HelperOptions>, GetHelperDescriptor>();
        services.AddTransient<IHelperDescriptor<HelperOptions>, GuidNewHelperDescriptor>();
        services.AddTransient<IHelperDescriptor<HelperOptions>, HashHelperDescriptor>();
        services.AddTransient<IHelperDescriptor<HelperOptions>, SetHelperDescriptor>();
        services.AddTransient<IHelperDescriptor<HelperOptions>, StringReplaceHelperDescriptor>();

        return services;
    }
}
