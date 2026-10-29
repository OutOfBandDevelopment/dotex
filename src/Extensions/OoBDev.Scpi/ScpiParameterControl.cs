namespace OoBDev.Scpi;

/// <summary>
/// A widget hint for a parameter, independent of its <see cref="ScpiParameterKind"/>. The package only carries it; a
/// user interface decides what to do with it.
/// </summary>
public enum ScpiParameterControl
{
    /// <summary>A bounded number field.</summary>
    Numeric,

    /// <summary>A drag slider over the minimum and maximum.</summary>
    Slider,

    /// <summary>A plain text field.</summary>
    Text,

    /// <summary>One of the parameter's options.</summary>
    Choice,
}
