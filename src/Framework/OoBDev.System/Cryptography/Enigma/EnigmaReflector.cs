using System.Collections.Generic;

namespace OoBDev.System.Cryptography.Enigma;

/// <summary>
/// Represents an Enigma reflector configuration with wiring specifications.
/// The reflector redirects electrical signals back through the rotors in reverse, ensuring the cipher is reciprocal.
/// </summary>
public record EnigmaReflector
{
    /// <summary>
    /// Gets a collection of historical Enigma reflector configurations (A, B, C, B Thin, C Thin).
    /// </summary>
    public static IEnumerable<EnigmaReflector> Reflectors { get; } =
    [
        new EnigmaReflector {Number="Reflector A", Series="",Wiring="EJMZALYXVBWFCRQUONTSPIKHGD",Introduced=""},
        new EnigmaReflector {Number="Reflector B", Series="",Wiring="YRUHQSLDPXNGOKMIEBFZCWVJAT",Introduced=""},
        new EnigmaReflector {Number="Reflector C", Series="",Wiring="FVPJIAOYEDRZXWGCTKUQSBNMHL",Introduced=""},
        new EnigmaReflector {Number="Reflector B Thin", Series="M4 R1 (M3 + Thin)",Wiring="ENKQAUYWJICOPBLMDXZVFTHRGS",Introduced="1940"},
        new EnigmaReflector {Number="Reflector C Thin", Series="M4 R1 (M3 + Thin)",Wiring="RDOBJNTKVEHMLFCWZAXGYIPSUQ",Introduced="1940"},
    ];

    /// <summary>
    /// Gets or initializes the year or date when this reflector was introduced.
    /// </summary>
    public required string Introduced { get; init; }

    /// <summary>
    /// Gets or initializes the reflector designation/number (e.g., "Reflector A", "Reflector B").
    /// </summary>
    public required string Number { get; init; }

    /// <summary>
    /// Gets or initializes the Enigma series this reflector was used with (e.g., "M4 R1 (M3 + Thin)").
    /// </summary>
    public required string Series { get; init; }

    /// <summary>
    /// Gets or initializes the wiring specification as a 26-letter string defining the reflector's electrical connections.
    /// </summary>
    public required string Wiring { get; init; }
}
