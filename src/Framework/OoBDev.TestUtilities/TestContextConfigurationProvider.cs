using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// A configuration provider that reads configuration values from MSTest TestContext.Properties.
/// </summary>
public class TestContextConfigurationProvider : ConfigurationProvider
{
    private readonly TestContextConfigurationSource _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestContextConfigurationProvider"/> class.
    /// </summary>
    /// <param name="source">The configuration source.</param>
    public TestContextConfigurationProvider(TestContextConfigurationSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>
    /// Loads configuration values from TestContext.Properties.
    /// </summary>
    public override void Load()
    {
        var data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in _source.TestContext.Properties)
        {
            var key = entry.Key;
            if (string.IsNullOrEmpty(key))
                continue;

            // Apply prefix filter if specified
            if (!string.IsNullOrEmpty(_source.Prefix))
            {
                if (!key.StartsWith(_source.Prefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Remove prefix from key
                key = key.Substring(_source.Prefix.Length);

                // Remove separator after prefix (__, :, or _)
                if (key.StartsWith("__", StringComparison.Ordinal))
                {
                    key = key.Substring(2);
                }
                else if (key.StartsWith(":", StringComparison.Ordinal) ||
                         key.StartsWith("_", StringComparison.Ordinal))
                {
                    key = key.Substring(1);
                }
            }

            // Normalize key: replace double underscores with colons
            // This allows cross-platform safe hierarchical configuration
            key = key.Replace("__", ":", StringComparison.Ordinal);

            var value = entry.Value?.ToString();
            data[key] = value;
        }

        Data = data;
    }
}
