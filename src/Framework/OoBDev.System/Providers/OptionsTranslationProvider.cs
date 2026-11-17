using Microsoft.Extensions.Options;
using System;
using System.Globalization;

namespace OoBDev.System.Providers;

/// <summary>
/// Translation provider backed by <see cref="TranslationOptions"/> (typically bound from configuration).
/// </summary>
/// <param name="options">The translation entries.</param>
public class OptionsTranslationProvider(IOptions<TranslationOptions> options) : ITranslationProvider
{
    /// <inheritdoc />
    public string? Translate(string key, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(culture);

        var entries = options.Value.Cultures;
        for (var current = culture; ; current = current.Parent)
        {
            if (entries.TryGetValue(current.Name, out var texts) && texts.TryGetValue(key, out var text))
                return text;
            if (current.Name.Length == 0 || current.Equals(current.Parent))
                return null;
        }
    }
}
