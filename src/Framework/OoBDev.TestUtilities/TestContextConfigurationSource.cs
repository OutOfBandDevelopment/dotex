using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Represents MSTest TestContext as an <see cref="IConfigurationSource"/>.
/// </summary>
public class TestContextConfigurationSource : IConfigurationSource
{
    /// <summary>
    /// Gets the TestContext to read properties from.
    /// </summary>
    public TestContext TestContext { get; }

    /// <summary>
    /// Gets the optional prefix to filter properties by.
    /// </summary>
    public string? Prefix { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestContextConfigurationSource"/> class.
    /// </summary>
    /// <param name="testContext">The TestContext to read properties from.</param>
    /// <param name="prefix">Optional prefix to filter properties by.</param>
    public TestContextConfigurationSource(TestContext testContext, string? prefix = null)
    {
        TestContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
        Prefix = prefix;
    }

    /// <summary>
    /// Builds the <see cref="IConfigurationProvider"/> for this source.
    /// </summary>
    /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
    /// <returns>A <see cref="TestContextConfigurationProvider"/>.</returns>
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new TestContextConfigurationProvider(this);
    }
}
