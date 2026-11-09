namespace OoBDev.Handlebars;

/// <summary>
/// Options of the Handlebars template provider.
/// </summary>
public class HandlebarsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether <c>{{value}}</c> is written as is. When <c>false</c>, the output is
    /// HTML-encoded unless a helper writes a safe string or the template uses <c>{{{value}}}</c>. Default is <c>true</c>
    /// because the engine renders text, not only HTML.
    /// </summary>
    public bool NoEscape { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether compiled templates are kept. The cache key is the SHA-256 of the
    /// template text, so an edited template is compiled again and no invalidation is needed. Default is <c>true</c>.
    /// </summary>
    public bool EnableCompilationCache { get; set; } = true;

    /// <summary>
    /// Gets or sets how many compiled templates are kept; the cache is cleared when it reaches this size. Default is 100.
    /// </summary>
    public int MaxCachedTemplates { get; set; } = 100;
}
