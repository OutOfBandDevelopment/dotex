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
}
