using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace OoBDev.AI;

/// <summary>Registration for the estimated LLM usage decorators.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers one <see cref="LlmUsageMeter"/> and wraps every already registered (plain or keyed) <see cref="IMessageCompletion"/> and
    /// <see cref="IStreamedCompletion"/> service so each call is counted. Call it after the adapter registrations;
    /// calling it twice does not wrap twice.
    /// </summary>
    public static IServiceCollection TryAddEstimatedLlmUsage(this IServiceCollection services)
    {
        if (services.Any(d => d.ServiceType == typeof(LlmUsageMeter)))
        {
            return services;
        }

        services.TryAddSingleton<LlmUsageMeter>();
        Decorate<IMessageCompletion>(services, (inner, sp) => new EstimatingMessageCompletion(inner, sp.GetRequiredService<LlmUsageMeter>()));
        Decorate<IStreamedCompletion>(services, (inner, sp) => new EstimatingStreamedCompletion(inner, sp.GetRequiredService<LlmUsageMeter>()));
        return services;
    }

    private static void Decorate<T>(IServiceCollection services, Func<T, IServiceProvider, T> wrap)
        where T : class
    {
        foreach (var original in services.Where(d => d.ServiceType == typeof(T)).ToList())
        {
            var index = services.IndexOf(original);
            services.RemoveAt(index);
            services.Insert(index, original.IsKeyedService
                ? ServiceDescriptor.DescribeKeyed(typeof(T), original.ServiceKey, (sp, key) => wrap(CreateKeyed<T>(original, sp, key), sp), original.Lifetime)
                : ServiceDescriptor.Describe(typeof(T), sp => wrap(Create<T>(original, sp), sp), original.Lifetime));
        }
    }

    private static T CreateKeyed<T>(ServiceDescriptor descriptor, IServiceProvider sp, object? key) =>
        (T)(descriptor.KeyedImplementationInstance
            ?? descriptor.KeyedImplementationFactory?.Invoke(sp, key)
            ?? ActivatorUtilities.CreateInstance(sp, descriptor.KeyedImplementationType!));

    private static T Create<T>(ServiceDescriptor descriptor, IServiceProvider sp) =>
        (T)(descriptor.ImplementationInstance
            ?? descriptor.ImplementationFactory?.Invoke(sp)
            ?? ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!));
}
