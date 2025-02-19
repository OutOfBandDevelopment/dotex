using OoBDev.System.IO;
using OoBDev.System.Linq;
using OoBDev.System.Net.Mime;
using OoBDev.System.Providers;
using OoBDev.System.Security.Cryptography;
using OoBDev.System.Text;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.System.Text.Templating;
using OoBDev.System.Text.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.System;

/// <summary>
/// Suggested IOC configurations
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// This will add all available extensions to the IOC container
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddSystemExtensions(
        this IServiceCollection services,
        IConfiguration config,
#if DEBUG
        SystemExtensionBuilder? builder
#else
        SystemExtensionBuilder? builder = default
#endif
        )
    {
        builder ??= new();
        services.TryAddSearchQueryExtensions(builder.StringCasingOrder);
        services.TryTemplatingExtensions(config, builder.FileTemplatingConfigurationSection);
        services.TrySecurityExtensions(builder.DefaultHashType);
        services.TrySerializerExtensions(builder.DefaultSerializerType);
        services.TryAddProviders();
        ;
        return services;
    }

    /// <summary>
    /// Add support for shared security extensions
    /// </summary>
    /// <param name="services"></param>
    /// <param name="defaultHashType"></param>
    /// <returns></returns>
    public static IServiceCollection TrySecurityExtensions(
        this IServiceCollection services,
#if DEBUG
        HashTypes defaultHashType
#else
        HashTypes defaultHashType = HashTypes.Md5
#endif
        )
    {
        services.TryAddSingleton(sp => sp.GetRequiredKeyedService<IHash>(defaultHashType));

        services.TryAddKeyedSingleton<IHash, Md5Hash>(nameof(HashTypes.Md5).ToUpper());
        services.TryAddKeyedSingleton<IHash, Sha256Hash>(nameof(HashTypes.Sha256).ToUpper());
        services.TryAddKeyedSingleton<IHash, Sha512Hash>(nameof(HashTypes.Sha512).ToUpper());

        services.TryAddKeyedSingleton(HashTypes.Md5, (sp, key) => sp.GetRequiredKeyedService<IHash>(key?.ToString()?.ToUpper()));
        services.TryAddKeyedSingleton(HashTypes.Sha256, (sp, key) => sp.GetRequiredKeyedService<IHash>(key?.ToString()?.ToUpper()));
        services.TryAddKeyedSingleton(HashTypes.Sha512, (sp, key) => sp.GetRequiredKeyedService<IHash>(key?.ToString()?.ToUpper()));

        return services;
    }

    /// <summary>
    /// Add support for shared Serializer
    /// </summary>
    /// <param name="services"></param>
    /// <param name="defaultSerializerType"></param>
    /// <returns></returns>
    public static IServiceCollection TrySerializerExtensions(
        this IServiceCollection services,
#if DEBUG
        SerializerTypes defaultSerializerType
#else
        SerializerTypes defaultSerializerType = SerializerTypes.Json
#endif
        )
    {
        services.TryAddSingleton(sp => sp.GetRequiredKeyedService<ISerializer>(defaultSerializerType));

        services.TryAddKeyedSingleton(
            SerializerTypes.Json,
            (sp, key) => sp.GetRequiredKeyedService<ISerializer>(key?.ToString()?.ToUpper())
            );
        services.TryAddKeyedSingleton(
            SerializerTypes.Bson,
            (sp, key) => sp.GetRequiredKeyedService<ISerializer>(key?.ToString()?.ToUpper())
            );
        services.TryAddKeyedSingleton(
            SerializerTypes.Xml,
            (sp, key) => sp.GetRequiredKeyedService<ISerializer>(key?.ToString()?.ToUpper())
            );

        services.TryAddSingleton<IJsonSerializer, DefaultJsonSerializer>();
        services.TryAddKeyedSingleton<ISerializer>(
            nameof(SerializerTypes.Json).ToUpper(),
            (sp, key) => sp.GetRequiredService<IJsonSerializer>()
            );

        services.TryAddSingleton<IBsonSerializer, DefaultBsonSerializer>();
        services.TryAddKeyedSingleton<ISerializer>(
            nameof(SerializerTypes.Bson).ToUpper(),
            (sp, key) => sp.GetRequiredService<IBsonSerializer>()
            );

        services.TryAddSingleton<IXmlSerializer, DefaultXmlSerializer>();
        services.TryAddKeyedSingleton<ISerializer>(
            nameof(SerializerTypes.Xml).ToUpper(),
            (sp, key) => sp.GetRequiredService<IXmlSerializer>()
            );

        services.TryAddSingleton(_ => DefaultJsonSerializer.DefaultOptions);
        return services;
    }

    /// <summary>
    /// Add support for shared Templating
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to add services to.</param>
    /// <param name="configurationSection"></param>
    /// <returns></returns>
    public static IServiceCollection TryTemplatingExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string configurationSection
#else
        string configurationSection = nameof(FileTemplatingOptions)
#endif
        )
    {
        services.TryAddTransient<ITemplateEngine, TemplateEngine>();

        services.AddTransient<ITemplateSource, FileTemplateSource>();

        services.Configure<FileTemplatingOptions>(options => configuration.Bind(configurationSection, options));

        services.AddTransient<ITemplateProvider, XsltTemplateProvider>();

        services.AddTransient<IFileType>(_ => new FileType { Extension = ".md", ContentType = ContentTypesExtensions.Text.Markdown, IsTemplateType = false });
        services.AddTransient<IFileType>(_ => new FileType { Extension = ".yaml", ContentType = ContentTypesExtensions.Text.Yaml, IsTemplateType = false });

        services.AddTransient<IFileType>(_ => new FileType { Extension = ".html", ContentType = ContentTypesExtensions.Text.Html, IsTemplateType = false });
        services.AddTransient<IFileType>(_ => new FileType { Extension = ".txt", ContentType = ContentTypesExtensions.Text.Plain, IsTemplateType = false });
        services.AddTransient<IFileType>(_ => new FileType { Extension = ".json", ContentType = DefaultJsonSerializer.DefaultContentType, IsTemplateType = false });
        services.AddTransient<IFileType>(_ => new FileType { Extension = ".xml", ContentType = DefaultXmlSerializer.DefaultContentType, IsTemplateType = false });

        services.AddTransient<IFileType>(_ => new FileType { Extension = ".xslt", ContentType = ContentTypesExtensions.Application.XSLT, IsTemplateType = true });

        return services;
    }

    /// <summary>
    /// Add support for type providers
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddProviders(
        this IServiceCollection services
        )
    {
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.TryAddSingleton<IGuidProvider, GuidProvider>();

        services.TryAddSingleton<ITempFileFactory, TempFileFactory>();

        return services;
    }
}
