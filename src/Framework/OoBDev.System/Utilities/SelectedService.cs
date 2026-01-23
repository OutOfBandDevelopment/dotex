using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OoBDev.System.Utilities;

/// <summary>
/// Resolves and selects a service implementation based on configuration, supporting keyed service selection.
/// </summary>
/// <typeparam name="TService">The service type to select.</typeparam>
public class SelectedService<TService> : ISelectedService<TService> where TService : notnull
{
    /// <summary>
    /// Initializes a new instance of the SelectedService class, resolving the service based on configuration.
    /// </summary>
    /// <param name="configuration">The configuration to use for service key lookup.</param>
    /// <param name="serviceProvider">The service provider to resolve the selected service from.</param>
    public SelectedService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        var serviceKey = $"OoBDev::ServiceKeys::{typeof(TService).FullName}";
        var selectedServiceKey = configuration[serviceKey];
        Value = serviceProvider.GetKeyedService<TService>(selectedServiceKey)
                            ?? serviceProvider.GetRequiredService<TService>();
    }

    /// <summary>
    /// Gets the selected service instance.
    /// </summary>
    public TService Value { get; }
}
