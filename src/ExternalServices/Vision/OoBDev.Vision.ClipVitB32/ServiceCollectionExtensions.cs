using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Onnx.ImageEmbeddings;

namespace OoBDev.Vision.ClipVitB32;

/// <summary>Registration of the in-process CLIP ViT-B/32 image, text and zero-shot services.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the keyed CLIP image embedding generator, text embedding generator and zero-shot classifier
    /// (key <see cref="VisionGlobals.ClipVitB32Key"/>); needs an <see cref="IImageDecoder"/>
    /// (for example <c>TryAddSkiaImageDecoder()</c>).
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing the options.</param>
    /// <param name="optionSection">The configuration section name of the image tower options; the text tower uses <c>{optionSection}Text</c>.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddClipVitB32Services(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string optionSection
#else
        string optionSection = VisionGlobals.DefaultSection
#endif
        )
    {
        services.TryAddKeyedSingleton(VisionGlobals.ClipVitB32Key, (sp, _) =>
        {
            var options = new OnnxImageEmbeddingOptions();
            ClipVitB32Model.ApplyDefaults(options);
            configuration.Bind(optionSection, options);
            var logger = sp.GetRequiredService<ILogger<OnnxImageEmbeddingGenerator>>();
            OnnxImageEmbeddingGenerator.EnsureModelAsync(options, logger).GetAwaiter().GetResult();
            return new OnnxImageEmbeddingGenerator(Options.Create(options), sp.GetRequiredService<IImageDecoder>(), logger);
        });
        services.TryAddKeyedSingleton<IEmbeddingGenerator<DataContent, Embedding<float>>>(
            VisionGlobals.ClipVitB32Key, (sp, key) => sp.GetRequiredKeyedService<OnnxImageEmbeddingGenerator>(key));

        services.TryAddKeyedSingleton(VisionGlobals.ClipVitB32Key, (sp, _) =>
        {
            var options = new ClipTextOptions();
            ClipVitB32Model.ApplyDefaults(options);
            configuration.Bind(optionSection + "Text", options);
            ClipVitB32Model.EnsureAsync(options, sp.GetRequiredService<ILogger<ClipTextEmbeddingGenerator>>()).GetAwaiter().GetResult();
            return new ClipTextEmbeddingGenerator(options);
        });
        services.TryAddKeyedSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
            VisionGlobals.ClipVitB32Key, (sp, key) => sp.GetRequiredKeyedService<ClipTextEmbeddingGenerator>(key));

        services.TryAddKeyedSingleton(VisionGlobals.ClipVitB32Key, (sp, key) => new ZeroShotImageClassifier(
            sp.GetRequiredKeyedService<IEmbeddingGenerator<DataContent, Embedding<float>>>(key),
            sp.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(key)));
        return services;
    }
}
