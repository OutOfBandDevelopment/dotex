namespace OoBDev.Scpi;

/// <summary>
/// The data type of a <see cref="ScpiParameterDefinition"/>.
/// </summary>
public enum ScpiParameterKind
{
    /// <summary>A bounded number.</summary>
    Numeric,

    /// <summary>One of a fixed set of options.</summary>
    Choice,

    /// <summary>Free text.</summary>
    Text,
}
