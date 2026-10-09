using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OoBDev.Onnx.ImageEmbeddings.Skia;

/// <summary>Registration for the SkiaSharp image decoder.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="SkiaImageDecoder"/> as the <see cref="IImageDecoder"/> unless one is already registered.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection TryAddSkiaImageDecoder(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<IImageDecoder, SkiaImageDecoder>();
        return services;
    }
}
