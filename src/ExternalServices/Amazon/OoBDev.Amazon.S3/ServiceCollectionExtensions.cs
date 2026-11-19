using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Documents.Containers;

namespace OoBDev.Amazon.S3;

/// <summary>Registers the S3 blob container provider.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the S3 blob container provider factory; it is offered while <c>Region</c> or <c>ServiceUrl</c> is set.</summary>
    /// <param name="services">The services.</param>
    /// <param name="configuration">The configuration holding the options section.</param>
    /// <param name="sectionName">The options section.</param>
    /// <returns>The services.</returns>
    public static IServiceCollection TryAddAwsS3BlobServices(this IServiceCollection services, IConfiguration configuration, string sectionName = nameof(S3BlobOptions))
    {
        services.AddOptions<S3BlobOptions>().Bind(configuration.GetSection(sectionName));
        services.TryAddSingleton<IS3ClientFactory, S3ClientFactory>();
        services.TryAddSingleton<IAmazonS3>(sp => sp.GetRequiredService<IS3ClientFactory>().Create());
        services.AddTransient<IBlobContainerProviderFactory, S3BlobContainerProviderFactory>();
        services.AddKeyedTransient<IBlobContainerProviderFactory, S3BlobContainerProviderFactory>(AwsS3Globals.ProviderName);
        return services;
    }
}
