namespace OoBDev.Scpi;

/// <summary>
/// What a query's reply contains when it is not an ordinary text line.
/// </summary>
public enum ScpiResponseFormat
{
    /// <summary>An ordinary text line.</summary>
    Text,

    /// <summary>A bitmap in a definite-length block, for example a screen dump.</summary>
    Image,
}
