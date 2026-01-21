namespace OoBDev.DacFx;

/// <summary>
/// Specifies the source for SQL model options when merging multiple DacPac files.
/// </summary>
public enum ModelOptionSource
{
    /// <summary>
    /// Use custom-defined model options.
    /// </summary>
    Custom,

    /// <summary>
    /// Use model options from the first DacPac file in the merge.
    /// </summary>
    First,

    /// <summary>
    /// Use model options from the last DacPac file in the merge.
    /// </summary>
    Last,
}
