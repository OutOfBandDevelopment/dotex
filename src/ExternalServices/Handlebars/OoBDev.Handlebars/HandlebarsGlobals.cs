namespace OoBDev.Handlebars;

/// <summary>
/// Provider keys of the Handlebars adapter.
/// </summary>
public static class HandlebarsGlobals
{
    /// <summary>
    /// The kebab-case key the Handlebars services are registered under.
    /// </summary>
    public const string ProviderKey = "handlebars";

    /// <summary>
    /// The earlier key, still registered so existing configuration keeps working.
    /// </summary>
    public const string LegacyKey = "HANDLEBARS";
}
