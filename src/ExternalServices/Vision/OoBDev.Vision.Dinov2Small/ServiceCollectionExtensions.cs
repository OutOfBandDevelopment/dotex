using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Onnx.ImageEmbeddings;

namespace OoBDev.Vision.Dinov2Small;

/// <summary>Registration of the in-process DINOv2-small image embedding generator.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the keyed DINOv2-small generator (key <see cref="VisionGlobals.Dinov2SmallKey"/>); needs an
    /// <see cref="IImageDecoder"/> (for example <c>TryAddSkiaImageDecoder()</c>).
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing <see cref="OnnxImageEmbeddingOptions"/>.</param>
    /// <param name="optionSection">The configuration section name for the options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddDinov2SmallServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string optionSection
#else
        string optionSection = VisionGlobals.DefaultSection
#endif
        )
    {
        services.TryAddKeyedSingleton(VisionGlobals.Dinov2SmallKey, (sp, _) =>
        {
            var options = new OnnxImageEmbeddingOptions();
            Dinov2SmallModel.ApplyDefaults(options);
            configuration.Bind(optionSection, options);
            return new OnnxImageEmbeddingGenerator(Options.Create(options), sp.GetRequiredService<IImageDecoder>(), sp.GetRequiredService<ILogger<OnnxImageEmbeddingGenerator>>());
        });
        services.TryAddKeyedSingleton<IEmbeddingGenerator<DataContent, Embedding<float>>>(
            VisionGlobals.Dinov2SmallKey, (sp, key) => sp.GetRequiredKeyedService<OnnxImageEmbeddingGenerator>(key));
        return services;
    }
}
