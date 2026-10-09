using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Onnx.ImageEmbeddings;

namespace OoBDev.Vision.VitBasePatch16;

/// <summary>Registration of the in-process ViT-base-patch16-224 image classifier.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the keyed ViT-base-patch16-224 classifier (key <see cref="VisionGlobals.VitBasePatch16Key"/>); needs an
    /// <see cref="IImageDecoder"/> (for example <c>TryAddSkiaImageDecoder()</c>).
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration containing <see cref="OnnxImageEmbeddingOptions"/>.</param>
    /// <param name="optionSection">The configuration section name for the options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection TryAddVitBasePatch16Services(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string optionSection
#else
        string optionSection = VisionGlobals.DefaultSection
#endif
        )
    {
        services.TryAddKeyedSingleton(VisionGlobals.VitBasePatch16Key, (sp, _) =>
        {
            var options = new OnnxImageEmbeddingOptions();
            VitBasePatch16Model.ApplyDefaults(options);
            configuration.Bind(optionSection, options);
            var logger = sp.GetRequiredService<ILogger<OnnxImageClassifier>>();
            var folder = OnnxImageEmbeddingGenerator.EnsureModelAsync(options, logger).GetAwaiter().GetResult();
            return new OnnxImageClassifier(Options.Create(options), VitBasePatch16Model.LoadLabels(folder), sp.GetRequiredService<IImageDecoder>(), logger);
        });
        services.TryAddKeyedSingleton<IImageClassifier>(
            VisionGlobals.VitBasePatch16Key, (sp, key) => sp.GetRequiredKeyedService<OnnxImageClassifier>(key));
        return services;
    }
}
