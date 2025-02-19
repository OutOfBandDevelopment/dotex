using OoBDev.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Extensions.Configuration;

/// <summary>
/// Extension methods for adding in-memory collections to the <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds an in-memory collection to the <see cref="IConfigurationBuilder"/> using the specified initial data.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add the in-memory collection to.</param>
    /// <param name="initialData">The initial data to populate the in-memory collection.</param>
    /// <returns>The modified <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddInMemoryCollection(
        this IConfigurationBuilder configurationBuilder,
        IEnumerable<(string key, string? value)> initialData) =>
        configurationBuilder.AddInMemoryCollection(
            initialData
            .GroupBy(i => i.key)
            .ToDictionary(i => i.Key, i => i.First().value)
            );

    /// <summary>
    /// Adds an in-memory collection to the <see cref="IConfigurationBuilder"/> using the specified initial data.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder"/> to add the in-memory collection to.</param>
    /// <param name="item">The first item of the in-memory collection.</param>
    /// <param name="initialData">Additional initial data to populate the in-memory collection.</param>
    /// <returns>The modified <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddInMemoryCollection(
        this IConfigurationBuilder configurationBuilder,
        (string key, string? value) item,
        params (string key, string? value)[] initialData) =>
        configurationBuilder.AddInMemoryCollection(
            new[] { item }.Concat(initialData)
            );
}
