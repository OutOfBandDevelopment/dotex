using System.Collections.Generic;

namespace OoBDev.DataLoader;

/// <summary>Options for one load.</summary>
public sealed class LoadOptions
{
    /// <summary>Duplicate handling.</summary>
    public DuplicatePolicy Duplicates { get; set; } = DuplicatePolicy.Skip;

    /// <summary>Stop at the first rejected row.</summary>
    public bool StopOnError { get; set; }
}
