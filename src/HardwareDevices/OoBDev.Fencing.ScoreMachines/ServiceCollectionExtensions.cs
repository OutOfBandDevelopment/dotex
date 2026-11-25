using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Fencing.ScoreMachines.Favero;
using OoBDev.Fencing.ScoreMachines.SaintGeorge;
using OoBDev.System.IO.Messages;
using System;

namespace OoBDev.Fencing.ScoreMachines;

/// <summary>Registration for the fencing score machine package.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers the keyed message decoders (<see cref="FencingGlobals.FaveroKey"/> and <see cref="FencingGlobals.SaintGeorgeKey"/>).</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddFencingScoreMachines(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddKeyedSingleton<IMessageDecoder<IScoreMachineState>, FaveroMessageDecoder>(FencingGlobals.FaveroKey);
        services.TryAddKeyedSingleton<IMessageDecoder<IScoreMachineState>, SgMessageDecoder>(FencingGlobals.SaintGeorgeKey);
        return services;
    }
}
