using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OoBDev.System.Utilities;

public class SelectedService<TService> : ISelectedService<TService> where TService : notnull
{
    public SelectedService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        var serviceKey = $"OoBDev::ServiceKeys::{typeof(TService).FullName}";
        var selectedServiceKey = configuration[serviceKey];
        Value = serviceProvider.GetKeyedService<TService>(selectedServiceKey)
                            ?? serviceProvider.GetRequiredService<TService>();
    }

    public TService Value { get; }
}
