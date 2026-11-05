using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.IO.Mqtt;

/// <summary>Registers the MQTT device adapter.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="MqttDeviceAdapter"/> as <see cref="IDeviceAdapter"/>. Callers still configure <see cref="MqttDeviceAdapterOptions"/> themselves.</summary>
    public static IServiceCollection AddMqttDeviceAdapter(this IServiceCollection services)
    {
        services.AddOptions<MqttDeviceAdapterOptions>().ValidateDataAnnotations();
        services.TryAddSingleton<IMqttConnectionFactory, MqttNetConnectionFactory>();
        services.AddTransient<IDeviceAdapter, MqttDeviceAdapter>();
        return services;
    }
}
