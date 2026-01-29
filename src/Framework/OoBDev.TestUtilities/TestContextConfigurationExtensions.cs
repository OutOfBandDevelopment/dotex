using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods for adding <see cref="TestContextConfigurationProvider"/> to an <see cref="IConfigurationBuilder"/>.
/// </summary>
public static class TestContextConfigurationExtensions
{
    /// <summary>
    /// Adds configuration from MSTest TestContext.Properties to the configuration builder.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="testContext">The TestContext to read properties from.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="testContext"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var config = new ConfigurationBuilder()
    ///     .AddJsonFile("appsettings.json")
    ///     .AddTestContext(TestContext)
    ///     .Build();
    /// </code>
    /// </example>
    public static IConfigurationBuilder AddTestContext(
        this IConfigurationBuilder builder,
        TestContext testContext)
    {
        return builder.AddTestContext(testContext, prefix: null);
    }

    /// <summary>
    /// Adds configuration from MSTest TestContext.Properties to the configuration builder,
    /// filtering properties by the specified prefix.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
    /// <param name="testContext">The TestContext to read properties from.</param>
    /// <param name="prefix">
    /// Optional prefix to filter properties by. Only properties starting with this prefix
    /// will be included, and the prefix will be removed from the resulting configuration keys.
    /// </param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="builder"/> or <paramref name="testContext"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// // Only load properties starting with "MyApp:"
    /// var config = new ConfigurationBuilder()
    ///     .AddTestContext(TestContext, prefix: "MyApp")
    ///     .Build();
    /// </code>
    /// </example>
    public static IConfigurationBuilder AddTestContext(
        this IConfigurationBuilder builder,
        TestContext testContext,
        string? prefix)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (testContext == null)
            throw new ArgumentNullException(nameof(testContext));

        return builder.Add(new TestContextConfigurationSource(testContext, prefix));
    }
}
