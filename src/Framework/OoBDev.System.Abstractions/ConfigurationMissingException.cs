using System;

namespace OoBDev.System;

/// <summary>
/// Represents an exception thrown when a required configuration is missing.
/// </summary>
/// <remarks>
/// This exception is typically thrown when attempting to access a configuration value that is not present.
/// </remarks>
public class ConfigurationMissingException : ApplicationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationMissingException"/> class with the specified configuration key.
    /// </summary>
    /// <param name="configurationKey">The key of the missing configuration.</param>
    /// <remarks>
    /// This constructor sets the exception message to indicate the missing configuration key.
    /// </remarks>
    public ConfigurationMissingException(string configurationKey) :
            base($"Configuration \"{configurationKey}\" is missing")
    {
    }
}
