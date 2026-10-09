namespace OoBDev.GroqCloud;

/// <summary>
/// Provider keys of the Groq Cloud adapter.
/// </summary>
public static class GroqCloudGlobals
{
    /// <summary>
    /// The kebab-case key the Groq Cloud services are registered under.
    /// </summary>
    public const string ProviderKey = "groq-cloud";

    /// <summary>
    /// The earlier key, still registered so existing configuration keeps working.
    /// </summary>
    public const string LegacyKey = "GroqCloud";
}
