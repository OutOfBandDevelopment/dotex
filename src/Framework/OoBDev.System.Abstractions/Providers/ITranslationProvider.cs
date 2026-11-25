using System.Globalization;

namespace OoBDev.System.Providers;

/// <summary>
/// Looks up server-side text by key and culture.
/// </summary>
public interface ITranslationProvider
{
    /// <summary>
    /// Gets the text for a key in a culture, falling back from the specific culture (fr-CA) to its parents (fr) and to the invariant entries.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <param name="culture">The culture to translate to.</param>
    /// <returns>The text, or null when no entry exists for the key.</returns>
    string? Translate(string key, CultureInfo culture);
}
