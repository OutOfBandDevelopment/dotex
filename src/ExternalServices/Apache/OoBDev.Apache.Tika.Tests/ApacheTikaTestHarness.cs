using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace OoBDev.Apache.Tika.Tests;

public static class ApacheTikaTestHarness
{
    public static IServiceProvider GetServiceProvider(Dictionary<string, string?> config, Action<IServiceCollection>? update = default) =>
        GetServiceProvider(
            new ConfigurationBuilder()
            .AddInMemoryCollection(config)
            .Build(), update);

    public static IServiceProvider GetServiceProvider(IConfiguration config, Action<IServiceCollection>? update = default)
    {
        var serviceCollection = new ServiceCollection()
            .TryAddApacheTikaServices(config, nameof(ApacheTikaClientOptions))
            ;

        serviceCollection.AddLogging(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Debug)
            );

        update?.Invoke(serviceCollection);

        var service = serviceCollection.BuildServiceProvider();
        return service;
    }

    public static T Create<T>(this IServiceProvider serviceProvider, params object[] parameters) where T : class =>
        ActivatorUtilities.CreateInstance<T>(serviceProvider, parameters);
}
