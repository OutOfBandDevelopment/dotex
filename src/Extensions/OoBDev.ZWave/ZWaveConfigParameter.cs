using System.Collections.Generic;

namespace OoBDev.ZWave;

/// <summary>
/// One configuration parameter (command class 0x70) a manufacturer documents for a product.
/// </summary>
/// <param name="Index">The parameter number.</param>
/// <param name="Label">Short name.</param>
/// <param name="Size">Value size in bytes (1, 2 or 4).</param>
/// <param name="Minimum">Lowest allowed value, when documented.</param>
/// <param name="Maximum">Highest allowed value, when documented.</param>
/// <param name="Default">Factory value, when documented.</param>
/// <param name="Help">Longer description, when documented.</param>
/// <param name="Items">Named values for list parameters; empty otherwise.</param>
public sealed record ZWaveConfigParameter(
    int Index,
    string Label,
    int Size,
    long? Minimum,
    long? Maximum,
    long? Default,
    string? Help,
    IReadOnlyList<KeyValuePair<long, string>> Items);
