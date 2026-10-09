namespace OoBDev.Caching;

/// <summary>
/// Shared constants for caching provider selection.
/// </summary>
public static class CachingGlobals
{
    /// <summary>
    /// The configuration path holding the key of the caching provider to use (for example <c>redis</c>).
    /// When empty the default <see cref="ICachingProvider"/> registration is used.
    /// </summary>
    public const string ConfigurationPath = "OoBDev:CachingProvider:Type";

    /// <summary>
    /// The key the configuration-selected <see cref="ICachingProvider"/> is registered under.
    /// </summary>
    public const string SelectedKey = "selected";
}
