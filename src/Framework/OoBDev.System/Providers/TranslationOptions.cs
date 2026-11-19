using System;
using System.Collections.Generic;

namespace OoBDev.System.Providers;

/// <summary>
/// Translation entries by culture name (empty name = the neutral fallback) and then by key.
/// </summary>
public class TranslationOptions
{
    /// <summary>
    /// Gets or initializes the entries: culture name, then key, then text.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> Cultures { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
