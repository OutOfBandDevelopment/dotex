namespace OoBDev.SBert;

/// <summary>
/// Provider keys of the SBert adapter.
/// </summary>
public static class SBertProviderGlobals
{
    /// <summary>
    /// The kebab-case key the SBert services are registered under.
    /// </summary>
    public const string ProviderKey = "sbert";

    /// <summary>
    /// The earlier key, still registered so existing configuration keeps working.
    /// </summary>
    public const string LegacyKey = "SBERT";
}
