namespace OoBDev.System.Text.Json.Serialization;

/// <summary>
/// Represents an interface to identify a shared JSON serialization process.
/// </summary>
public interface IJsonSerializer : ISerializer
{
    /// <summary>
    /// Converts the provided property name according to the configured property naming policy.
    /// </summary>
    /// <param name="propertyName">The original property name.</param>
    /// <returns>The converted property name.</returns>
    string AsPropertyName(string propertyName);
}
