using System.Globalization;

namespace OoBDev.System.Numerics;

/// <summary>
/// The SI prefix symbols used by the engineering and byte notations, indexed by the power of the base (1000 or 1024).
/// </summary>
internal static class Symbols
{
    private const int Lowest = -8;

    private static readonly string[] _symbols = ["y", "z", "a", "f", "p", "n", "μ", "m", "", "k", "M", "G", "T", "P", "E", "Z", "Y"];

    /// <summary>Gets the symbol for a power, or <see langword="null"/> when the power has no symbol.</summary>
    public static string? ForPower(int power) =>
        power is >= Lowest and <= -Lowest ? _symbols[power - Lowest] : null;

    /// <summary>Finds the power of a one-character symbol; the empty (zero power) symbol is not a match.</summary>
    public static bool TryGetPower(char symbol, out int power)
    {
        for (var index = 0; index < _symbols.Length; index++)
        {
            if (_symbols[index].Length == 1 && _symbols[index][0] == symbol)
            {
                power = index + Lowest;
                return true;
            }
        }

        power = 0;
        return false;
    }

    /// <summary>Formats a scaled value with an invariant culture and a default of three decimals.</summary>
    public static string Scaled(double value, string? format) =>
        value.ToString(string.IsNullOrEmpty(format) ? "0.###" : format, CultureInfo.InvariantCulture);
}
