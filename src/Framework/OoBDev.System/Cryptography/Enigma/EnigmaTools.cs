using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OoBDev.System.Cryptography.Enigma;

/// <summary>
/// Provides utility extension methods for processing text in Enigma cipher operations.
/// </summary>
public static class EnigmaTools
{
    /// <summary>
    /// Cleans the input by converting to uppercase and removing non-alphabetic characters.
    /// </summary>
    /// <param name="input">The character sequence to clean.</param>
    /// <returns>A sequence of uppercase letters (A-Z) only.</returns>
    public static IEnumerable<char> Clean(this IEnumerable<char> input) =>
        input.Select(c => (char)(c > 'Z' ? c - 32 : c))
                    .Where(c => c >= 'A' && c <= 'Z');

    /// <summary>
    /// Converts a character sequence to a string.
    /// </summary>
    /// <param name="input">The character sequence to convert.</param>
    /// <returns>A string composed of the input characters.</returns>
    public static string AsString(this IEnumerable<char> input) =>
        new([.. input]);

    /// <summary>
    /// Splits a string into chunks of the specified length.
    /// </summary>
    /// <param name="input">The string to split.</param>
    /// <param name="at">The length of each chunk (default is 2 for digraphs).</param>
    /// <returns>An enumerable of string chunks.</returns>
    public static IEnumerable<string> SplitAt(this string input, int at = 2) =>
        Enumerable.Range(0, input.Length / at)
                  .Select(i => input.Substring(i * at, at));

    /// <summary>
    /// Applies a set of character swaps to the input string (used for plugboard processing).
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <param name="swaps">An array of 2-character strings defining character pairs to swap (can be null for no swaps).</param>
    /// <returns>The string with all specified character swaps applied.</returns>
    internal static string SwapSet(this string input, string[]? swaps)
    {
        return swaps == null
            ? input
            : swaps.Aggregate(new StringBuilder(input ?? ""),
                               (sb, s) => sb.Replace(s[0], '_')
                                            .Replace(s[1], s[0])
                                            .Replace('_', s[1]),
                               sb => sb.ToString());
    }
}
