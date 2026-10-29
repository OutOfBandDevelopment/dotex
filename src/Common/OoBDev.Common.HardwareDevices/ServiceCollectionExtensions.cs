using Microsoft.Extensions.DependencyInjection;
using OoBDev.DerEE.De5000;
using OoBDev.Kuando.Busylight;
using OoBDev.Quarta.RadexOne;
using OoBDev.Velleman.K8055;
using System;

namespace OoBDev.Common.HardwareDevices;

/// <summary>
/// Provides extension methods for registering the hardware device packages in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add the message encoders and decoders of every hardware device package.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection TryCommonHardwareDeviceExtensions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddBusylight();
        services.TryAddDe5000();
        services.TryAddK8055();
        services.TryAddRadexOne();

        return services;
    }
}
