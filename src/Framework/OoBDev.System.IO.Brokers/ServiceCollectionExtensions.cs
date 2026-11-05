using Microsoft.Extensions.DependencyInjection;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.IO.Brokers;

/// <summary>Registers the broker device adapters.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="BrokerDeviceAdapter"/> over AMQP 0-9-1. Callers still configure <see cref="BrokerDeviceAdapterOptions"/> themselves.</summary>
    public static IServiceCollection AddAmqpDeviceAdapter(this IServiceCollection services) => Add(services, new AmqpConnectionFactory());

    /// <summary>Registers <see cref="BrokerDeviceAdapter"/> over STOMP 1.2. Callers still configure <see cref="BrokerDeviceAdapterOptions"/> themselves.</summary>
    public static IServiceCollection AddStompDeviceAdapter(this IServiceCollection services) => Add(services, new StompConnectionFactory());

    private static IServiceCollection Add(IServiceCollection services, IBrokerConnectionFactory factory)
    {
        services.AddOptions<BrokerDeviceAdapterOptions>().ValidateDataAnnotations();
        services.AddSingleton(factory);
        services.AddTransient<IDeviceAdapter, BrokerDeviceAdapter>();
        return services;
    }
}
