using System;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Data.DesiredState;

/// <summary>Resolves a provider by engine key (keyed registrations).</summary>
public sealed class DesiredStateProviderFactory(IServiceProvider services)
{
    public IDesiredStateProvider Create(string engine)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(engine);
        return services.GetKeyedService<IDesiredStateProvider>(engine.ToLowerInvariant())
            ?? throw new NotSupportedException($"No desired-state provider for '{engine}'.");
    }
}
