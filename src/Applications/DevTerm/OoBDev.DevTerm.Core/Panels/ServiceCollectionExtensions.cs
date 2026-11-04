using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Panels;

/// <summary>Registration for device panels contributed by device libraries.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TPanel"/> and everything a front end needs for it: the <see cref="IDevicePanelContribution"/>
    /// for its menus and a transient <see cref="IPresenter"/> for its decoder (a fresh decoder per resolution).
    /// </summary>
    /// <typeparam name="TPanel">The device library's panel.</typeparam>
    /// <param name="services">The service collection.</param>
    public static IServiceCollection TryAddDevicePanel<TPanel>(this IServiceCollection services)
        where TPanel : class, IDevicePanel
    {
        ArgumentNullException.ThrowIfNull(services);
        if (services.Any(d => d.ServiceType == typeof(TPanel)))
        {
            return services;
        }

        services.AddSingleton<TPanel>();
        services.AddSingleton<IDevicePanelContribution>(sp => new DevicePanelContribution(sp.GetRequiredService<TPanel>()));
        services.AddTransient<IPresenter>(sp => new DeviceDecoderPresenter(sp.GetRequiredService<TPanel>().CreateDecoder()));
        return services;
    }
}
